/*=========================================================
  MODULO: VISITAS
  DESCRIPCION:
  Procedimientos almacenados para el control de entradas
  y salidas de los visitantes de la galeria.

  TABLA: Visitas
  (IdVisita, IdCliente, FechaIngreso, FechaSalida, Observaciones)

  SEGURIDAD:
  - Las operaciones de escritura usan transaccion.
  - Las operaciones exitosas se registran en BitacoraVisitas.
  - Los errores se registran en BitacoraErrores.
=========================================================*/


/*=========================================================
  PROCEDIMIENTO: LISTAR VISITAS
  MODULO: VISITAS
  DESCRIPCION:
  Obtiene todas las visitas registradas junto con la
  informacion basica del visitante.
  No modifica datos.
=========================================================*/

CREATE OR ALTER PROCEDURE sp_Visitas_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        V.IdVisita,
        V.IdCliente,
        V.FechaIngreso,
        V.FechaSalida,
        V.Observaciones,
        C.Nombre AS NombreCliente,
        C.Apellido AS ApellidoCliente,
        C.Correo AS CorreoCliente,
        C.Telefono AS TelefonoCliente
    FROM Visitas AS V
    INNER JOIN Clientes AS C
        ON V.IdCliente = C.IdCliente
    ORDER BY
        CASE WHEN V.FechaSalida IS NULL THEN 0 ELSE 1 END,
        V.FechaIngreso DESC;
END;
GO


/*=========================================================
  PROCEDIMIENTO: OBTENER VISITA POR ID
  MODULO: VISITAS
  DESCRIPCION:
  Obtiene una visita especifica junto con la informacion
  basica del visitante.
  No modifica datos.
=========================================================*/

CREATE OR ALTER PROCEDURE sp_Visitas_ObtenerPorId
    @IdVisita INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        V.IdVisita,
        V.IdCliente,
        V.FechaIngreso,
        V.FechaSalida,
        V.Observaciones,
        C.Nombre AS NombreCliente,
        C.Apellido AS ApellidoCliente,
        C.Correo AS CorreoCliente,
        C.Telefono AS TelefonoCliente
    FROM Visitas AS V
    INNER JOIN Clientes AS C
        ON V.IdCliente = C.IdCliente
    WHERE V.IdVisita = @IdVisita;
END;
GO


/*=========================================================
  PROCEDIMIENTO: REGISTRAR ENTRADA
  MODULO: VISITAS
  DESCRIPCION:
  Registra la entrada de un visitante a la galeria y
  genera la auditoria correspondiente.

  VALIDACIONES:
  - El cliente debe existir.
  - El cliente debe estar activo.
  - La fecha de ingreso no puede ser futura.
  - El cliente no puede tener otra visita sin salida.

  SEGURIDAD:
  - Usa transaccion.
  - Registra operaciones exitosas en BitacoraVisitas.
  - Registra errores en BitacoraErrores.
=========================================================*/

