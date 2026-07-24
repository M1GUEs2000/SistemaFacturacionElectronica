-- =====================================================================
-- Tablas de auditoría del PinPad para Access (C:\BDFacturacion.accdb)
--
-- MODELO SIN ID NUMÉRICO: la llave de correlación entre las 3 tablas es la
-- NUMEROFACTURA. En las tablas de detalle, TRANSACCIONID guarda esa MISMA
-- factura (por eso es TEXTO, no Número). Para operaciones sin factura (leer
-- tarjeta, consultar config, anular) el manejador pone un marcador "OP-...".
--
-- Ejecutar en Access: Crear > Diseño de consulta > Vista SQL, pegar UNA
-- sentencia, Ejecutar (!), y repetir con la siguiente. Access NO ejecuta
-- varias sentencias juntas. (Si las tablas ya existen, no re-ejecutar.)
-- =====================================================================

-- 1) CABECERA -----------------------------------------------------------
CREATE TABLE PINPAD_LOG (
    FECHAINICIO       DATETIME,
    FECHAFIN          DATETIME,
    TIPOOPERACION     TEXT(50),
    CODIGORESPUESTA   TEXT(20),
    MENSAJERESPUESTA  MEMO,
    EXITOSO           YESNO,
    EXCEPCIONMENSAJE  MEMO,
    NUMEROFACTURA     TEXT(50),
    CAJAID            TEXT(20),
    USUARIOSISTEMA    TEXT(100),
    MAQUINAORIGEN     TEXT(100)
);

-- 2) DETALLE DE COBRO ---------------------------------------------------
CREATE TABLE PINPAD_AUTORIZADAS (
    TRANSACCIONID        TEXT(50),
    MONTO                CURRENCY,
    TIPOTRANSACCION      TEXT(20),
    RED                  TEXT(20),
    CODIGORESPUESTAAUT   TEXT(10),
    MENSAJERESPUESTAAUT  TEXT(255),
    REDADQUIRENTE        TEXT(30),
    REFERENCIA           TEXT(20),
    LOTE                 TEXT(20),
    AUTORIZACION         TEXT(20),
    TID                  TEXT(20),
    MID                  TEXT(20),
    CODIGOADQUIRENTE     TEXT(20),
    NOMBREADQUIRENTE     TEXT(60),
    NOMBREGRUPOTARJETA   TEXT(30),
    MODOLECTURA          TEXT(20),
    TARJETAHABIENTE      TEXT(100),
    NUMEROTARJETA        TEXT(30),
    NUMEROFACTURA        TEXT(50)
);

-- 3) DETALLE DE ANULACIÓN ----------------------------------------------
CREATE TABLE PINPAD_ANULACIONES (
    TRANSACCIONID         TEXT(50),
    REFERENCIAORIGINAL    TEXT(20),
    AUTORIZACIONORIGINAL  TEXT(20),
    REDADQUIRENTE         TEXT(30),
    NUMEROFACTURA         TEXT(50)
);

-- 4) ÍNDICES de correlación por factura (opcional pero recomendado)
CREATE INDEX IX_LOG_FACTURA ON PINPAD_LOG (NUMEROFACTURA);
CREATE INDEX IX_AUTORIZADAS_FACTURA ON PINPAD_AUTORIZADAS (NUMEROFACTURA);
CREATE INDEX IX_ANULACIONES_FACTURA ON PINPAD_ANULACIONES (NUMEROFACTURA);
