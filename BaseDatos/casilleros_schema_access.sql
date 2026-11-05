-- =====================================================================
-- Esquema del módulo de Casilleros para Access (Jet/ACE).
-- Mismo DDL que usa LogicaNegocios\Casilleros\CasilleroSchemaInstalador.cs
-- (rama Access) — si la app ya arrancó una vez, estas tablas ya existen y
-- este script no hace falta; se deja como referencia / para crearlas a mano
-- sin depender de la app.
--
-- IMPORTANTE: Access NO ejecuta varias sentencias en una sola consulta.
-- Cada bloque de abajo va en su propia consulta SQL (Crear → Diseño de
-- consulta → Vista SQL → pegar → Ejecutar), una por una, en este orden.
-- =====================================================================

-- 1) CASILLEROS -------------------------------------------------------
CREATE TABLE CASILLEROS (
    ID AUTOINCREMENT PRIMARY KEY,
    CODIGO TEXT(20) NOT NULL,
    UBICACION TEXT(100),
    ESTADO TEXT(20) NOT NULL,
    ACTIVO YESNO NOT NULL
);

-- 2) CASILLERO_ASIGNACIONES --------------------------------------------
CREATE TABLE CASILLERO_ASIGNACIONES (
    ID AUTOINCREMENT PRIMARY KEY,
    IDCASILLERO LONG NOT NULL,
    NOMBRECLIENTE TEXT(150) NOT NULL,
    MINUTOSSOLICITADOS LONG NOT NULL,
    FECHAINICIO DATETIME NOT NULL,
    FECHAFIN DATETIME NOT NULL,
    ESTADO TEXT(20) NOT NULL,
    NOTIFICADO YESNO NOT NULL,
    USUARIO TEXT(100),
    FECHACREACION DATETIME NOT NULL
);

-- 3) CASILLERO_PARAMETROS (una sola fila) ------------------------------
CREATE TABLE CASILLERO_PARAMETROS (
    ID AUTOINCREMENT PRIMARY KEY,
    MINUTOSNOTIFICACIONPREVIA LONG NOT NULL
);

-- Fila inicial (10 minutos de aviso por defecto). Ejecutar como consulta
-- de acción aparte, después de crear la tabla de arriba.
INSERT INTO CASILLERO_PARAMETROS (MINUTOSNOTIFICACIONPREVIA) VALUES (10);

-- 4) CASILLERO_NOTIFICACIONES -------------------------------------------
CREATE TABLE CASILLERO_NOTIFICACIONES (
    ID AUTOINCREMENT PRIMARY KEY,
    IDASIGNACION LONG NOT NULL,
    MENSAJE TEXT(255) NOT NULL,
    TIPO TEXT(20) NOT NULL,
    FECHACREACION DATETIME NOT NULL,
    MOSTRADA YESNO NOT NULL,
    LEIDA YESNO NOT NULL
);

-- =====================================================================
-- Con esto ya está el esquema completo. Si prefieres que la app lo cree
-- sola, no hace falta correr nada de esto: al abrir el módulo de
-- Casilleros (frmCasilleroPrincipal) llama a
-- CasilleroSchemaInstalador.AsegurarTablas, que crea lo que falte y hace
-- las migraciones de columnas (TIPO/MOSTRADA) si la tabla ya existía de
-- una versión anterior.
-- =====================================================================