CREATE OR ALTER PROCEDURE sp_Visitas_RegistrarEntrada
    @IdCliente INT,
    @FechaIngreso DATETIME2,
    @Observaciones NVARCHAR(500) = NULL,
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdVisita INT;
    DECLARE @RollbackEjecutado BIT = 0;

    BEGIN TRY

        /*---------------------------------------------
          1. Validar que el cliente exista
        ---------------------------------------------*/

        IF NOT EXISTS
        (
            SELECT 1
            FROM Clientes
            WHERE IdCliente = @IdCliente
        )
        BEGIN
            THROW 50101,
                  'El visitante indicado no existe.',
                  1;
        END;


        /*---------------------------------------------
          2. Validar que el cliente este activo
        ---------------------------------------------*/

        IF NOT EXISTS
        (
            SELECT 1
            FROM Clientes
            WHERE IdCliente = @IdCliente
              AND Estado = 1
        )
        BEGIN
            THROW 50102,
                  'El visitante indicado esta inactivo.',
                  1;
        END;


        /*---------------------------------------------
          3. Validar la fecha de ingreso
        ---------------------------------------------*/

        IF @FechaIngreso > SYSDATETIME()
        BEGIN
            THROW 50103,
                  'La fecha de entrada no puede ser futura.',
                  1;
        END;


        /*---------------------------------------------
          4. Validar que no tenga una visita en curso
        ---------------------------------------------*/

        IF EXISTS
        (
            SELECT 1
            FROM Visitas
            WHERE IdCliente = @IdCliente
              AND FechaSalida IS NULL
        )
        BEGIN
            THROW 50104,
                  'El visitante ya tiene una entrada sin salida registrada.',
                  1;
        END;


        /*---------------------------------------------
          5. Iniciar transaccion
        ---------------------------------------------*/

        BEGIN TRANSACTION;


        /*---------------------------------------------
          6. Insertar la visita
        ---------------------------------------------*/

        INSERT INTO Visitas
        (
            IdCliente,
            FechaIngreso,
            FechaSalida,
            Observaciones
        )
        VALUES
        (
            @IdCliente,
            @FechaIngreso,
            NULL,
            @Observaciones
        );


        /*---------------------------------------------
          7. Obtener ID generado
        ---------------------------------------------*/

        SET @IdVisita = SCOPE_IDENTITY();


        /*---------------------------------------------
          8. Registrar auditoria
        ---------------------------------------------*/

        INSERT INTO BitacoraVisitas
        (
            IdUsuario,
            Procedimiento,
            Operacion,
            RegistroAfectado,
            Detalle,
            FechaHora,
            Resultado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_Visitas_RegistrarEntrada',
            'INSERT',
            @IdVisita,
            CONCAT(
                N'IdCliente: ', @IdCliente,
                N' | Entrada: ', CONVERT(NVARCHAR(30), @FechaIngreso, 120)
            ),
            SYSDATETIME(),
            N'EXITOSO'
        );


        /*---------------------------------------------
          9. Confirmar transaccion
        ---------------------------------------------*/

        COMMIT TRANSACTION;


        /*---------------------------------------------
          10. Devolver visita creada
        ---------------------------------------------*/

        SELECT @IdVisita AS IdVisita;

    END TRY

    BEGIN CATCH

        /*---------------------------------------------
          11. Revertir si existe transaccion activa
        ---------------------------------------------*/

        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
            SET @RollbackEjecutado = 1;
        END;


        /*---------------------------------------------
          12. Registrar el error
        ---------------------------------------------*/

        INSERT INTO BitacoraErrores
        (
            IdUsuario,
            Procedimiento,
            NumeroError,
            Descripcion,
            FechaHora,
            RollbackEjecutado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_Visitas_RegistrarEntrada',
            ERROR_NUMBER(),
            ERROR_MESSAGE(),
            SYSDATETIME(),
            @RollbackEjecutado
        );


        /*---------------------------------------------
          13. Devolver el error a la aplicacion
        ---------------------------------------------*/

        THROW;

    END CATCH;
END;
GO


/*=========================================================
  PROCEDIMIENTO: REGISTRAR SALIDA
  MODULO: VISITAS
  DESCRIPCION:
  Sella la hora de salida de un visitante que se
  encuentra dentro de la galeria.

  VALIDACIONES:
  - La visita debe existir.
  - La visita no debe tener salida registrada.
  - La salida no puede ser anterior a la entrada.

  SEGURIDAD:
  - Usa transaccion.
  - Registra operaciones exitosas en BitacoraVisitas.
  - Registra errores en BitacoraErrores.
=========================================================*/

