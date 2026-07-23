-- =====================================================================
-- Tablas de auditoría del PinPad para Access (C:\BDFacturacion.accdb)
-- Ejecutar en Access: Crear > Diseño de consulta > Vista SQL, pegar UNA
-- sentencia, Ejecutar (!), y repetir con la siguiente. Access NO ejecuta
-- varias sentencias juntas.
-- =====================================================================

-- 1) CABECERA -----------------------------------------------------------
CREATE TABLE PINPAD_LOG (
    ID                LONG        NOT NULL,
    TIPO_OPERACION    TEXT(50),
    USUARIO_SISTEMA   TEXT(100),
    CAJA_ID           TEXT(20),
    NUMERO_FACTURA    TEXT(50),
    FECHA_INICIO      DATETIME,
    CODIGO_RESPUESTA  TEXT(20),
    MENSAJE_RESPUESTA MEMO,
    EXITOSO           YESNO,
    EXCEPCION_MENSAJE MEMO,
    FECHA_FIN         DATETIME,
    CONSTRAINT PK_PINPAD_LOG PRIMARY KEY (ID)
);

-- 2) DETALLE DE COBRO ---------------------------------------------------
CREATE TABLE PINPAD_AUTORIZADAS (
    ID                    AUTOINCREMENT,
    TRANSACCION_ID        LONG,
    MONTO                 CURRENCY,
    BASE_IMPONIBLE        CURRENCY,
    BASE0                 CURRENCY,
    IVA                   CURRENCY,
    TIPO_TRANSACCION      TEXT(20),
    RED                   TEXT(20),
    TIPO_CREDITO          TEXT(50),
    EXITOSO               YESNO,
    CODIGO_RESPUESTA_AUT  TEXT(10),
    MENSAJE_RESPUESTA_AUT TEXT(255),
    AUTORIZACION          TEXT(20),
    REFERENCIA            TEXT(20),
    LOTE                  TEXT(20),
    RED_ADQUIRENTE        TEXT(30),
    NOMBRE_GRUPO_TARJETA  TEXT(30),
    TARJETA_HABIENTE      TEXT(100),
    NUMERO_TARJETA        TEXT(30),
    MODO_LECTURA          TEXT(20),
    FECHA                 TEXT(20),
    HORA                  TEXT(20),
    TID                   TEXT(20),
    MID                   TEXT(20),
    CONSTRAINT PK_PINPAD_AUTORIZADAS PRIMARY KEY (ID)
);

-- 3) DETALLE DE ANULACIÓN ----------------------------------------------
CREATE TABLE PINPAD_ANULACIONES (
    ID                   AUTOINCREMENT,
    TRANSACCION_ID       LONG,
    REFERENCIA_ORIGINAL  TEXT(20),
    AUTORIZACION_ORIGINAL TEXT(20),
    RED_ADQUIRENTE       TEXT(30),
    CONSTRAINT PK_PINPAD_ANULACIONES PRIMARY KEY (ID)
);

-- 4) ÍNDICE de correlación cabecera <- detalle (opcional pero recomendado)
CREATE INDEX IX_AUTORIZADAS_TRANS ON PINPAD_AUTORIZADAS (TRANSACCION_ID);
