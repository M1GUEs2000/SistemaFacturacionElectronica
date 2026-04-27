using AccesoDatos.Abstractions;
using System;
using System.Data;
using System.Globalization;

namespace LogicaNegocios
{
    public class ProductoManejador
    {
        private readonly IConexionBD _conexion;
        private readonly LogManejador _log;

        public ProductoManejador(
            IConexionBD conexion,
            LogManejador log
        )
        {
            _conexion = conexion;
            _log = log;
        }

        // ======================================================
        // PRODUCTOS ORDEN < 9001 (POS / dropdown)
        // ======================================================
        public DataSet MostrarProductos()
        {
            string sql = "SELECT NOMBRE, VALOR, CODIGO, IVA, APLICA_ICE, ICE_CODIGO, ICE_TIPO, ICE_PORCENTAJE, ICE_VALOR, ICE_UNIDAD, AZUCAR_GRAMOS, VOLUMEN FROM PRODUCTOS WHERE ORDEN < 9001 ORDER BY NOMBRE";
            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // PRODUCTOS ORDEN >= 9001 (despensa / especiales)
        // ======================================================
        public DataSet MostrarProductosOrden()
        {
            string sql = "SELECT NOMBRE, VALOR, CODIGO, IVA, APLICA_ICE, ICE_CODIGO, ICE_TIPO, ICE_PORCENTAJE, ICE_VALOR, ICE_UNIDAD, AZUCAR_GRAMOS, VOLUMEN FROM PRODUCTOS WHERE ORDEN >= 9001 ORDER BY NOMBRE";
            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // MOSTRAR TODOS + BÚSQUEDA (admin / gestión)
        // SELECT * para que funcione independientemente de qué
        // columnas ICE existan en la BD en cada momento.
        // ======================================================
        public DataSet MostrarTodoProductos(string Nombre)
        {
            string sql = "SELECT * FROM PRODUCTOS";

            if (!string.IsNullOrEmpty(Nombre))
                sql += " WHERE NOMBRE LIKE '%" + Nombre + "%'";

            sql += " ORDER BY NOMBRE";

            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // MOSTRAR ESTADOS
        // ======================================================
        public DataSet MostrarEstados()
        {
            string sql = "SELECT DISTINCT ESTATUS FROM PRODUCTOS";
            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // CONSULTA POR NOMBRE (trae todos los campos — usado por GenerarXml)
        // ======================================================
        public DataSet ConsultaNombre(string Nombre)
        {
            string sql = "SELECT * FROM PRODUCTOS WHERE NOMBRE='" + Nombre + "'";
            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // BÚSQUEDA POR NOMBRE (contiene)
        // ======================================================
        public DataSet BuscarPorNombre(string nombre)
        {
            string sql = "SELECT NOMBRE, VALOR, CODIGO, IVA, APLICA_ICE, ICE_CODIGO, ICE_TIPO, ICE_PORCENTAJE, ICE_VALOR, ICE_UNIDAD, AZUCAR_GRAMOS, VOLUMEN FROM PRODUCTOS WHERE NOMBRE LIKE '%" + nombre + "%' ORDER BY NOMBRE";
            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // INSERTAR
        // ======================================================
        public int Insertar(
            string Nombre,
            string Valor,
            string Estatus,
            string Orden,
            string Codigo,
            string IVA,
            string AplicaIce,
            string IceCodigo,
            string IceTipo,
            string IcePorcentaje,
            string IceValor,
            string IceUnidad,
            string AzucarGramos,
            string Volumen,
            string Pvp,
            string TipoVehiculo,
            string Usuario,
            string IP)
        {
            string sql = @"INSERT INTO PRODUCTOS
                           (NOMBRE, VALOR, ESTATUS, ORDEN, CODIGO, IVA,
                            APLICA_ICE, ICE_CODIGO, ICE_TIPO, ICE_PORCENTAJE,
                            ICE_VALOR, ICE_UNIDAD, AZUCAR_GRAMOS, VOLUMEN,
                            PVP, TIPO_VEHICULO)
                           VALUES
                           ('" + Nombre          + "', " +
                            Num(Valor)           + ", '" +
                            Estatus              + "', " +
                            Num(Orden)           + ", '" +
                            Codigo               + "', '" +
                            IVA                  + "', " +
                            Bool(AplicaIce)      + ", '" +
                            IceCodigo            + "', '" +
                            IceTipo              + "', " +
                            Num(IcePorcentaje)   + ", " +
                            Num(IceValor)        + ", '" +
                            IceUnidad            + "', " +
                            Num(AzucarGramos)    + ", " +
                            Num(Volumen)         + ", " +
                            Num(Pvp)             + ", '" +
                            TipoVehiculo         + "')";

            _log.CrearLog("Se insertó el producto: " + Nombre, Usuario, IP, sql);

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // ACTUALIZAR
        // ======================================================
        public int Actualizar(
            string Nombre,
            string Valor,
            string Estado,
            string Orden,
            string Codigo,
            string IVA,
            string AplicaIce,
            string IceCodigo,
            string IceTipo,
            string IcePorcentaje,
            string IceValor,
            string IceUnidad,
            string AzucarGramos,
            string Volumen,
            string Pvp,
            string TipoVehiculo,
            string Usuario,
            string IP)
        {
            string sql = @"UPDATE PRODUCTOS SET
                           VALOR           = "  + Num(Valor)         + @",
                           ESTATUS         = '" + Estado             + @"',
                           ORDEN           = "  + Num(Orden)         + @",
                           CODIGO          = '" + Codigo             + @"',
                           IVA             = '" + IVA                + @"',
                           APLICA_ICE      = "  + Bool(AplicaIce)    + @",
                           ICE_CODIGO      = '" + IceCodigo          + @"',
                           ICE_TIPO        = '" + IceTipo            + @"',
                           ICE_PORCENTAJE  = "  + Num(IcePorcentaje) + @",
                           ICE_VALOR       = "  + Num(IceValor)      + @",
                           ICE_UNIDAD      = '" + IceUnidad          + @"',
                           AZUCAR_GRAMOS   = "  + Num(AzucarGramos)  + @",
                           VOLUMEN         = "  + Num(Volumen)       + @",
                           PVP             = "  + Num(Pvp)           + @",
                           TIPO_VEHICULO   = '" + TipoVehiculo       + @"'
                           WHERE NOMBRE = '" + Nombre + "'";

            _log.CrearLog("Se actualizó el producto: " + Nombre, Usuario, IP, sql);

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // ELIMINAR
        // ======================================================
        public int Eliminar(string Nombre, string Usuario, string IP)
        {
            string sql = "DELETE FROM PRODUCTOS WHERE Nombre='" + Nombre + "'";

            _log.CrearLog("Se eliminó el producto: " + Nombre, Usuario, IP, sql);

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // HELPERS DE FORMATO PARA ACCESS SQL
        // ======================================================

        // Currency / Number / Double: convierte a número invariante o NULL si vacío.
        private static string Num(string val)
        {
            if (string.IsNullOrWhiteSpace(val))
                return "NULL";

            // Acepta tanto "1,85" como "1.85"
            string clean = val.Trim().Replace(",", ".");

            if (double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
                return d.ToString("G", CultureInfo.InvariantCulture);

            return "NULL";
        }

        // Yes/No / bit: SI/1/TRUE → True (Access) o 1 (SQL Server)  |  resto → False / 0
        private string Bool(string val)
        {
            bool b = false;

            if (!string.IsNullOrWhiteSpace(val))
            {
                string v = val.Trim().ToUpperInvariant();
                b = v == "SI" || v == "1" || v == "TRUE" || v == "YES";
            }

            if (_conexion.EsSqlServer)
                return b ? "1" : "0";

            return b ? "True" : "False";
        }
    }
}
