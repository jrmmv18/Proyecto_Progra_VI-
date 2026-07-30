using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration; // Requerido para leer el appsettings.json
using GaleriaArte.Web.Models;

namespace GaleriaArte.Web.Data
{
    public class MantenimientoRepository : IMantenimientoRepository
    {
        private readonly string _connectionString;

        // El constructor resuelve de forma segura la variable _connectionString
        public MantenimientoRepository(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            // Mapeo exacto de la clave de tu appsettings.json
            _connectionString = configuration.GetConnectionString("GaleriaArteDB") ?? string.Empty;
        }

        // 1. MÉTODO PARA REGISTRAR EN REPOSITORIO (Blindado contra NOCOUNT)
        public bool RegistrarMantenimientoCompleto(Mantenimiento mantenimiento)
        {
            if (mantenimiento == null) return false;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("sp_RegistrarMantenimiento", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@IdObra", mantenimiento.IdObra);
                        cmd.Parameters.AddWithValue("@DescripcionTrabajo", mantenimiento.DescripcionTrabajo ?? (object)DBNull.Value);

                        // Evitar enviar DateTime.MinValue o fechas fuera del rango de SQL DATETIME.
                        // Si la fecha no está definida, enviamos DBNull para que el stored procedure pueda asignar GETDATE().
                        var fechaParam = new SqlParameter("@FechaMantenimiento", SqlDbType.DateTime);
                        if (mantenimiento.FechaMantenimiento == default(DateTime) || mantenimiento.FechaMantenimiento < new DateTime(1753, 1, 1))
                        {
                            fechaParam.Value = DBNull.Value;
                        }
                        else
                        {
                            fechaParam.Value = mantenimiento.FechaMantenimiento;
                        }
                        cmd.Parameters.Add(fechaParam);

                        cmd.Parameters.AddWithValue("@IdUsuario", mantenimiento.IdUsuario);

                        conn.Open();

                        // Ejecutamos el comando en la base de datos
                        cmd.ExecuteNonQuery();

                        conn.Close();

                        // Si llegó a esta línea sin saltar al 'catch', la inserción fue exitosa
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                throw; // Permite que el controlador capture cualquier error real de SQL
            }
        }

        // 2. MÉTODO PARA LISTAR EN REPOSITORIO (Actualizado a INT)
        public List<Mantenimiento> ListarTodos()
        {
            var lista = new List<Mantenimiento>();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("sp_ListarMantenimientos", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var mantenimiento = new Mantenimiento
                                {
                                    IdMantenimiento = reader["IdMantenimiento"] != DBNull.Value ? Convert.ToInt32(reader["IdMantenimiento"]) : 0,
                                    IdObra = reader["IdObra"] != DBNull.Value ? Convert.ToInt32(reader["IdObra"]) : 0,
                                    DescripcionTrabajo = reader["DescripcionTrabajo"] != DBNull.Value ? reader["DescripcionTrabajo"].ToString()! : string.Empty,
                                    FechaMantenimiento = reader["FechaMantenimiento"] != DBNull.Value ? Convert.ToDateTime(reader["FechaMantenimiento"]) : DateTime.Now,
                                    IdUsuario = reader["IdUsuario"] != DBNull.Value ? Convert.ToInt32(reader["IdUsuario"]) : 0
                                };
                                lista.Add(mantenimiento);
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception)
            {
                return new List<Mantenimiento>();
            }

            return lista;
        }
    }
}