CREATE OR ALTER PROCEDURE sp_Visitas_RegistrarSalida
    @IdVisita INT,
    @FechaSalida DATETIME2,
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @FechaIngreso DATETIME2;
    DECLARE @SalidaActual DATETIME2;
    DECLARE @RollbackEjecutado BIT = 0;

    BEGIN TRY

        /*---------------------------------------------
          1. Validar que la visita exista
        ---------------------------------------------*/

        SELECT
            @FechaIngreso = FechaIngreso,
            @SalidaActual = FechaSalida
        FROM Visitas
        WHERE IdVisita = @IdVisita;

        IF @FechaIngreso IS NULL
        BEGIN
            THROW 50105,
                  'La visita indicada no existe.',
                  1;
        END;


        /*---------------------------------------------
          2. Validar que no tenga salida registrada
        ---------------------------------------------*/

        IF @SalidaActual IS NOT NULL
        BEGIN
            THROW 50106,
                  'La salida de este visitante ya fue registrada.',
                  1;
        END;


        /*---------------------------------------------
          3. Validar el orden de las fechas
        ---------------------------------------------*/

        IF @FechaSalida < @FechaIngreso
        BEGIN
            THROW 50107,
                  'La salida no puede ser anterior a la entrada.',
                  1;
        END;


        /*---------------------------------------------
          4. Iniciar transaccion
        ---------------------------------------------*/

        BEGIN TRANSACTION;


        /*---------------------------------------------
          5. Registrar la salida
        ---------------------------------------------*/

        UPDATE Visitas
        SET FechaSalida = @FechaSalida
        WHERE IdVisita = @IdVisita;


        /*---------------------------------------------
          6. Registrar auditoria
        ---------------------------------------------*/

        INSERT INTO BitacoraVisitas
        (
            IdUsuario,
            Procedimiento,
            Operacion,
            RegistroAfectado,
            Detalle,
            FechaHora,
            Resultado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_Visitas_RegistrarSalida',
            'UPDATE',
            @IdVisita,
            CONCAT(
                N'Entrada: ', CONVERT(NVARCHAR(30), @FechaIngreso, 120),
                N' | Salida: ', CONVERT(NVARCHAR(30), @FechaSalida, 120),
                N' | Minutos: ', DATEDIFF(MINUTE, @FechaIngreso, @FechaSalida)
            ),
            SYSDATETIME(),
            N'EXITOSO'
        );


        /*---------------------------------------------
          7. Confirmar transaccion
        ---------------------------------------------*/

        COMMIT TRANSACTION;


        /*---------------------------------------------
          8. Confirmar la operacion a la aplicacion
        ---------------------------------------------*/

        SELECT CAST(1 AS BIT) AS Resultado;

    END TRY

    BEGIN CATCH

        /*---------------------------------------------
          9. Revertir si existe transaccion activa
        ---------------------------------------------*/

        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
            SET @RollbackEjecutado = 1;
        END;


        /*---------------------------------------------
          10. Registrar el error
        ---------------------------------------------*/

        INSERT INTO BitacoraErrores
        (
            IdUsuario,
            Procedimiento,
            NumeroError,
            Descripcion,
            FechaHora,
            RollbackEjecutado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_Visitas_RegistrarSalida',
            ERROR_NUMBER(),
            ERROR_MESSAGE(),
            SYSDATETIME(),
            @RollbackEjecutado
        );


        /*---------------------------------------------
          11. Devolver el error a la aplicacion
        ---------------------------------------------*/

        THROW;

    END CATCH;
END;
GO


/*=========================================================
  PROCEDIMIENTO: ACTUALIZAR VISITA
  MODULO: VISITAS
  DESCRIPCION:
  Corrige los datos de una visita ya registrada.

  VALIDACIONES:
  - La visita debe existir.
  - El cliente debe existir.
  - El cliente debe estar activo.
  - La fecha de ingreso no puede ser futura.
  - La salida no puede ser anterior a la entrada.

  SEGURIDAD:
  - Usa transaccion.
  - Registra operaciones exitosas en BitacoraVisitas.
  - Registra errores en BitacoraErrores.
=========================================================*/

