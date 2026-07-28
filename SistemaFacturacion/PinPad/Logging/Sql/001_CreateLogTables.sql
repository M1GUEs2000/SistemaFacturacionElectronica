-- =====================================================================
-- Base de datos de auditoría para DF_PinPad.Wrapper
-- Cubre las 5 operaciones: ConsultaConfiguracionBasica, ConsultaTarjeta,
-- LecturaTarjeta, ProcesoPago, Anulacion (ProcesoControl).
-- =====================================================================
IF DB_ID('PinPadLogs') IS NULL
BEGIN
    PRINT 'Crea la base de datos PinPadLogs manualmente antes de ejecutar este script,';
    PRINT 'o descomenta la siguiente línea si tienes permisos:';
    -- CREATE DATABASE PinPadLogs;
END
GO

-- =====================================================================
-- Tabla CABECERA: una fila por cada operación ejecutada, sin importar el tipo.
-- Es el registro de auditoría principal (quién, cuándo, qué operación, resultado).
-- =====================================================================
IF OBJECT_ID('dbo.PinPad_Transacciones', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PinPad_Transacciones
    (
        Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,
        FechaHoraInicio     DATETIME2(3)      NOT NULL DEFAULT SYSDATETIME(),
        FechaHoraFin        DATETIME2(3)      NULL,

        -- 'ConsultaConfiguracionBasica' | 'ConsultaTarjeta' | 'LecturaTarjeta'
        -- | 'ProcesoPago' | 'Anulacion'
        TipoOperacion       VARCHAR(50)       NOT NULL,

        -- Según la especificación oficial de Datafast estos son 2 campos DISTINTOS de
        -- la trama de respuesta — antes los mezclábamos en una sola columna:
        --   "Código de respuesta de mensaje"    -> generado por el PIN PAD (nivel LAN):
        --       00=Ejecución exitosa, 01=Error en trama, 02=Realiza Inicio de Día,
        --       20=Error durante proceso, ER=Error conexión Pinpad
        --   "Código de respuesta del autorizador" -> enviado por el BANCO/autorizador
        --       (00=Aprobada, 51=Sin cupo, 54=Tarjeta vencida, 89=Terminal inválido, etc.
        --        ver catálogo completo en dbo.PinPad_CatalogoCodigosRespuesta)
        CodigoRespuesta     VARCHAR(10)       NULL,   -- nivel LAN (pin pad)
        CodigoRespuestaAut  VARCHAR(10)       NULL,   -- nivel autorizador (banco) — el que importa para saber si el pago fue aprobado o no
        MensajeRespuesta    VARCHAR(200)      NULL,
        Exitoso             BIT               NOT NULL DEFAULT 0,
        ExcepcionMensaje    NVARCHAR(MAX)     NULL,

        -- Número REAL de factura del POS (confirmado por el backend de facturación
        -- DESPUÉS de que el pago ya se procesó — por eso se llena con un UPDATE
        -- posterior, no en el INSERT inicial). El Id interno (arriba) sigue siendo
        -- el autoincremental de siempre; esta columna es solo para poder BUSCAR
        -- la transacción por el número de factura del POS.
        NumeroFactura       VARCHAR(50)       NULL,

        CajaID              VARCHAR(10)       NULL,
        UsuarioSistema      VARCHAR(100)      NULL,
        MaquinaOrigen       VARCHAR(100)      NULL
    );

    CREATE INDEX IX_PinPad_Transacciones_Fecha ON dbo.PinPad_Transacciones (FechaHoraInicio);
    CREATE INDEX IX_PinPad_Transacciones_Tipo  ON dbo.PinPad_Transacciones (TipoOperacion);
END
GO

-- Migración idempotente: agrega la columna si la tabla ya existía de una versión anterior
IF COL_LENGTH('dbo.PinPad_Transacciones', 'NumeroFactura') IS NULL
    ALTER TABLE dbo.PinPad_Transacciones ADD NumeroFactura VARCHAR(50) NULL;
GO

IF COL_LENGTH('dbo.PinPad_Transacciones', 'CodigoRespuestaAut') IS NULL
    ALTER TABLE dbo.PinPad_Transacciones ADD CodigoRespuestaAut VARCHAR(10) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PinPad_Transacciones_NumeroFactura')
    CREATE INDEX IX_PinPad_Transacciones_NumeroFactura ON dbo.PinPad_Transacciones (NumeroFactura);
GO

-- =====================================================================
-- Detalle específico de ProcesoPago (campos reales de RespuestaProcesoPago)
-- =====================================================================
IF OBJECT_ID('dbo.PinPad_DetallePago', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PinPad_DetallePago
    (
        Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,
        TransaccionId       BIGINT            NOT NULL,

        -- Solicitud
        Monto               DECIMAL(18,2)     NULL,
        TipoTransaccion     VARCHAR(50)       NULL,   -- Corriente / Diferido / etc.
        Red                 VARCHAR(50)       NULL,   -- Datafast / Medianet / Austro

        -- Respuesta
        CodigoRespuestaAut  VARCHAR(10)       NULL,
        MensajeRespuestaAut VARCHAR(200)      NULL,
        RedAdquirente       VARCHAR(50)       NULL,
        Referencia          VARCHAR(30)       NULL,
        Lote                VARCHAR(20)       NULL,
        Autorizacion        VARCHAR(20)       NULL,
        TID                 VARCHAR(20)       NULL,
        MID                 VARCHAR(20)       NULL,
        CodigoAdquirente    VARCHAR(20)       NULL,
        NombreAdquirente    VARCHAR(100)      NULL,
        NombreGrupoTarjeta  VARCHAR(100)      NULL,
        ModoLectura         VARCHAR(50)       NULL,
        TarjetaHabiente     VARCHAR(100)      NULL,
        NumeroTarjeta       NVARCHAR(300)     NULL,   -- puede venir tokenizado/encriptado (no siempre enmascarado simple)
        FechaVencimiento    VARCHAR(10)       NULL,

        -- Campos adicionales de la especificación oficial Datafast Fast Track,
        -- ya expuestos por la DLL pero que antes no persistíamos:
        NumeroTarjetaEncriptado NVARCHAR(500) NULL,   -- "Número de Tarjeta Encriptado" (PAN encriptado)
        ValorInteres        DECIMAL(18,2)     NULL,   -- "Valor del interés o financiamiento" (solo diferidos con interés)
        MensajePublicidad   VARCHAR(80)       NULL,   -- "Mensaje para impresión de premios o publicidad"
        AplicacionEMV       VARCHAR(20)       NULL,   -- "Identificación de APLICACIÓN EMV" (solo lectura chip)
        AID                 VARCHAR(20)       NULL,   -- "AID-EMV" (solo lectura chip)
        Criptograma         VARCHAR(22)       NULL,   -- "Tipo de Criptograma y valor - EMV"
        VerificacionPIN     VARCHAR(15)       NULL,   -- "Verificación de PIN" (solo lectura chip)
        ARQC                VARCHAR(16)       NULL,   -- solo lectura chip
        TVR                 VARCHAR(10)       NULL,   -- solo lectura chip
        TSI                 VARCHAR(4)        NULL,   -- solo lectura chip

        CONSTRAINT FK_PinPad_DetallePago_Transacciones
            FOREIGN KEY (TransaccionId) REFERENCES dbo.PinPad_Transacciones(Id)
    );
END
GO

-- Migración idempotente: ensancha la columna si la tabla ya existía con un tamaño menor
-- (el dato de tarjeta puede venir tokenizado/encriptado, mucho más largo que un número
-- de tarjeta enmascarado tradicional).
ALTER TABLE dbo.PinPad_DetallePago ALTER COLUMN NumeroTarjeta NVARCHAR(300) NULL;
GO
IF COL_LENGTH('dbo.PinPad_DetallePago', 'FechaVencimiento') IS NULL
    ALTER TABLE dbo.PinPad_DetallePago ADD FechaVencimiento VARCHAR(10) NULL;
GO
IF COL_LENGTH('dbo.PinPad_DetallePago', 'NumeroTarjetaEncriptado') IS NULL
    ALTER TABLE dbo.PinPad_DetallePago ADD NumeroTarjetaEncriptado NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.PinPad_DetallePago', 'ValorInteres') IS NULL
    ALTER TABLE dbo.PinPad_DetallePago ADD ValorInteres DECIMAL(18,2) NULL;
GO
IF COL_LENGTH('dbo.PinPad_DetallePago', 'MensajePublicidad') IS NULL
    ALTER TABLE dbo.PinPad_DetallePago ADD MensajePublicidad VARCHAR(80) NULL;
GO
IF COL_LENGTH('dbo.PinPad_DetallePago', 'AplicacionEMV') IS NULL
    ALTER TABLE dbo.PinPad_DetallePago ADD AplicacionEMV VARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.PinPad_DetallePago', 'AID') IS NULL
    ALTER TABLE dbo.PinPad_DetallePago ADD AID VARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.PinPad_DetallePago', 'Criptograma') IS NULL
    ALTER TABLE dbo.PinPad_DetallePago ADD Criptograma VARCHAR(22) NULL;
GO
IF COL_LENGTH('dbo.PinPad_DetallePago', 'VerificacionPIN') IS NULL
    ALTER TABLE dbo.PinPad_DetallePago ADD VerificacionPIN VARCHAR(15) NULL;
GO
IF COL_LENGTH('dbo.PinPad_DetallePago', 'ARQC') IS NULL
    ALTER TABLE dbo.PinPad_DetallePago ADD ARQC VARCHAR(16) NULL;
GO
IF COL_LENGTH('dbo.PinPad_DetallePago', 'TVR') IS NULL
    ALTER TABLE dbo.PinPad_DetallePago ADD TVR VARCHAR(10) NULL;
GO
IF COL_LENGTH('dbo.PinPad_DetallePago', 'TSI') IS NULL
    ALTER TABLE dbo.PinPad_DetallePago ADD TSI VARCHAR(4) NULL;
GO

-- =====================================================================
-- Detalle específico de ConsultaTarjeta / LecturaTarjeta
-- (campos reales de RespuestaConsultaTarjeta / RespuestaLecturaTarjeta)
-- =====================================================================
IF OBJECT_ID('dbo.PinPad_DetalleTarjeta', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PinPad_DetalleTarjeta
    (
        Id                      BIGINT IDENTITY(1,1) PRIMARY KEY,
        TransaccionId           BIGINT            NOT NULL,

        NumeroTarjeta           NVARCHAR(300)     NULL,  -- puede venir tokenizado/encriptado (no siempre enmascarado simple)
        NumeroTarjetaEncriptado NVARCHAR(500)     NULL,
        BinTarjeta              VARCHAR(20)       NULL,
        FechaVencimiento        VARCHAR(10)       NULL,
        RedAdquirienteCorriente VARCHAR(50)       NULL,  -- solo aplica a LecturaTarjeta
        RedAdquirienteDiferido  VARCHAR(50)       NULL,  -- solo aplica a LecturaTarjeta

        CONSTRAINT FK_PinPad_DetalleTarjeta_Transacciones
            FOREIGN KEY (TransaccionId) REFERENCES dbo.PinPad_Transacciones(Id)
    );
END
GO

-- Migración idempotente: ensancha las columnas si la tabla ya existía con un tamaño menor.
ALTER TABLE dbo.PinPad_DetalleTarjeta ALTER COLUMN NumeroTarjeta NVARCHAR(300) NULL;
GO
ALTER TABLE dbo.PinPad_DetalleTarjeta ALTER COLUMN NumeroTarjetaEncriptado NVARCHAR(500) NULL;
GO

-- =====================================================================
-- Detalle específico de Anulación (ProcesoControl)
-- =====================================================================
IF OBJECT_ID('dbo.PinPad_DetalleAnulacion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PinPad_DetalleAnulacion
    (
        Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
        TransaccionId   BIGINT            NOT NULL,

        LoteOriginal        VARCHAR(20)   NULL,       -- ya no se usa (flujo antiguo vía ProcesoControl), se deja por compatibilidad
        ReferenciaOriginal  VARCHAR(30)   NULL,
        AutorizacionOriginal VARCHAR(20)  NULL,        -- dato real que identifica la transacción a anular
        RedAdquirente       VARCHAR(50)   NULL,

        CONSTRAINT FK_PinPad_DetalleAnulacion_Transacciones
            FOREIGN KEY (TransaccionId) REFERENCES dbo.PinPad_Transacciones(Id)
    );
END
GO

-- Migración idempotente: agrega las columnas nuevas si la tabla ya existía de una versión anterior
IF COL_LENGTH('dbo.PinPad_DetalleAnulacion', 'AutorizacionOriginal') IS NULL
    ALTER TABLE dbo.PinPad_DetalleAnulacion ADD AutorizacionOriginal VARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.PinPad_DetalleAnulacion', 'RedAdquirente') IS NULL
    ALTER TABLE dbo.PinPad_DetalleAnulacion ADD RedAdquirente VARCHAR(50) NULL;
GO

-- =====================================================================
-- Detalle específico de Configuración de Red del Pin Pad (ProcesoConfigPinPad)
-- =====================================================================
IF OBJECT_ID('dbo.PinPad_DetalleConfigRed', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PinPad_DetalleConfigRed
    (
        Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,
        TransaccionId       BIGINT            NOT NULL,

        DireccionIP         VARCHAR(20)       NULL,
        Mascara             VARCHAR(20)       NULL,
        Gateway             VARCHAR(20)       NULL,
        PrincipalHost       VARCHAR(20)       NULL,
        PrincipalPuerto     VARCHAR(10)       NULL,
        AlternoHost         VARCHAR(20)       NULL,
        AlternoPuerto       VARCHAR(10)       NULL,
        PuertoEscucha       VARCHAR(10)       NULL,

        CONSTRAINT FK_PinPad_DetalleConfigRed_Transacciones
            FOREIGN KEY (TransaccionId) REFERENCES dbo.PinPad_Transacciones(Id)
    );
END
GO

-- =====================================================================
-- Detalle específico de Proceso de Control ("PC" — LAN.ProcesoControl)
-- =====================================================================
IF OBJECT_ID('dbo.PinPad_DetalleProcesoControl', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PinPad_DetalleProcesoControl
    (
        Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
        TransaccionId   BIGINT            NOT NULL,

        Lote            VARCHAR(20)       NULL,
        Referencia      VARCHAR(30)       NULL,

        CONSTRAINT FK_PinPad_DetalleProcesoControl_Transacciones
            FOREIGN KEY (TransaccionId) REFERENCES dbo.PinPad_Transacciones(Id)
    );
END
GO

-- =====================================================================
-- Trama cruda enviada y recibida (aplica a CUALQUIER operación, útil para soporte)
-- =====================================================================
IF OBJECT_ID('dbo.PinPad_Tramas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PinPad_Tramas
    (
        Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,
        TransaccionId       BIGINT            NOT NULL,
        Direccion           VARCHAR(10)       NOT NULL,   -- 'ENVIADA' / 'RESPUESTA'
        TramaHex            NVARCHAR(MAX)     NULL,
        FechaHora           DATETIME2(3)      NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT FK_PinPad_Tramas_Transacciones
            FOREIGN KEY (TransaccionId) REFERENCES dbo.PinPad_Transacciones(Id)
    );
END
GO

-- =====================================================================
-- Eventos generales (info/advertencia/error), centralizados en SQL
-- =====================================================================
IF OBJECT_ID('dbo.PinPad_Eventos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PinPad_Eventos
    (
        Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,
        FechaHora           DATETIME2(3)      NOT NULL DEFAULT SYSDATETIME(),
        TipoEvento          VARCHAR(30)       NOT NULL,   -- Info, Advertencia, Error, Debug
        Origen              VARCHAR(100)      NULL,       -- clase/método origen
        Mensaje             NVARCHAR(MAX)     NOT NULL,
        TransaccionId       BIGINT            NULL,
        CONSTRAINT FK_PinPad_Eventos_Transacciones
            FOREIGN KEY (TransaccionId) REFERENCES dbo.PinPad_Transacciones(Id)
    );
END
GO

-- =====================================================================
-- Vista de auditoría: historial unificado, útil para la pantalla "Historial" de la app
-- =====================================================================
IF OBJECT_ID('dbo.vw_PinPad_Auditoria', 'V') IS NOT NULL
    DROP VIEW dbo.vw_PinPad_Auditoria;
GO

CREATE VIEW dbo.vw_PinPad_Auditoria AS
SELECT
    t.Id,
    t.FechaHoraInicio,
    t.FechaHoraFin,
    t.TipoOperacion,
    t.CodigoRespuesta,
    t.CodigoRespuestaAut,
    t.MensajeRespuesta,
    t.Exitoso,
    t.CajaID,
    t.NumeroFactura,
    t.UsuarioSistema,
    t.MaquinaOrigen,
    p.Monto,
    p.Autorizacion,
    p.Lote,
    p.Referencia AS ReferenciaPago,
    a.AutorizacionOriginal,
    a.RedAdquirente AS RedAdquirenteAnulacion,
    a.ReferenciaOriginal
FROM dbo.PinPad_Transacciones t
LEFT JOIN dbo.PinPad_DetallePago p ON p.TransaccionId = t.Id
LEFT JOIN dbo.PinPad_DetalleAnulacion a ON a.TransaccionId = t.Id;
GO

-- =====================================================================
-- Catálogo OFICIAL de códigos de respuesta del autorizador (ISO8583),
-- tal como lo documenta Datafast en "Especificaciones Funcionales PINPAD
-- FAST TRACK" v1.6, sección "CÓDIGOS DE RESPUESTAS DEL PINPAD".
--
-- IMPORTANTE: el mismo código ISO8583 puede repetirse con distinto
-- significado según el TIPO de transacción (ej. "00" = Aprobada en un pago,
-- pero también aparece como "Reverso aprobado" en un reverso; "10" solo
-- aparece como "Reverso declinado"). Por eso esta tabla NO se usa como JOIN
-- automático 1 a 1 en la vista de auditoría (sería ambiguo) — es una tabla
-- de referencia para consulta manual del soporte/desarrollo:
--   SELECT * FROM dbo.PinPad_CatalogoCodigosRespuesta WHERE Codigo = '51';
-- =====================================================================
IF OBJECT_ID('dbo.PinPad_CatalogoCodigosRespuesta', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PinPad_CatalogoCodigosRespuesta
    (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        Codigo              VARCHAR(3)    NOT NULL,
        DescripcionISO8583  VARCHAR(60)   NULL,
        MensajeComercio     VARCHAR(30)   NOT NULL
    );

    -- Lista EXACTA y COMPLETA de la sección "CÓDIGOS DE RESPUESTAS DEL PINPAD" del
    -- documento oficial "Especificaciones Funcionales PINPAD FAST TRACK" v1.6
    -- (Datafast/HIPER S.A.). Se preservan las filas duplicadas tal como aparecen en
    -- el original -- el mismo código ISO8583 puede tener distinto mensaje según el
    -- contexto (ej. compra vs. reverso), así que no se deduplican.
    INSERT INTO dbo.PinPad_CatalogoCodigosRespuesta (Codigo, DescripcionISO8583, MensajeComercio) VALUES
    ('00', NULL, 'Aprobada'),
    ('02', 'Call issuer - referral', 'Llame C. Autoriz.'),
    ('03', 'Invalid Merchant', 'Establ invalido'),
    ('03', 'Invalid Merchant', 'Establ invalido'),
    ('04', 'Pick up', 'Retenga y llame'),
    ('05', 'Decline', 'Trans rechazada'),
    ('07', 'Pickup special', 'Retenga y llame'),
    ('12', 'Invalid Transaction', 'Transac invalida'),
    ('13', 'Invalid Amount', 'Monto invalido'),
    ('', 'Card status prohibit', 'Tarj. en Boletin'),
    ('14', 'Invalid Card Number', 'Error num tarj.'),
    ('15', 'No such issuer', 'Error num tarj.'),
    ('17', 'Customer cancellation', 'Socio cancelado'),
    ('88', 'Try Request Again', 'Favor reintente'),
    ('19', 'Try Request Again', 'Favor reintente'),
    ('30', 'Format Error', 'Error de formato'),
    ('39', 'No credit account', 'No tarj credito'),
    ('41', 'Lost card', 'Retenga y llame'),
    ('43', 'Stolen Card', 'Retenga y llame'),
    ('51', 'Insufficient Funds', 'Cupo no disponible'),
    ('52', 'No checking account', 'Error tipo cta'),
    ('53', 'No saving account', 'Error tipo cta'),
    ('54', 'Expired Card', 'Tarjeta expirada'),
    ('55', 'Incorrect PIN', 'Pin incorrecto'),
    ('57', 'Transaction no permitted', 'Transac invalida'),
    ('61', 'Exceeds wdrl amount limit', 'Exc monto limite'),
    ('62', 'Restricted Card', 'Tarj en boletin'),
    ('65', 'Exceeds wdrl frecuency limit', 'Exc num limite'),
    ('75', 'Allowable nbr of PIN exceed', 'Exced limite PIN'),
    ('76', 'Invalid Transaction', 'Cuenta invalida'),
    ('77', NULL, 'Modalidad inval.'),
    ('79', NULL, 'Vigencia errada'),
    ('80', NULL, 'Establ cancelado'),
    ('81', NULL, 'Verifique Plazo'),
    ('82', NULL, '# aut no existe'),
    ('87', NULL, 'KR Reintente'),
    ('00', NULL, 'Reverso aprobado'),
    ('00', NULL, 'Reverso aprobado'),
    ('10', NULL, 'Reverso declinado'),
    ('91', NULL, 'Entidad fuera de linea'),
    ('91', NULL, 'Entidad fuera de linea'),
    ('02', NULL, 'Llame C. Autoriz'),
    ('10', NULL, 'Reverso declinado'),
    ('96', 'Timeout', 'Entidad fuera linea'),
    ('91', 'Invalid Virtual Circuit', 'Entidad fuera linea'),
    ('91', 'Line Switch not Operational', 'Entidad fuera linea'),
    ('91', 'Issuer or switch inoperative', 'Entidad fuera linea'),
    ('96', 'System Malfunction', 'Entidad fuera linea'),
    ('96', 'System Malfunction', 'Entidad fuera linea'),
    ('91', 'Server off-line', 'Entidad fuera linea'),
    ('91', 'Server off-line', 'Entidad fuera linea'),
    ('91', 'Unsched Down', 'Entidad fuera linea'),
    ('91', 'Sched Down', 'Entidad fuera linea'),
    ('02', 'Exceeds wdrl amount limit', 'Llame C. Autoriz'),
    ('02', 'Exceeds wdrl frecuency limit', 'Llame C. Autoriz'),
    ('10', 'No match for reversal', 'Reverso declinado'),
    ('10', 'Transaction Previously Reversed', 'Reverso declinado'),
    ('10', 'Record no found', 'Reverso declinado'),
    ('02', 'Renewal date too old', 'Llame C. Autoriz'),
    ('02', 'Renewal date conflict', 'Llame C. Autoriz'),
    ('05', 'Mensaje Invalido', 'Mensaje invalido'),
    ('05', 'Invalid Track II', 'Reintente por favor'),
    ('89', 'Invalid Terminal', 'Terminal Invalido'),
    ('84', NULL, 'Llame C. Autoriz'),
    ('83', NULL, 'Difer NO permitido'),
    ('78', NULL, 'Verif. Interes'),
    ('74', NULL, 'Verif Cod Establ'),
    ('91', NULL, 'Entidad Fuera de Linea');
END
GO

-- =====================================================================
-- Vista "tabla completa" del Proceso de Pago: TODOS los campos de la trama
-- decodificados en una sola fila por transacción, tal como los nombra la
-- especificación oficial "Especificaciones Funcionales PINPAD FAST TRACK".
-- Pensada para soporte/auditoría — mucho más detallada que
-- vw_PinPad_Auditoria (que es el resumen para la pantalla de Historial).
-- =====================================================================
IF OBJECT_ID('dbo.vw_PinPad_TramaCompletaPago', 'V') IS NOT NULL
    DROP VIEW dbo.vw_PinPad_TramaCompletaPago;
GO

CREATE VIEW dbo.vw_PinPad_TramaCompletaPago AS
SELECT
    t.Id                                            AS TransaccionId,
    t.NumeroFactura,
    t.FechaHoraInicio,
    t.FechaHoraFin,
    t.CajaID,
    t.UsuarioSistema,
    t.MaquinaOrigen,
    t.Exitoso,

    -- "Código de respuesta de mensaje" (nivel LAN/pin pad)
    t.CodigoRespuesta                               AS CodigoRespuestaLAN,

    -- "Código de respuesta del autorizador" (nivel banco) + su significado
    t.CodigoRespuestaAut,
    t.MensajeRespuesta                              AS MensajeRespuestaAut,

    -- Solicitud
    p.Monto                                         AS MontoTotalTransaccion,
    p.TipoTransaccion,
    p.Red                                           AS CodigoIdentificacionRedAdquirente,

    -- Respuesta — identificación de la transacción
    p.RedAdquirente,
    p.Referencia                                    AS SecuencialTransaccion,   -- "Secuencial transacción" oficial
    p.Lote                                           AS NumeroLote,
    p.Autorizacion                                  AS NumeroAutorizacion,
    p.TID                                           AS TerminalID,
    p.MID                                           AS MerchantID,
    p.ValorInteres                                  AS ValorInteresOFinanciamiento,
    p.MensajePublicidad,

    -- Respuesta — banco/tarjeta
    p.CodigoAdquirente                              AS CodigoBancoAdquirente,
    p.NombreAdquirente                              AS NombreBancoAdquirente,
    p.NombreGrupoTarjeta,
    p.ModoLectura,          -- 01=Manual 02=Banda 03=Chip 04=Fallback manual 05=Fallback banda 06=Contactless
    p.TarjetaHabiente                               AS NombreTarjetahabiente,
    p.NumeroTarjeta                                 AS NumeroTarjetaTruncado,
    p.FechaVencimiento,
    p.NumeroTarjetaEncriptado,

    -- Respuesta — solo aplica si ModoLectura = Chip (03)
    p.AplicacionEMV,
    p.AID,
    p.Criptograma                                   AS TipoCriptogramaYValor,
    p.VerificacionPIN,
    p.ARQC,
    p.TVR,
    p.TSI

FROM dbo.PinPad_Transacciones t
LEFT JOIN dbo.PinPad_DetallePago p ON p.TransaccionId = t.Id
WHERE t.TipoOperacion = 'ProcesoPago';
GO
