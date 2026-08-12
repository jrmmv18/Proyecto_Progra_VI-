/*=========================================================
  MODULO: MANTENIMIENTO
  Procedimientos tomados del respaldo GaleriaArteDB actualizada.bak
  sp_ListarMantenimientos ahora devuelve NombreObra, que es lo que
  espera MantenimientoRepository despues de la fusion.
=========================================================*/


CREATE OR ALTER PROCEDURE dbo.sp_ListarMantenimientos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        M.IdMantenimiento,
        M.IdObra,
        O.Nombre AS NombreObra,
        M.TipoMantenimiento,
        M.Observaciones AS DescripcionTrabajo,
        M.FechaMantenimiento,
        M.IdUsuario
    FROM dbo.Mantenimiento AS M
    INNER JOIN dbo.Obras AS O
        ON M.IdObra = O.IdObra
    ORDER BY M.FechaMantenimiento DESC;
END


GO


/*=========================================================
  PROCEDIMIENTO: REGISTRAR MANTENIMIENTO

  MÓDULO:
  Mantenimiento

  DESCRIPCIÓN:
  Registra la cabecera de un mantenimiento para una obra.

  VALIDACIONES:
  - La obra debe existir.
  - El usuario debe existir.
  - La fecha no puede ser futura.

  SEGURIDAD:
  - Usa transacción.
  - Registra auditoría en BitacoraMantenimiento.
  - Registra auditoría en BitacoraGeneral.
  - Registra errores en BitacoraErrores.
=========================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_RegistrarMantenimiento
(
    @IdObra INT,
    @TipoMantenimiento NVARCHAR(40),
    @Observaciones NVARCHAR(500)=NULL,
    @FechaMantenimiento DATETIME2,
    @IdUsuario INT
)
AS
BEGIN

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @IdMantenimiento INT;
DECLARE @RollbackEjecutado BIT = 0;

BEGIN TRY

    ----------------------------------------------------
    -- 1. Validar obra
    ----------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM Obras
        WHERE IdObra=@IdObra
    )
    BEGIN
        THROW 52001,
              'La obra indicada no existe.',
              1;
    END;

    ----------------------------------------------------
    -- 2. Validar usuario
    ----------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM Usuarios
        WHERE IdUsuario=@IdUsuario
    )
    BEGIN
        THROW 52002,
              'El usuario indicado no existe.',
              1;
    END;

    ----------------------------------------------------
    -- 3. Validar fecha
    ----------------------------------------------------

    IF @FechaMantenimiento > SYSDATETIME()
    BEGIN
        THROW 52003,
              'La fecha no puede ser futura.',
              1;
    END;

    ----------------------------------------------------
    -- 4. Iniciar transacción
    ----------------------------------------------------

    BEGIN TRANSACTION;

    ----------------------------------------------------
    -- 5. Registrar mantenimiento
    ----------------------------------------------------

    INSERT INTO Mantenimiento
    (
        IdObra,
        IdUsuario,
        TipoMantenimiento,
        FechaMantenimiento,
        Observaciones
    )
    VALUES
    (
        @IdObra,
        @IdUsuario,
        @TipoMantenimiento,
        @FechaMantenimiento,
        @Observaciones
    );

    SET @IdMantenimiento=SCOPE_IDENTITY();

    ----------------------------------------------------
    -- 6. Bitácora del módulo
    ----------------------------------------------------

    INSERT INTO BitacoraMantenimiento
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
        'sp_RegistrarMantenimiento',
        'INSERT',
        @IdMantenimiento,
        CONCAT(
            'Obra: ',@IdObra,
            ' | Tipo: ',@TipoMantenimiento
        ),
        SYSDATETIME(),
        'EXITOSO'
    );

    ----------------------------------------------------
    -- 7. Bitácora General
    ----------------------------------------------------

    INSERT INTO BitacoraGeneral
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
        'Mantenimiento',
        'Mantenimiento',
        'INSERT',
        @IdMantenimiento,
        NULL,
        CONCAT(
            'Obra: ',@IdObra,
            ' | Tipo: ',@TipoMantenimiento
        ),
        SYSDATETIME(),
        'EXITOSO'
    );

    ----------------------------------------------------
    -- 8. Confirmar
    ----------------------------------------------------

    COMMIT TRANSACTION;

    ----------------------------------------------------
    -- 9. Devolver ID
    ----------------------------------------------------

    SELECT @IdMantenimiento AS IdMantenimiento;

END TRY

BEGIN CATCH

    IF @@TRANCOUNT>0
    BEGIN
        ROLLBACK TRANSACTION;
        SET @RollbackEjecutado=1;
    END;

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
        'sp_RegistrarMantenimiento',
        ERROR_NUMBER(),
        ERROR_MESSAGE(),
        SYSDATETIME(),
        @RollbackEjecutado
    );

    THROW;

END CATCH

END


GO


CREATE OR ALTER PROCEDURE dbo.sp_Mantenimiento_RegistrarProducto
    @IdMantenimiento INT,
    @IdProducto INT,
    @CantidadUtilizada INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @IdMantenimiento IS NULL OR @IdMantenimiento <= 0
    BEGIN
        RAISERROR('El mantenimiento no es válido.', 16, 1);
        RETURN;
    END;

    IF @IdProducto IS NULL OR @IdProducto <= 0
    BEGIN
        RAISERROR('El producto no es válido.', 16, 1);
        RETURN;
    END;

    IF @CantidadUtilizada IS NULL OR @CantidadUtilizada <= 0
    BEGIN
        RAISERROR('La cantidad utilizada debe ser mayor que cero.', 16, 1);
        RETURN;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Mantenimiento
        WHERE IdMantenimiento = @IdMantenimiento
    )
    BEGIN
        RAISERROR('El mantenimiento no existe.', 16, 1);
        RETURN;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Productos
        WHERE IdProducto = @IdProducto
    )
    BEGIN
        RAISERROR('El producto no existe.', 16, 1);
        RETURN;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.DetalleMantenimiento
        WHERE IdMantenimiento = @IdMantenimiento
          AND IdProducto = @IdProducto
    )
    BEGIN
        RAISERROR(
            'El producto ya fue registrado en este mantenimiento.',
            16,
            1
        );
        RETURN;
    END;

    INSERT INTO dbo.DetalleMantenimiento
    (
        IdMantenimiento,
        IdProducto,
        CantidadUtilizada
    )
    VALUES
    (
        @IdMantenimiento,
        @IdProducto,
        @CantidadUtilizada
    );
END


GO

