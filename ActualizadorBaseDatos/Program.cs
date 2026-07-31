using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;

namespace ActualizadorBaseDatos
{
    internal static class Program
    {
        private const string VersionDestino = "1.1.1";
        private static readonly string[] TablasNuevas =
        {
            "PARAMETROS_TRANSACCIONES", "PINPAD_ANULACIONES", "PINPAD_AUTORIZADAS",
            "PINPAD_CATALOGO_CODIGOS_RESPUESTA", "PINPAD_DETALLE_CONFIG_RED",
            "PINPAD_DETALLE_PROCESO_CONTROL", "PINPAD_DETALLE_TARJETA", "PINPAD_EVENTOS",
            "PINPAD_LOG", "PINPAD_PAGO_EXTENDIDO", "PINPAD_TRAMAS"
        };

        private static int Main(string[] args)
        {
            try
            {
                var database = GetArgument(args, "--database");
                var template = GetArgument(args, "--template");
                if (String.IsNullOrWhiteSpace(database) || String.IsNullOrWhiteSpace(template))
                    throw new ArgumentException("Uso: ActualizadorBaseDatos.exe --database <base-cliente.accdb> --template <plantilla.accdb>");
                if (!File.Exists(database)) throw new FileNotFoundException("No se encontró la base de datos del cliente.", database);
                if (!File.Exists(template)) throw new FileNotFoundException("No se encontró la base plantilla.", template);

                using (var target = Open(database))
                using (var source = Open(template))
                {
                    if (!NeedsMigration(target)) return 0;

                    var backup = database + ".backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".accdb";
                    File.Copy(database, backup, false);

                    EnsureVersionTable(target);
                    foreach (var table in TablasNuevas) EnsureTableFromTemplate(source, target, database, table);
                    EnsureColumn(target, "EMPRESA", "RUCPROVEEDOR", "TEXT(255)");
                    EnsureNumeroFacturaShortText(target);
                    RegisterVersion(target);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("No se pudo actualizar la base de datos: " + ex.Message);
                return 1;
            }
        }

        private static string GetArgument(string[] args, string name)
        {
            for (var i = 0; i < args.Length - 1; i++) if (String.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            return null;
        }

        private static OleDbConnection Open(string path)
        {
            var connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.15.0;Data Source=" + path + ";Mode=Share Deny None;");
            connection.Open();
            return connection;
        }

        private static bool NeedsMigration(OleDbConnection target)
        {
            if (!TableExists(target, "VERSIONBASE") || !ColumnExists(target, "EMPRESA", "RUCPROVEEDOR") || IsNumeroFacturaLongText(target)) return true;
            foreach (var table in TablasNuevas) if (!TableExists(target, table)) return true;
            using (var command = new OleDbCommand("SELECT COUNT(*) FROM [VERSIONBASE] WHERE [VERSIONAPLICADA] = ?", target))
            {
                command.Parameters.AddWithValue("@p1", VersionDestino);
                return Convert.ToInt32(command.ExecuteScalar()) == 0;
            }
        }

        private static bool TableExists(OleDbConnection connection, string table)
        {
            var schema = connection.GetSchema("Tables");
            foreach (DataRow row in schema.Rows)
                if (String.Equals(Convert.ToString(row["TABLE_TYPE"]), "TABLE", StringComparison.OrdinalIgnoreCase) && String.Equals(Convert.ToString(row["TABLE_NAME"]), table, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static bool ColumnExists(OleDbConnection connection, string table, string column)
        {
            using (var command = new OleDbCommand("SELECT * FROM " + Q(table) + " WHERE 1=0", connection))
            using (var reader = command.ExecuteReader())
                for (var i = 0; i < reader.FieldCount; i++) if (String.Equals(reader.GetName(i), column, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static void EnsureVersionTable(OleDbConnection target)
        {
            if (!TableExists(target, "VERSIONBASE")) Execute(target, "CREATE TABLE [VERSIONBASE] ([VERSIONAPLICADA] TEXT(50), [FECHAAPLICACION] TEXT(30))");
        }

        private static void EnsureTableFromTemplate(OleDbConnection source, OleDbConnection target, string targetPath, string table)
        {
            if (TableExists(target, table)) return;
            // La consulta se ejecuta en la plantilla y crea en la base del cliente la misma tabla, con sus registros iniciales.
            var externalPath = targetPath.Replace("'", "''");
            Execute(source, "SELECT * INTO " + Q(table) + " IN '" + externalPath + "' FROM " + Q(table));
        }

        private static void EnsureColumn(OleDbConnection connection, string table, string column, string definition)
        {
            if (!ColumnExists(connection, table, column)) Execute(connection, "ALTER TABLE " + Q(table) + " ADD COLUMN " + Q(column) + " " + definition);
        }

        private static bool IsNumeroFacturaLongText(OleDbConnection connection)
        {
            using (var command = new OleDbCommand("SELECT * FROM [FACTURACION] WHERE 1=0", connection))
            using (var reader = command.ExecuteReader())
            {
                var schema = reader.GetSchemaTable();
                foreach (DataRow row in schema.Rows)
                    if (String.Equals(Convert.ToString(row["ColumnName"]), "NUMEROFACTURA", StringComparison.OrdinalIgnoreCase))
                        return Convert.ToInt64(row["ColumnSize"]) > 255;
            }
            return false;
        }

        private static void EnsureNumeroFacturaShortText(OleDbConnection connection)
        {
            if (!IsNumeroFacturaLongText(connection)) return;
            using (var command = new OleDbCommand("SELECT MAX(LEN([NUMEROFACTURA])) FROM [FACTURACION]", connection))
            {
                var value = command.ExecuteScalar();
                var maximumLength = value == DBNull.Value || value == null ? 0 : Convert.ToInt32(value);
                if (maximumLength > 255)
                    throw new InvalidOperationException("NUMEROFACTURA contiene valores mayores de 255 caracteres y no puede convertirse a texto corto.");
            }
            Execute(connection, "ALTER TABLE [FACTURACION] ALTER COLUMN [NUMEROFACTURA] TEXT(255)");
        }

        private static void RegisterVersion(OleDbConnection connection)
        {
            using (var command = new OleDbCommand("SELECT COUNT(*) FROM [VERSIONBASE] WHERE [VERSIONAPLICADA] = ?", connection))
            {
                command.Parameters.AddWithValue("@p1", VersionDestino);
                if (Convert.ToInt32(command.ExecuteScalar()) > 0) return;
            }
            using (var command = new OleDbCommand("INSERT INTO [VERSIONBASE] ([VERSIONAPLICADA], [FECHAAPLICACION]) VALUES (?, ?)", connection))
            {
                command.Parameters.AddWithValue("@p1", VersionDestino);
                command.Parameters.AddWithValue("@p2", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                command.ExecuteNonQuery();
            }
        }

        private static void Execute(OleDbConnection connection, string sql) { using (var command = new OleDbCommand(sql, connection)) command.ExecuteNonQuery(); }
        private static string Q(string name) { return "[" + name.Replace("]", "]]" ) + "]"; }
    }
}
