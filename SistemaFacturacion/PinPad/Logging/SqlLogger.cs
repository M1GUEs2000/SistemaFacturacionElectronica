using System;
using System.Data.SqlClient;
using DF_PinPad.Wrapper.Models;

namespace DF_PinPad.Wrapper.Logging
{
    /// <summary>
    /// Implementación con ADO.NET puro (System.Data.SqlClient), sin dependencias externas.
    /// </summary>
    public class SqlLogger : ISqlLogger
    {
        private readonly string _connectionString;

        public SqlLogger(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public long IniciarTransaccion(string tipoOperacion, string usuarioSistema, string cajaId = null, string numeroFactura = null)
        {
            const string sql = @"
                INSERT INTO dbo.PinPad_Transacciones (TipoOperacion, UsuarioSistema, MaquinaOrigen, CajaID, NumeroFactura)
                OUTPUT INSERTED.Id
                VALUES (@TipoOperacion, @UsuarioSistema, @MaquinaOrigen, @CajaID, @NumeroFactura);";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@TipoOperacion", tipoOperacion);
                cmd.Parameters.AddWithValue("@UsuarioSistema", (object)usuarioSistema ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MaquinaOrigen", Environment.MachineName);
                cmd.Parameters.AddWithValue("@CajaID", (object)cajaId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NumeroFactura", (object)numeroFactura ?? DBNull.Value);

                conn.Open();
                // OUTPUT INSERTED.Id (columna BIGINT) llega boxeado como Int64, no como decimal.
                // Convert.ToInt64 es tolerante al tipo real devuelto por el driver;
                // un cast directo como (long)(decimal)... revienta con InvalidCastException.
                return Convert.ToInt64(cmd.ExecuteScalar());
            }
        }

        public void FinalizarTransaccion(long transaccionId, string codigoRespuesta, string mensajeRespuesta,
            bool exitoso, string excepcionMensaje, string codigoRespuestaAut = null)
        {
            const string sql = @"
                UPDATE dbo.PinPad_Transacciones
                SET FechaHoraFin       = SYSDATETIME(),
                    CodigoRespuesta    = @CodigoRespuesta,
                    CodigoRespuestaAut = @CodigoRespuestaAut,
                    MensajeRespuesta   = @MensajeRespuesta,
                    Exitoso            = @Exitoso,
                    ExcepcionMensaje   = @ExcepcionMensaje
                WHERE Id = @Id;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", transaccionId);
                cmd.Parameters.AddWithValue("@CodigoRespuesta", (object)codigoRespuesta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CodigoRespuestaAut", (object)codigoRespuestaAut ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MensajeRespuesta", (object)mensajeRespuesta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Exitoso", exitoso);
                cmd.Parameters.AddWithValue("@ExcepcionMensaje", (object)excepcionMensaje ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void VincularNumeroFactura(long transaccionId, string numeroFactura)
        {
            const string sql = @"
                UPDATE dbo.PinPad_Transacciones
                SET NumeroFactura = @NumeroFactura
                WHERE Id = @Id;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", transaccionId);
                cmd.Parameters.AddWithValue("@NumeroFactura", (object)numeroFactura ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void GuardarDetallePago(long transaccionId, ProcesoPagoRequest request, ProcesoPagoResult result)
        {
            const string sql = @"
                INSERT INTO dbo.PinPad_DetallePago
                    (TransaccionId, Monto, TipoTransaccion, Red,
                     CodigoRespuestaAut, MensajeRespuestaAut, RedAdquirente, Referencia, Lote,
                     Autorizacion, TID, MID, CodigoAdquirente, NombreAdquirente,
                     NombreGrupoTarjeta, ModoLectura, TarjetaHabiente, NumeroTarjeta, FechaVencimiento,
                     NumeroTarjetaEncriptado, ValorInteres, MensajePublicidad, AplicacionEMV, AID,
                     Criptograma, VerificacionPIN, ARQC, TVR, TSI)
                VALUES
                    (@TransaccionId, @Monto, @TipoTransaccion, @Red,
                     @CodigoRespuestaAut, @MensajeRespuestaAut, @RedAdquirente, @Referencia, @Lote,
                     @Autorizacion, @TID, @MID, @CodigoAdquirente, @NombreAdquirente,
                     @NombreGrupoTarjeta, @ModoLectura, @TarjetaHabiente, @NumeroTarjeta, @FechaVencimiento,
                     @NumeroTarjetaEncriptado, @ValorInteres, @MensajePublicidad, @AplicacionEMV, @AID,
                     @Criptograma, @VerificacionPIN, @ARQC, @TVR, @TSI);";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@TransaccionId", transaccionId);
                cmd.Parameters.AddWithValue("@Monto", (object)request?.Monto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TipoTransaccion", (object)request?.TipoTransaccion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Red", (object)request?.Red ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CodigoRespuestaAut", (object)result?.CodigoRespuestaAut ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MensajeRespuestaAut", (object)result?.MensajeRespuestaAut ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RedAdquirente", (object)result?.RedAdquirente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Referencia", (object)result?.Referencia ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Lote", (object)result?.Lote ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Autorizacion", (object)result?.Autorizacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TID", (object)result?.TID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MID", (object)result?.MID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CodigoAdquirente", (object)result?.CodigoAdquirente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NombreAdquirente", (object)result?.NombreAdquirente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NombreGrupoTarjeta", (object)result?.NombreGrupoTarjeta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ModoLectura", (object)result?.ModoLectura ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TarjetaHabiente", (object)result?.TarjetaHabiente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NumeroTarjeta", (object)result?.NumeroTarjeta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaVencimiento", (object)result?.FechaVencimiento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NumeroTarjetaEncriptado", (object)result?.NumeroTarjetaEncriptado ?? DBNull.Value);

                decimal valorInteres;
                object valorInteresParam = (result?.ValorInteres != null && decimal.TryParse(result.ValorInteres,
                    System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out valorInteres))
                    ? (object)valorInteres : DBNull.Value;
                cmd.Parameters.AddWithValue("@ValorInteres", valorInteresParam);

                cmd.Parameters.AddWithValue("@MensajePublicidad", (object)result?.MensajePublicidad ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AplicacionEMV", (object)result?.AplicacionEMV ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AID", (object)result?.AID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Criptograma", (object)result?.Criptograma ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@VerificacionPIN", (object)result?.VerificacionPIN ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ARQC", (object)result?.ARQC ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TVR", (object)result?.TVR ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TSI", (object)result?.TSI ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void GuardarDetalleTarjeta(long transaccionId,
            string numeroTarjeta, string numeroTarjetaEncriptado, string binTarjeta,
            string fechaVencimiento, string redAdquirienteCorriente, string redAdquirienteDiferido)
        {
            const string sql = @"
                INSERT INTO dbo.PinPad_DetalleTarjeta
                    (TransaccionId, NumeroTarjeta, NumeroTarjetaEncriptado, BinTarjeta,
                     FechaVencimiento, RedAdquirienteCorriente, RedAdquirienteDiferido)
                VALUES
                    (@TransaccionId, @NumeroTarjeta, @NumeroTarjetaEncriptado, @BinTarjeta,
                     @FechaVencimiento, @RedAdquirienteCorriente, @RedAdquirienteDiferido);";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@TransaccionId", transaccionId);
                cmd.Parameters.AddWithValue("@NumeroTarjeta", (object)numeroTarjeta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NumeroTarjetaEncriptado", (object)numeroTarjetaEncriptado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BinTarjeta", (object)binTarjeta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaVencimiento", (object)fechaVencimiento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RedAdquirienteCorriente", (object)redAdquirienteCorriente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RedAdquirienteDiferido", (object)redAdquirienteDiferido ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void GuardarDetalleAnulacion(long transaccionId, string referenciaOriginal, string autorizacionOriginal, string redAdquirente)
        {
            const string sql = @"
                INSERT INTO dbo.PinPad_DetalleAnulacion (TransaccionId, ReferenciaOriginal, AutorizacionOriginal, RedAdquirente)
                VALUES (@TransaccionId, @ReferenciaOriginal, @AutorizacionOriginal, @RedAdquirente);";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@TransaccionId", transaccionId);
                cmd.Parameters.AddWithValue("@ReferenciaOriginal", (object)referenciaOriginal ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AutorizacionOriginal", (object)autorizacionOriginal ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RedAdquirente", (object)redAdquirente ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void GuardarDetalleConfigRed(long transaccionId, ConfiguracionRedRequest request)
        {
            const string sql = @"
                INSERT INTO dbo.PinPad_DetalleConfigRed
                    (TransaccionId, DireccionIP, Mascara, Gateway, PrincipalHost, PrincipalPuerto,
                     AlternoHost, AlternoPuerto, PuertoEscucha)
                VALUES
                    (@TransaccionId, @DireccionIP, @Mascara, @Gateway, @PrincipalHost, @PrincipalPuerto,
                     @AlternoHost, @AlternoPuerto, @PuertoEscucha);";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@TransaccionId", transaccionId);
                cmd.Parameters.AddWithValue("@DireccionIP", (object)request?.DireccionIP ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Mascara", (object)request?.Mascara ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gateway", (object)request?.Gateway ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PrincipalHost", (object)request?.PrincipalHost ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PrincipalPuerto", (object)request?.PrincipalPuerto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AlternoHost", (object)request?.AlternoHost ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AlternoPuerto", (object)request?.AlternoPuerto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PuertoEscucha", (object)request?.PuertoEscucha ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void GuardarDetalleProcesoControl(long transaccionId, string lote, string referencia)
        {
            const string sql = @"
                INSERT INTO dbo.PinPad_DetalleProcesoControl (TransaccionId, Lote, Referencia)
                VALUES (@TransaccionId, @Lote, @Referencia);";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@TransaccionId", transaccionId);
                cmd.Parameters.AddWithValue("@Lote", (object)lote ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Referencia", (object)referencia ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void GuardarTrama(long transaccionId, string direccion, string tramaHex)
        {
            const string sql = @"
                INSERT INTO dbo.PinPad_Tramas (TransaccionId, Direccion, TramaHex)
                VALUES (@TransaccionId, @Direccion, @TramaHex);";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@TransaccionId", transaccionId);
                cmd.Parameters.AddWithValue("@Direccion", direccion);
                cmd.Parameters.AddWithValue("@TramaHex", (object)tramaHex ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void RegistrarEvento(string tipoEvento, string origen, string mensaje, long? transaccionId = null)
        {
            const string sql = @"
                INSERT INTO dbo.PinPad_Eventos (TipoEvento, Origen, Mensaje, TransaccionId)
                VALUES (@TipoEvento, @Origen, @Mensaje, @TransaccionId);";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@TipoEvento", tipoEvento);
                cmd.Parameters.AddWithValue("@Origen", (object)origen ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Mensaje", mensaje);
                cmd.Parameters.AddWithValue("@TransaccionId", (object)transaccionId ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
