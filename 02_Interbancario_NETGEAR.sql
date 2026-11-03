/* ============================================================================
   SCRIPT INTERBANCARIO ADAPTADO A CajeroFase2 (BD_CajeroRed_Actualizada)
   Protocolo NETGEAR 2.2.0: Descubrimiento UDP 5005 + Operaciones TCP 5000 (IB_*)
   ============================================================================ */
USE BD_CajeroRed_Actualizada;
GO

-- 1. Parámetros de identidad interbancaria de nuestro banco
IF NOT EXISTS (SELECT 1 FROM dbo.Parametro WHERE Nombre = 'MI_CODIGO_BANCO')
    INSERT INTO dbo.Parametro (Nombre, Valor, Descripcion)
    VALUES ('MI_CODIGO_BANCO', 'BANC03', 'Código único de nuestro banco en la red interbancaria');

IF NOT EXISTS (SELECT 1 FROM dbo.Parametro WHERE Nombre = 'MI_NOMBRE_BANCO')
    INSERT INTO dbo.Parametro (Nombre, Valor, Descripcion)
    VALUES ('MI_NOMBRE_BANCO', 'CAJERORED', 'Nombre de nuestro banco (sin | ni saltos de línea)');

IF NOT EXISTS (SELECT 1 FROM dbo.Parametro WHERE Nombre = 'CUPO_DIARIO_IB')
    INSERT INTO dbo.Parametro (Nombre, Valor, Descripcion)
    VALUES ('CUPO_DIARIO_IB', '25000.00', 'Cupo diario total para transferencias interbancarias');
GO