CREATE OR ALTER PROCEDURE sp_Visitas_Actualizar
    @IdVisita INT,
    @IdCliente INT,
    @FechaIngreso DATETIME2,
    @FechaSalida DATETIME2 = NULL,
    @Observaciones NVARCHAR(500) = NULL,
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @DetalleAnterior NVARCHAR(1000);
    DECLARE @RollbackEjecutado BIT = 0;

    BEGIN TRY

        /*---------------------------------------------
          1. Validar que la visita exista
        ---------------------------------------------*/

        SELECT @DetalleAnterior =
            CONCAT(
                N'IdCliente: ', IdCliente,
                N' | Entrada: ', CONVERT(NVARCHAR(30), FechaIngreso, 120),
                N' | Salida: ', ISNULL(CONVERT(NVARCHAR(30), FechaSalida, 120), N'Sin registrar')
            )
        FROM Visitas
        WHERE IdVisita = @IdVisita;

        IF @DetalleAnterior IS NULL
        BEGIN
            THROW 50105,
                  'La visita indicada no existe.',
                  1;
        END;


        /*---------------------------------------------
          2. Validar que el cliente exista
        ---------------------------------------------*/

        IF NOT EXISTS
        (
            SELECT 1
            FROM Clientes
            WHERE IdCliente = @IdCliente
        )
        BEGIN
            THROW 50101,
                  'El visitante indicado no existe.',
                  1;
        END;


        /*---------------------------------------------
          3. Validar que el cliente este activo
        ---------------------------------------------*/

        IF NOT EXISTS
        (
            SELECT 1
            FROM Clientes
            WHERE IdCliente = @IdCliente
              AND Estado = 1
        )
        BEGIN
            THROW 50102,
                  'El visitante indicado esta inactivo.',
                  1;
        END;


        /*---------------------------------------------
          4. Validar la fecha de ingreso
        ---------------------------------------------*/

        IF @FechaIngreso > SYSDATETIME()
        BEGIN
            THROW 50103,
                  'La fecha de entrada no puede ser futura.',
                  1;
        END;


        /*---------------------------------------------
          5. Validar el orden de las fechas
        ---------------------------------------------*/

        IF @FechaSalida IS NOT NULL
           AND @FechaSalida < @FechaIngreso
        BEGIN
            THROW 50107,
                  'La salida no puede ser anterior a la entrada.',
                  1;
        END;


        /*---------------------------------------------
          6. Iniciar transaccion
        ---------------------------------------------*/

        BEGIN TRANSACTION;


        /*---------------------------------------------
          7. Actualizar la visita
        ---------------------------------------------*/

        UPDATE Visitas
        SET IdCliente = @IdCliente,
            FechaIngreso = @FechaIngreso,
            FechaSalida = @FechaSalida,
            Observaciones = @Observaciones
        WHERE IdVisita = @IdVisita;


        /*---------------------------------------------
          8. Registrar auditoria
        ---------------------------------------------*/

        INSERT INTO BitacoraVisitas
        (
            IdUsuario,
            Procedimiento,
            Operacion,
            RegistroAfectado,
            Detalle,
            FechaHora,
            Resultado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_Visitas_Actualizar',
            'UPDATE',
            @IdVisita,
            CONCAT(
                N'Anterior -> ', @DetalleAnterior,
                N' || Nuevo -> IdCliente: ', @IdCliente,
                N' | Entrada: ', CONVERT(NVARCHAR(30), @FechaIngreso, 120),
                N' | Salida: ', ISNULL(CONVERT(NVARCHAR(30), @FechaSalida, 120), N'Sin registrar')
            ),
            SYSDATETIME(),
            N'EXITOSO'
        );


        /*---------------------------------------------
          9. Confirmar transaccion
        ---------------------------------------------*/

        COMMIT TRANSACTION;


        /*---------------------------------------------
          10. Confirmar la operacion a la aplicacion
        ---------------------------------------------*/

        SELECT CAST(1 AS BIT) AS Resultado;

    END TRY

    BEGIN CATCH

        /*---------------------------------------------
          11. Revertir si existe transaccion activa
        ---------------------------------------------*/

        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
            SET @RollbackEjecutado = 1;
        END;


        /*---------------------------------------------
          12. Registrar el error
        ---------------------------------------------*/

        INSERT INTO BitacoraErrores
        (
            IdUsuario,
            Procedimiento,
            NumeroError,
            Descripcion,
            FechaHora,
            RollbackEjecutado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_Visitas_Actualizar',
            ERROR_NUMBER(),
            ERROR_MESSAGE(),
            SYSDATETIME(),
            @RollbackEjecutado
        );


        /*---------------------------------------------
          13. Devolver el error a la aplicacion
        ---------------------------------------------*/

        THROW;

    END CATCH;
END;
GO
