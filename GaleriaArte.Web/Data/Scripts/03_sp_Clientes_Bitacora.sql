/*=========================================================
  MODULO: CLIENTES
  Se agrega bitacora a Crear y Actualizar, que antes no
  registraban nada. Cambiar estado ya lo hacia.

  Bitacoras:
  - BitacoraGeneral : alta y modificacion del cliente, igual
                      que hacen Artistas, Productos y Usuarios
  - BitacoraErrores : cualquier fallo

  Las validaciones y el comportamiento visible no cambian.
=========================================================*/


/*=========================================================
  PROCEDIMIENTO: CREAR CLIENTE
  MODULO: CLIENTES
  DESCRIPCION:
  Registra un cliente visitante y genera la auditoria.

  VALIDACIONES:
  - El nombre es obligatorio.
  - El apellido es obligatorio.
  - El correo no puede repetirse.

  SEGURIDAD:
  - Usa transaccion.
  - Registra operaciones exitosas en BitacoraGeneral.
  - Registra errores en BitacoraErrores.
=========================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_Clientes_Crear
    @Nombre NVARCHAR(50),
    @Apellido NVARCHAR(50),
    @Telefono VARCHAR(20) = NULL,
    @Correo VARCHAR(100) = NULL,
    @Direccion NVARCHAR(200) = NULL,
    @IdUsuario INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdCliente INT;
    DECLARE @RollbackEjecutado BIT = 0;

    BEGIN TRY

        /*---------------------------------------------
          1. Validar nombre
        ---------------------------------------------*/

        IF NULLIF(LTRIM(RTRIM(@Nombre)), '') IS NULL
        BEGIN
            THROW 50220,
                  'El nombre del cliente es obligatorio.',
                  1;
        END;


        /*---------------------------------------------
          2. Validar apellido
        ---------------------------------------------*/

        IF NULLIF(LTRIM(RTRIM(@Apellido)), '') IS NULL
        BEGIN
            THROW 50221,
                  'El apellido del cliente es obligatorio.',
                  1;
        END;


        /*---------------------------------------------
          3. Validar correo repetido
        ---------------------------------------------*/

        IF @Correo IS NOT NULL
           AND EXISTS
           (
               SELECT 1
               FROM dbo.Clientes
               WHERE Correo = @Correo
           )
        BEGIN
            THROW 50222,
                  'Ya existe un cliente registrado con ese correo.',
                  1;
        END;


        /*---------------------------------------------
          4. Insertar
        ---------------------------------------------*/

        BEGIN TRANSACTION;

        INSERT INTO dbo.Clientes
        (
            Nombre,
            Apellido,
            Telefono,
            Correo,
            Direccion,
            Estado,
            FechaRegistro
        )
        VALUES
        (
            LTRIM(RTRIM(@Nombre)),
            LTRIM(RTRIM(@Apellido)),
            NULLIF(LTRIM(RTRIM(@Telefono)), ''),
            NULLIF(LTRIM(RTRIM(@Correo)), ''),
            NULLIF(LTRIM(RTRIM(@Direccion)), ''),
            1,
            SYSDATETIME()
        );

        SET @IdCliente = CAST(SCOPE_IDENTITY() AS INT);


        /*---------------------------------------------
          5. Registrar auditoria
        ---------------------------------------------*/

        INSERT INTO dbo.BitacoraGeneral
        (
            IdUsuario,
            Modulo,
            TablaAfectada,
            TipoOperacion,
            RegistroAfectado,
            ValorAnterior,
            ValorNuevo,
            FechaHora,
            Resultado
        )
        VALUES
        (
            @IdUsuario,
            N'Clientes',
            N'Clientes',
            'INSERT',
            @IdCliente,
            NULL,
            CONCAT(
                N'Nombre: ', @Nombre,
                N' ', @Apellido,
                N' | Teléfono: ', ISNULL(@Telefono, N'-'),
                N' | Correo: ', ISNULL(@Correo, N'-'),
                N' | Estado: Activo'),
            SYSDATETIME(),
            N'EXITOSO'
        );

        COMMIT TRANSACTION;

        SELECT @IdCliente AS IdCliente;

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
            SET @RollbackEjecutado = 1;
        END;

        INSERT INTO dbo.BitacoraErrores
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
            N'sp_Clientes_Crear',
            ERROR_NUMBER(),
            ERROR_MESSAGE(),
            SYSDATETIME(),
            @RollbackEjecutado
        );

        THROW;

    END CATCH;
END;
GO