-- 2. Tabla de Bancos Externos de la Red (Descubiertos por UDP 5005 o registrados manualmente)
IF OBJECT_ID('dbo.BancosRed', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BancosRed (
        CodigoBanco NVARCHAR(20) NOT NULL PRIMARY KEY,
        NombreBanco NVARCHAR(100) NOT NULL,
        IP NVARCHAR(45) NOT NULL,
        PuertoTcp INT NOT NULL DEFAULT 5000,
        PuertoUdp INT NOT NULL DEFAULT 5005,
        LimiteDiario DECIMAL(12,2) NOT NULL DEFAULT 25000.00,
        TransferidoHoy DECIMAL(12,2) NOT NULL DEFAULT 0.00,
        Activo BIT NOT NULL DEFAULT 1,
        Autorizado BIT NOT NULL DEFAULT 1,
        UltimoVisto DATETIME NOT NULL DEFAULT GETDATE()
    );

    INSERT INTO dbo.BancosRed (CodigoBanco, NombreBanco, IP, PuertoTcp, PuertoUdp, LimiteDiario, TransferidoHoy, Activo, Autorizado)
    VALUES ('BANC02', 'NETGEAR', '192.168.0.34', 5000, 5005, 25000.00, 0.00, 1, 1);
END
GO

-- 3. Tabla de Transferencias Interbancarias (Entrantes y Salientes)
-- Clave única persistida: (BancoOrigen, TokenTransferencia)
IF OBJECT_ID('dbo.TransferenciasInterbancarias', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TransferenciasInterbancarias (
        IdTransferencia INT IDENTITY(1,1) PRIMARY KEY,
        Direccion NVARCHAR(10) NOT NULL CHECK (Direccion IN ('ENTRANTE', 'SALIENTE')),
        TokenTransferencia NVARCHAR(60) NOT NULL,
        IdMensaje NVARCHAR(60) NULL,
        BancoOrigen NVARCHAR(20) NOT NULL,
        CuentaOrigen NVARCHAR(30) NOT NULL,
        NombreOrigen NVARCHAR(100) NOT NULL,
        BancoDestino NVARCHAR(20) NOT NULL,
        CuentaDestino NVARCHAR(30) NOT NULL,
        NombreDestino NVARCHAR(100) NOT NULL,
        IdUsuarioLocal INT NULL REFERENCES dbo.Usuario(IdUsuario),
        Monto DECIMAL(12,2) NOT NULL CHECK (Monto > 0),
        Concepto NVARCHAR(200) NULL,
        HuellaContenido NVARCHAR(300) NOT NULL,
        CodigoRespuesta NVARCHAR(5) NOT NULL,
        Estado NVARCHAR(20) NOT NULL, -- 'CONFIRMADA', 'RESERVADA', 'RECHAZADA', 'INCIERTA'
        CupoRestante DECIMAL(12,2) NULL,
        IpRemota NVARCHAR(45) NULL,
        Fecha DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UX_Transferencia_BancoToken UNIQUE (BancoOrigen, TokenTransferencia)
    );

    CREATE INDEX IX_TransferenciasIB_Fecha ON dbo.TransferenciasInterbancarias(Fecha DESC);
END
GO

-- 4. Procedimiento Almacenado para IB_TRANSFERIR entrante (en UNA sola transacción local)
CREATE OR ALTER PROCEDURE dbo.sp_RecibirTransferenciaIB
    @IdMensaje NVARCHAR(60),
    @TokenTransferencia NVARCHAR(60),
    @BancoOrigen NVARCHAR(20),
    @CuentaOrigen NVARCHAR(30),
    @NombreOrigen NVARCHAR(100),
    @BancoDestino NVARCHAR(20),
    @CuentaDestino NVARCHAR(30),
    @Monto DECIMAL(12,2),
    @Concepto NVARCHAR(200),
    @IpRemota NVARCHAR(45),
    @CodigoOut NVARCHAR(5) OUTPUT,
    @MensajeOut NVARCHAR(200) OUTPUT,
    @NombreDestinoOut NVARCHAR(100) OUTPUT,
    @CupoRestanteOut DECIMAL(12,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @CodigoOut = '96';
    SET @MensajeOut = 'No confirmado';
    SET @NombreDestinoOut = '';
    SET @CupoRestanteOut = 0;

    DECLARE @MiCodigo NVARCHAR(20) = ISNULL((SELECT Valor FROM dbo.Parametro WHERE Nombre = 'MI_CODIGO_BANCO'), 'BANC03');
    DECLARE @CupoDiario DECIMAL(12,2) = ISNULL(TRY_CAST((SELECT Valor FROM dbo.Parametro WHERE Nombre = 'CUPO_DIARIO_IB') AS DECIMAL(12,2)), 25000.00);

    DECLARE @HuellaNueva NVARCHAR(300) = CONCAT(
        UPPER(LTRIM(RTRIM(@BancoOrigen))), '|', LTRIM(RTRIM(@CuentaOrigen)), '|',
        UPPER(LTRIM(RTRIM(@BancoDestino))), '|', LTRIM(RTRIM(@CuentaDestino)), '|',
        CONVERT(VARCHAR(30), CAST(@Monto AS DECIMAL(12,2)))
    );

    BEGIN TRY
        BEGIN TRANSACTION;

        -- A. Idempotencia y detección de conflicto "94"
        DECLARE @HuellaExistente NVARCHAR(300), @CodExistente NVARCHAR(5), @NomDestExistente NVARCHAR(100), @CupoExistente DECIMAL(12,2);

        SELECT 
            @HuellaExistente = HuellaContenido,
            @CodExistente = CodigoRespuesta,
            @NomDestExistente = NombreDestino,
            @CupoExistente = ISNULL(CupoRestante, 0)
        FROM dbo.TransferenciasInterbancarias WITH (UPDLOCK, HOLDLOCK)
        WHERE UPPER(BancoOrigen) = UPPER(LTRIM(RTRIM(@BancoOrigen)))
          AND TokenTransferencia = LTRIM(RTRIM(@TokenTransferencia));

        IF @HuellaExistente IS NOT NULL
        BEGIN
            IF @HuellaExistente = @HuellaNueva AND @CodExistente = '00'
            BEGIN
                SET @CodigoOut = '00';
                SET @MensajeOut = 'Transferencia acreditada';
                SET @NombreDestinoOut = @NomDestExistente;
                SET @CupoRestanteOut = @CupoExistente;
                COMMIT TRANSACTION;
                RETURN;
            END
            ELSE
            BEGIN
                SET @CodigoOut = '94';
                SET @MensajeOut = 'Referencia en conflicto: el mismo token fue enviado con datos distintos';
                COMMIT TRANSACTION;
                RETURN;
            END
        END

        -- B. Validar que bancoDestino sea el nuestro
        IF UPPER(LTRIM(RTRIM(@BancoDestino))) <> UPPER(LTRIM(RTRIM(@MiCodigo)))
        BEGIN
            SET @CodigoOut = '30';
            SET @MensajeOut = 'El bancoDestino no corresponde a este banco';
            COMMIT TRANSACTION;
            RETURN;
        END

        -- C. Validar autorización del banco origen e IP del socket TCP
        IF NOT EXISTS (
            SELECT 1 FROM dbo.BancosRed
            WHERE UPPER(CodigoBanco) = UPPER(LTRIM(RTRIM(@BancoOrigen)))
              AND Activo = 1
              AND Autorizado = 1
              AND (IP = @IpRemota OR @IpRemota = '127.0.0.1' OR IP = '0.0.0.0')
        )
        BEGIN
            SET @CodigoOut = '57';
            SET @MensajeOut = 'Banco origen o dirección IP no autorizados';
            COMMIT TRANSACTION;
            RETURN;
        END

        -- D. Validar monto positivo
        IF @Monto IS NULL OR @Monto <= 0
        BEGIN
            SET @CodigoOut = '30';
            SET @MensajeOut = 'Monto inválido';
            COMMIT TRANSACTION;
            RETURN;
        END

        -- E. Buscar cuenta destino activa (por NumeroTarjeta o por IdUsuario)
        DECLARE @IdUsuarioDestino INT = NULL;
        SELECT TOP 1
            @IdUsuarioDestino = u.IdUsuario,
            @NombreDestinoOut = u.Nombre
        FROM dbo.Usuario u WITH (UPDLOCK, HOLDLOCK)
        LEFT JOIN dbo.Tarjeta t ON t.IdUsuario = u.IdUsuario AND t.Activa = 1
        WHERE u.Estado = 'Activo'
          AND (t.NumeroTarjeta = LTRIM(RTRIM(@CuentaDestino)) OR CAST(u.IdUsuario AS NVARCHAR(30)) = LTRIM(RTRIM(@CuentaDestino)));

        IF @IdUsuarioDestino IS NULL
        BEGIN
            SET @CodigoOut = '14';
            SET @MensajeOut = 'Cuenta inexistente o inactiva';
            COMMIT TRANSACTION;
            RETURN;
        END

        -- F. Validar cupo diario disponible
        DECLARE @UsadoHoy DECIMAL(12,2) = ISNULL((
            SELECT SUM(Monto)
            FROM dbo.TransferenciasInterbancarias WITH (UPDLOCK, HOLDLOCK)
            WHERE CodigoRespuesta = '00'
              AND CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)
        ), 0.00);

        IF (@UsadoHoy + @Monto) > @CupoDiario
        BEGIN
            SET @CodigoOut = '65';
            SET @MensajeOut = 'Cupo diario insuficiente';
            SET @CupoRestanteOut = CASE WHEN @CupoDiario > @UsadoHoy THEN @CupoDiario - @UsadoHoy ELSE 0 END;
            COMMIT TRANSACTION;
            RETURN;
        END

        -- G. Acreditar saldo local, guardar transferencia y actualizar contadores
        UPDATE dbo.Usuario
        SET Saldo = Saldo + @Monto
        WHERE IdUsuario = @IdUsuarioDestino;

        SET @CupoRestanteOut = @CupoDiario - (@UsadoHoy + @Monto);

        INSERT INTO dbo.TransferenciasInterbancarias (
            Direccion, TokenTransferencia, IdMensaje, BancoOrigen, CuentaOrigen, NombreOrigen,
            BancoDestino, CuentaDestino, NombreDestino, IdUsuarioLocal, Monto, Concepto,
            HuellaContenido, CodigoRespuesta, Estado, CupoRestante, IpRemota
        )
        VALUES (
            'ENTRANTE', LTRIM(RTRIM(@TokenTransferencia)), @IdMensaje, LTRIM(RTRIM(@BancoOrigen)), LTRIM(RTRIM(@CuentaOrigen)), ISNULL(@NombreOrigen, ''),
            LTRIM(RTRIM(@BancoDestino)), LTRIM(RTRIM(@CuentaDestino)), @NombreDestinoOut, @IdUsuarioDestino, @Monto, @Concepto,
            @HuellaNueva, '00', 'CONFIRMADA', @CupoRestanteOut, @IpRemota
        );

        UPDATE dbo.BancosRed
        SET TransferidoHoy = TransferidoHoy + @Monto,
            UltimoVisto = GETDATE()
        WHERE UPPER(CodigoBanco) = UPPER(LTRIM(RTRIM(@BancoOrigen)));

        INSERT INTO dbo.Bitacora (Origen, Accion, Detalle)
        VALUES ('Servidor', 'IB_TRANSFERIR_RECIBIDO',
                CONCAT('Token: ', @TokenTransferencia, ' | Origen: ', @BancoOrigen, ' (', @CuentaOrigen, ') | Destino: ', @CuentaDestino, ' | Monto: Q.', @Monto));

        SET @CodigoOut = '00';
        SET @MensajeOut = 'Transferencia acreditada';

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @CodigoOut = '96';
        SET @MensajeOut = ERROR_MESSAGE();
    END CATCH
END
GO
