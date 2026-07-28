using System;
using System.Data;
using System.Data.SqlClient;

namespace DF_PinPad.Wrapper.Logging
{
    public class AuditQueryService : IAuditQueryService
    {
        private readonly string _connectionString;

        public AuditQueryService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public DataTable ObtenerHistorial(int maxFilas = 200)
        {
            const string sql = @"
                SELECT TOP (@MaxFilas) *
                FROM dbo.vw_PinPad_Auditoria
                ORDER BY FechaHoraInicio DESC;";

            var table = new DataTable();
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@MaxFilas", maxFilas);
                adapter.Fill(table);
            }
            return table;
        }
    }
}