/*=========================================================
  PROCEDIMIENTO: ACTUALIZAR CLIENTE
  MODULO: CLIENTES
  DESCRIPCION:
  Modifica los datos de un cliente y deja constancia del
  valor anterior y del nuevo.

  VALIDACIONES:
  - El cliente debe existir.
  - El nombre es obligatorio.
  - El apellido es obligatorio.
  - El correo no puede repetirse en otro cliente.

  SEGURIDAD:
  - Usa transaccion.
  - Registra operaciones exitosas en BitacoraGeneral.
  - Registra errores en BitacoraErrores.
=========================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_Clientes_Actualizar
    @IdCliente INT,
    @Nombre NVARCHAR(50),
    @Apellido NVARCHAR(50),
    @Telefono VARCHAR(20) = NULL,
    @Correo VARCHAR(100) = NULL,
    @Direccion NVARCHAR(200) = NULL,
    @IdUsuario INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Anterior NVARCHAR(1000);
    DECLARE @RollbackEjecutado BIT = 0;

    BEGIN TRY

        /*---------------------------------------------
          1. Validar que el cliente exista y guardar
             como estaba antes del cambio
        ---------------------------------------------*/

        SELECT @Anterior =
            CONCAT(
                N'Nombre: ', Nombre,
                N' ', Apellido,
                N' | Teléfono: ', ISNULL(Telefono, N'-'),
                N' | Correo: ', ISNULL(Correo, N'-'),
                N' | Dirección: ', ISNULL(Direccion, N'-'))
        FROM dbo.Clientes
        WHERE IdCliente = @IdCliente;

        IF @Anterior IS NULL
        BEGIN
            THROW 50223,
                  'El cliente indicado no existe.',
                  1;
        END;


        /*---------------------------------------------
          2. Validar nombre
        ---------------------------------------------*/

        IF NULLIF(LTRIM(RTRIM(@Nombre)), '') IS NULL
        BEGIN
            THROW 50220,
                  'El nombre del cliente es obligatorio.',
                  1;
        END;


        /*---------------------------------------------
          3. Validar apellido
        ---------------------------------------------*/

        IF NULLIF(LTRIM(RTRIM(@Apellido)), '') IS NULL
        BEGIN
            THROW 50221,
                  'El apellido del cliente es obligatorio.',
                  1;
        END;


        /*---------------------------------------------
          4. Validar correo repetido
        ---------------------------------------------*/

        IF @Correo IS NOT NULL
           AND EXISTS
           (
               SELECT 1
               FROM dbo.Clientes
               WHERE Correo = @Correo
                 AND IdCliente <> @IdCliente
           )
        BEGIN
            THROW 50224,
                  'Ya existe otro cliente registrado con ese correo.',
                  1;
        END;


        /*---------------------------------------------
          5. Actualizar
        ---------------------------------------------*/

        BEGIN TRANSACTION;

        UPDATE dbo.Clientes
        SET
            Nombre = LTRIM(RTRIM(@Nombre)),
            Apellido = LTRIM(RTRIM(@Apellido)),
            Telefono = NULLIF(LTRIM(RTRIM(@Telefono)), ''),
            Correo = NULLIF(LTRIM(RTRIM(@Correo)), ''),
            Direccion = NULLIF(LTRIM(RTRIM(@Direccion)), '')
        WHERE IdCliente = @IdCliente;


        /*---------------------------------------------
          6. Registrar auditoria
        ---------------------------------------------*/

        INSERT INTO dbo.BitacoraGeneral
        (
            IdUsuario,
            Modulo,
            TablaAfectada,
            TipoOperacion,
            RegistroAfectado,
            ValorAnterior,
            ValorNuevo,
            FechaHora,
            Resultado
        )
        VALUES
        (
            @IdUsuario,
            N'Clientes',
            N'Clientes',
            'UPDATE',
            @IdCliente,
            @Anterior,
            CONCAT(
                N'Nombre: ', @Nombre,
                N' ', @Apellido,
                N' | Teléfono: ', ISNULL(@Telefono, N'-'),
                N' | Correo: ', ISNULL(@Correo, N'-'),
                N' | Dirección: ', ISNULL(@Direccion, N'-')),
            SYSDATETIME(),
            N'EXITOSO'
        );

        COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
            SET @RollbackEjecutado = 1;
        END;

        INSERT INTO dbo.BitacoraErrores
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
            N'sp_Clientes_Actualizar',
            ERROR_NUMBER(),
            ERROR_MESSAGE(),
            SYSDATETIME(),
            @RollbackEjecutado
        );

        THROW;

    END CATCH;
END;
GO
