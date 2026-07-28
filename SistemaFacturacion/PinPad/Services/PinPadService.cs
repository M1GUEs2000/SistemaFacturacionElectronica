using System;
using System.Globalization;
using System.Linq;
using DF_PinPad; // LAN, LANConfig, EnvioProcesoPago, RespuestaProcesoPago, EnvioProcesoControl, etc.
using DF_PinPad.Wrapper.Config;
using DF_PinPad.Wrapper.Logging;
using DF_PinPad.Wrapper.Models;

namespace DF_PinPad.Wrapper.Services
{
    /// <summary>
    /// Envuelve DF_PinPad.LAN (clase orquestadora real de la DLL) e implementa las 5
    /// operaciones soportadas, registrando auditoría completa de cada una en SQL Server.
    ///
    /// Todas las firmas usadas fueron confirmadas leyendo la metadata IL de DF_PinPad.dll
    /// por reflection (no son una suposición):
    ///
    ///   LANConfig(string IP, int Puerto, int TimeOut, string MID, string TID,
    ///             string CajaID, int Version, int SHA)
    ///   LAN(LANConfig cfg)
    ///
    ///   RespuestaConfiguracionBasica LAN.ConsultaConfiguracionBasica()      // sin parámetros
    ///   RespuestaConsultaTarjeta     LAN.ConsultaTarjeta()                  // sin parámetros
    ///   RespuestaLecturaTarjeta      LAN.LecturaTarjeta()                  // sin parámetros
    ///   RespuestaProcesoPago         LAN.ProcesoPago(EnvioProcesoPago envio)  // usado para Pago Y Anulación (TipoTransaccion="3")
    ///
    /// ⚠️ Único punto sin confirmar por reflection (requiere prueba real contra el pin pad):
    ///    el formato exacto de los campos numéricos de EnvioProcesoPago (ver FormatMonto()).
    /// </summary>
    public class PinPadService : IPinPadService
    {
        private readonly PinPadSettings _settings;
        private readonly ISqlLogger _logger;

        public PinPadService(PinPadSettings settings, ISqlLogger logger)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private LANConfig BuildConfig()
        {
            // NOTA: DF_PinPad.dll requiere un archivo "DF_PinPad.dll.config" junto a sí misma
            // en la carpeta de salida (bin\Debug), con la clave de appSettings "GrabaLog".
            // Sin ese archivo, el propio constructor de LAN lanza NullReferenceException al
            // intentar leer esa configuración internamente (ver README, sección "Requisito
            // de despliegue crítico"). Ya está incluido y configurado en los .csproj de los
            // proyectos ejecutables (DF_PinPad.App y DF_PinPad.Wrapper.ConsoleTest).
            return new LANConfig(
                _settings.IP,
                _settings.Puerto,
                _settings.TimeOutMs,
                _settings.MID,
                _settings.TID,
                _settings.CajaID,
                _settings.Version,
                _settings.SHA);
        }

        // =================================================================
        // 1) CONSULTA DE CONFIGURACIÓN BÁSICA
        // =================================================================
        public ConfiguracionBasicaResult ConsultarConfiguracionBasica()
        {
            long transaccionId = _logger.IniciarTransaccion("ConsultaConfiguracionBasica", Environment.UserName, _settings.CajaID);
            var result = new ConfiguracionBasicaResult { TransaccionLogId = transaccionId };

            try
            {
                var cfg = BuildConfig();
                var lan = new LAN(cfg);

                _logger.RegistrarEvento("Info", nameof(PinPadService),
                    "Iniciando ConsultaConfiguracionBasica", transaccionId);

                RespuestaConfiguracionBasica respuesta = lan.ConsultaConfiguracionBasica();

                _logger.GuardarTrama(transaccionId, "ENVIADA", lan.TramaEnviada);
                _logger.GuardarTrama(transaccionId, "RESPUESTA", lan.TramaRespuesta);

                result.CodigoRespuesta = respuesta?.CodigoRespuesta;
                result.MensajeRespuesta = respuesta?.MensajeRespuesta;
                result.TramaPinPad = lan.TramaRespuesta; // TramaPinPad es privado en la DLL; usamos LAN.TramaRespuesta (público)
                result.Exitoso = respuesta != null && respuesta.CodigoRespuesta == "00";

                _logger.FinalizarTransaccion(transaccionId, result.CodigoRespuesta, result.MensajeRespuesta,
                    result.Exitoso, null);

                return result;
            }
            catch (Exception ex)
            {
                result.Exitoso = false;
                result.ExcepcionMensaje = ex.ToString();
                _logger.FinalizarTransaccion(transaccionId, null, null, false, ex.ToString());
                _logger.RegistrarEvento("Error", nameof(PinPadService), ex.ToString(), transaccionId);
                throw;
            }
        }

        // =================================================================
        // 2) CONSULTA DE TARJETA
        // =================================================================
        public ConsultaTarjetaResult ConsultarTarjeta()
        {
            long transaccionId = _logger.IniciarTransaccion("ConsultaTarjeta", Environment.UserName, _settings.CajaID);
            var result = new ConsultaTarjetaResult { TransaccionLogId = transaccionId };

            try
            {
                var cfg = BuildConfig();
                var lan = new LAN(cfg);

                _logger.RegistrarEvento("Info", nameof(PinPadService),
                    "Iniciando ConsultaTarjeta (esperando que el usuario deslice/inserte la tarjeta en el pin pad)",
                    transaccionId);

                RespuestaConsultaTarjeta respuesta = lan.ConsultaTarjeta();

                _logger.GuardarTrama(transaccionId, "ENVIADA", lan.TramaEnviada);
                _logger.GuardarTrama(transaccionId, "RESPUESTA", lan.TramaRespuesta);

                result.CodigoRespuesta = respuesta?.CodigoRespuesta;
                result.MensajeRespuesta = respuesta?.MensajeRespuesta;
                result.NumeroTarjeta = respuesta?.NumeroTarjeta;
                result.BinTarjeta = respuesta?.BinTarjeta;
                result.FechaVencimiento = respuesta?.FechaVencimiento;
                result.TramaPinPad = lan.TramaRespuesta; // TramaPinPad es privado en la DLL; usamos LAN.TramaRespuesta (público)
                result.Exitoso = respuesta != null && respuesta.CodigoRespuesta == "00";

                _logger.GuardarDetalleTarjeta(transaccionId,
                    result.NumeroTarjeta, null, result.BinTarjeta, result.FechaVencimiento, null, null);

                _logger.FinalizarTransaccion(transaccionId, result.CodigoRespuesta, result.MensajeRespuesta,
                    result.Exitoso, null);

                return result;
            }
            catch (Exception ex)
            {
                result.Exitoso = false;
                result.ExcepcionMensaje = ex.ToString();
                _logger.FinalizarTransaccion(transaccionId, null, null, false, ex.ToString());
                _logger.RegistrarEvento("Error", nameof(PinPadService), ex.ToString(), transaccionId);
                throw;
            }
        }

        // =================================================================
        // 3) LECTURA DE TARJETA
        // =================================================================
        public LecturaTarjetaResult LeerTarjeta()
        {
            long transaccionId = _logger.IniciarTransaccion("LecturaTarjeta", Environment.UserName, _settings.CajaID);
            var result = new LecturaTarjetaResult { TransaccionLogId = transaccionId };

            try
            {
                var cfg = BuildConfig();
                var lan = new LAN(cfg);

                _logger.RegistrarEvento("Info", nameof(PinPadService),
                    "Iniciando LecturaTarjeta (esperando que el usuario deslice/inserte la tarjeta en el pin pad)",
                    transaccionId);

                RespuestaLecturaTarjeta respuesta = lan.LecturaTarjeta();

                _logger.GuardarTrama(transaccionId, "ENVIADA", lan.TramaEnviada);
                _logger.GuardarTrama(transaccionId, "RESPUESTA", lan.TramaRespuesta);

                result.CodigoRespuesta = respuesta?.CodigoRespuesta;
                result.MensajeRespuesta = respuesta?.MensajeRespuesta;
                result.RedAdquirienteCorriente = respuesta?.RedAdquirienteCorriente;
                result.RedAdquirienteDiferido = respuesta?.RedAdquirienteDiferido;
                result.NumeroTarjeta = respuesta?.NumeroTarjeta;
                result.NumeroTarjetaEncriptado = respuesta?.NumeroTarjetaEncriptado;
                result.BinTarjeta = respuesta?.BinTarjeta;
                result.FechaVencimiento = respuesta?.FechaVencimiento;
                result.TramaPinPad = lan.TramaRespuesta; // TramaPinPad es privado en la DLL; usamos LAN.TramaRespuesta (público)
                result.Exitoso = respuesta != null && respuesta.CodigoRespuesta == "00";

                _logger.GuardarDetalleTarjeta(transaccionId,
                    result.NumeroTarjeta, result.NumeroTarjetaEncriptado, result.BinTarjeta,
                    result.FechaVencimiento, result.RedAdquirienteCorriente, result.RedAdquirienteDiferido);

                _logger.FinalizarTransaccion(transaccionId, result.CodigoRespuesta, result.MensajeRespuesta,
                    result.Exitoso, null);

                return result;
            }
            catch (Exception ex)
            {
                result.Exitoso = false;
                result.ExcepcionMensaje = ex.ToString();
                _logger.FinalizarTransaccion(transaccionId, null, null, false, ex.ToString());
                _logger.RegistrarEvento("Error", nameof(PinPadService), ex.ToString(), transaccionId);
                throw;
            }
        }

        // =================================================================
        // 4) PROCESO DE PAGO
        // =================================================================
        public ProcesoPagoResult ProcesarPago(ProcesoPagoRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            long transaccionId = _logger.IniciarTransaccion("ProcesoPago", request.UsuarioSistema, _settings.CajaID, request.NumeroFactura);
            var result = new ProcesoPagoResult { TransaccionLogId = transaccionId };

            try
            {
                var cfg = BuildConfig();
                var lan = new LAN(cfg);

                // La DLL NO valida que Total = Base0 + BaseImponible + IVA — pero Datafast sí,
                // y rechaza con "valores incorrectos" si no cuadran. Por eso el Total SIEMPRE
                // se recalcula aquí a partir de los otros tres, ignorando cualquier valor de
                // Monto que venga inconsistente (p. ej. desde la consola de pruebas).
                decimal baseImponible = request.BaseImponible ?? request.Monto;
                decimal montoCalculado = request.Base0 + baseImponible + request.IVA;
                request.Monto = montoCalculado; // mantiene consistente el log de auditoría también

                // Referencia: la DLL exige numérico de máximo 6 dígitos (PadLeft(6,'0') interno).
                // Si viene vacía, o no numérica, o de más de 6 dígitos, se genera una válida.
                string referencia = request.Referencia;
                if (string.IsNullOrWhiteSpace(referencia) || referencia.Length > 6 || !referencia.All(char.IsDigit))
                {
                    referencia = GenerarReferenciaNumerica();
                    request.Referencia = referencia;
                }

                var envio = new EnvioProcesoPago(cfg)
                {
                    TipoTransaccion = MapTipoTransaccion(request.TipoTransaccion),
                    RedAdquirente = MapRed(request.Red),
                    CodigoDiferido = MapTipoCredito(request.TipoTransaccion, request.TipoCredito),
                    PlazoDiferido = (request.PlazoDiferido ?? 0).ToString(CultureInfo.InvariantCulture),
                    MesesGracia = (request.MesesGracia ?? 0).ToString(CultureInfo.InvariantCulture),
                    MontoTotal = FormatMonto(montoCalculado),
                    BaseImponible = FormatMonto(baseImponible),
                    Base0 = FormatMonto(request.Base0),
                    IVA = FormatMonto(request.IVA),
                    Servicio = FormatMontoOpcional(request.Servicio),
                    Propina = FormatMontoOpcional(request.Propina),
                    MontoFijo = FormatMontoOpcional(request.MontoFijo),
                    Referencia = referencia,
                    Hora = DateTime.Now,
                    Fecha = DateTime.Now,
                    Autorizacion = string.Empty,
                    MID = _settings.MID,
                    TID = _settings.TID,
                    CID = string.Empty,
                    OTT = string.Empty,
                    OTTProveedor = MapBilletera(request.Billetera),
                    // Campo OFICIAL "Número de factura" de la especificación Datafast (15 AN,
                    // justificado con blancos a la derecha) — antes lo dejábamos vacío y solo
                    // correlacionábamos en NUESTRA propia base. Ahora viaja nativamente en la
                    // trama, visible también para Datafast/soporte.
                    Factura = (request.NumeroFactura ?? string.Empty).Trim(),
                    PushBanco = string.Empty,
                    PushCodigo = string.Empty
                };

                _logger.RegistrarEvento("Info", nameof(PinPadService),
                    $"Iniciando ProcesoPago. Monto={request.Monto}, Red={request.Red}, Tipo={request.TipoTransaccion}",
                    transaccionId);

                RespuestaProcesoPago respuesta = lan.ProcesoPago(envio);

                result.TramaEnviadaHex = lan.TramaEnviada;
                result.TramaRespuestaHex = lan.TramaRespuesta;
                _logger.GuardarTrama(transaccionId, "ENVIADA", result.TramaEnviadaHex);
                _logger.GuardarTrama(transaccionId, "RESPUESTA", result.TramaRespuestaHex);

                result.CodigoRespuesta = respuesta?.CodigoRespuesta;
                result.CodigoRespuestaAut = respuesta?.CodigoRespuestaAut;
                result.MensajeRespuestaAut = respuesta?.MensajeRespuestaAut;
                result.RedAdquirente = respuesta?.RedAdquirente;
                result.Referencia = respuesta?.Referencia;
                result.Lote = respuesta?.Lote;
                result.Hora = respuesta?.Hora;
                result.Fecha = respuesta?.Fecha;
                result.Autorizacion = respuesta?.Autorizacion;
                result.TID = respuesta?.TID;
                result.MID = respuesta?.MID;
                result.CodigoAdquirente = respuesta?.CodigoAdquirente;
                result.NombreAdquirente = respuesta?.NombreAdquirente;
                result.NombreGrupoTarjeta = respuesta?.NombreGrupoTarjeta;
                result.ModoLectura = respuesta?.ModoLectura;
                result.TarjetaHabiente = respuesta?.TarjetaHabiente;
                result.NumeroTarjeta = respuesta?.NumeroTajeta; // nombre real en la DLL (sin "r")
                result.FechaVencimiento = respuesta?.FechaVencimiento;
                result.NumeroTarjetaEncriptado = respuesta?.NumerTarjetaEncriptado; // nombre real en la DLL (sin "o")
                result.TarjetaTruncadaOTT = respuesta?.TarjetaTruncOTT;
                result.ValorInteres = respuesta?.Interes;
                result.MensajePublicidad = respuesta?.Publicidad;
                result.AplicacionEMV = respuesta?.AplicacionEMV;
                result.AID = respuesta?.AID;
                result.Criptograma = respuesta?.Criptograma;
                result.VerificacionPIN = respuesta?.PIN;
                result.ARQC = respuesta?.ARQC;
                result.TVR = respuesta?.TVR;
                result.TSI = respuesta?.TSI;
                result.TramaPinPad = lan.TramaRespuesta; // TramaPinPad es privado en la DLL; usamos LAN.TramaRespuesta (público)
                // CodigoRespuesta (nivel LAN/pin pad) y CodigoRespuestaAut (nivel banco) son
                // campos DISTINTOS según la especificación oficial Datafast. Si CodigoRespuesta
                // != "00" (ej. "20"=Error durante proceso), la operación nunca completó
                // correctamente a nivel LAN — CONFIRMADO CON UN CASO REAL: una anulación con
                // "TARJETA NO COINCIDE" trae CodigoRespuesta="20" pero CodigoRespuestaAut="00"
                // (valor residual, no una aprobación real). Por eso se exigen AMBOS en "00".
                result.Exitoso = respuesta != null
                    && respuesta.CodigoRespuesta == "00"
                    && respuesta.CodigoRespuestaAut == "00";

                _logger.GuardarDetallePago(transaccionId, request, result);
                _logger.FinalizarTransaccion(transaccionId, result.CodigoRespuesta, result.MensajeRespuestaAut,
                    result.Exitoso, null, result.CodigoRespuestaAut);

                _logger.RegistrarEvento("Info", nameof(PinPadService),
                    $"ProcesoPago finalizado. Exitoso={result.Exitoso}, Codigo={result.CodigoRespuesta}, Auth={result.Autorizacion}",
                    transaccionId);

                return result;
            }
            catch (Exception ex)
            {
                result.Exitoso = false;
                result.ExcepcionMensaje = ex.ToString();
                _logger.FinalizarTransaccion(transaccionId, null, null, false, ex.ToString());
                _logger.RegistrarEvento("Error", nameof(PinPadService), ex.ToString(), transaccionId);
                throw;
            }
        }

        // =================================================================
        // 5) ANULACIÓN
        //    Usa LAN.ProcesoPago(EnvioProcesoPago) con TipoTransaccion="3" (Anulación) —
        //    NO LAN.ProcesoControl. Confirmado comparando la pantalla de referencia
        //    ("Simulador .NET principal") con los campos reales de cada clase:
        //    EnvioProcesoControl NO tiene RedAdquirente ni Autorizacion, pero la pantalla
        //    de Anulación de referencia sí pide "Red Adq" y "Autorización" — eso solo
        //    existe en EnvioProcesoPago. Por eso la anulación real usa el mismo mecanismo
        //    que un pago, simplemente con TipoTransaccion=Anulación.
        //
        //    Nota sobre la trama (confirmado por IL de ObtenerTrama): cuando
        //    TipoTransaccion=="03" (Anulación), el campo Autorizacion se envía TAL CUAL
        //    (sin relleno) con el valor real que aquí se proporcione, y Referencia se
        //    fuerza siempre a espacios sin importar lo que se envíe (por eso Referencia
        //    es opcional/informativa en este flujo, no la usa la DLL para identificar
        //    la transacción original — la identificación real es por Autorización).
        // =================================================================
        public AnulacionResult Anular(AnulacionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            long transaccionId = _logger.IniciarTransaccion("Anulacion", request.UsuarioSistema, _settings.CajaID);
            var result = new AnulacionResult { TransaccionLogId = transaccionId };

            try
            {
                var cfg = BuildConfig();
                var lan = new LAN(cfg);

                if (string.IsNullOrWhiteSpace(request.Autorizacion))
                    throw new ArgumentException("Autorizacion es obligatoria para anular una transacción.");

                // La DLL rellena Referencia con PadLeft(6,'0') para Anulación (confirmado:
                // en Anulación SÍ se usa el valor real, a diferencia de Corriente/Diferido
                // donde se ignora). Si no es numérica de máximo 6 dígitos, desalinea la trama.
                string referenciaAnulacion = (request.Referencia ?? string.Empty).Trim();
                if (referenciaAnulacion.Length > 6 || (referenciaAnulacion.Length > 0 && !referenciaAnulacion.All(char.IsDigit)))
                    throw new ArgumentException($"Referencia debe ser numérica de máximo 6 dígitos. Valor recibido: '{referenciaAnulacion}'");

                var envio = new EnvioProcesoPago(cfg)
                {
                    TipoTransaccion = MapTipoTransaccion("Anulacion"), // "3"
                    RedAdquirente = MapRed(request.Red),
                    CodigoDiferido = ((int)TipoCredito.Corriente).ToString(CultureInfo.InvariantCulture),
                    PlazoDiferido = "0",
                    MesesGracia = "0",
                    MontoTotal = FormatMonto(0),
                    BaseImponible = FormatMonto(0),
                    Base0 = FormatMonto(0),
                    IVA = FormatMonto(0),
                    Servicio = FormatMontoOpcional(0),
                    Propina = FormatMontoOpcional(0),
                    MontoFijo = FormatMontoOpcional(0),
                    Referencia = referenciaAnulacion,
                    Hora = DateTime.Now,
                    Fecha = DateTime.Now,
                    Autorizacion = request.Autorizacion.Trim(),
                    MID = _settings.MID,
                    TID = _settings.TID,
                    CID = string.Empty,
                    OTT = string.Empty,
                    OTTProveedor = string.Empty,
                    Factura = string.Empty,
                    PushBanco = string.Empty,
                    PushCodigo = string.Empty
                };

                _logger.RegistrarEvento("Info", nameof(PinPadService),
                    $"Iniciando Anulacion. Red={request.Red}, Autorizacion={request.Autorizacion}", transaccionId);

                RespuestaProcesoPago respuesta = lan.ProcesoPago(envio);

                _logger.GuardarTrama(transaccionId, "ENVIADA", lan.TramaEnviada);
                _logger.GuardarTrama(transaccionId, "RESPUESTA", lan.TramaRespuesta);

                result.CodigoRespuesta = respuesta?.CodigoRespuesta;
                result.CodigoRespuestaAut = respuesta?.CodigoRespuestaAut;
                result.MensajeRespuesta = respuesta?.MensajeRespuestaAut;
                result.Lote = respuesta?.Lote;
                result.Referencia = respuesta?.Referencia;
                result.Autorizacion = respuesta?.Autorizacion;
                result.RedAdquirente = respuesta?.RedAdquirente;
                result.CodigoAdquirente = respuesta?.CodigoAdquirente;
                result.NombreAdquirente = respuesta?.NombreAdquirente;
                result.TarjetaHabiente = respuesta?.TarjetaHabiente;
                result.NumeroTarjeta = respuesta?.NumeroTajeta; // nombre real en la DLL (sin "r")
                result.FechaVencimiento = respuesta?.FechaVencimiento;
                result.NombreGrupoTarjeta = respuesta?.NombreGrupoTarjeta;
                result.TramaPinPad = lan.TramaRespuesta; // TramaPinPad es privado en la DLL; usamos LAN.TramaRespuesta (público)
                // CORREGIDO: igual que en ProcesarPago, el código real de aprobación/rechazo
                // del adquirente está en CodigoRespuestaAut, no en CodigoRespuesta (que es
                // solo el ACK de comunicación LAN).
                // Mismo criterio que en ProcesarPago: se exigen AMBOS códigos en "00" (ver
                // caso real documentado ahí — "TARJETA NO COINCIDE" trae CodigoRespuesta="20"
                // con CodigoRespuestaAut="00" residual, que sin este chequeo se marcaría
                // incorrectamente como exitosa).
                result.Exitoso = respuesta != null
                    && respuesta.CodigoRespuesta == "00"
                    && respuesta.CodigoRespuestaAut == "00";

                _logger.GuardarDetalleAnulacion(transaccionId, request.Referencia, request.Autorizacion, request.Red);
                _logger.FinalizarTransaccion(transaccionId, result.CodigoRespuesta, result.MensajeRespuesta,
                    result.Exitoso, null, result.CodigoRespuestaAut);

                return result;
            }
            catch (Exception ex)
            {
                result.Exitoso = false;
                result.ExcepcionMensaje = ex.ToString();
                _logger.FinalizarTransaccion(transaccionId, null, null, false, ex.ToString());
                _logger.RegistrarEvento("Error", nameof(PinPadService), ex.ToString(), transaccionId);
                throw;
            }
        }

        // =================================================================
        // 6) PROCESO DE CONFIGURACIÓN DE RED DEL PIN PAD
        //    LAN.ProcesoConfigPinPad(EnvioProcesoConfigPinPad) -> RespuestaProcesoConfigPinPad
        //    Campos reales confirmados por reflection: DireccionIP, Mascara, Gateway,
        //    PrincipalHost, PrincipalPuerto, AlternoHost, AlternoPuerto, PuertoEscucha (todos string).
        // =================================================================
        public ConfiguracionRedResult ConfigurarRedPinPad(ConfiguracionRedRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            long transaccionId = _logger.IniciarTransaccion("ConfiguracionRed", request.UsuarioSistema, _settings.CajaID);
            var result = new ConfiguracionRedResult { TransaccionLogId = transaccionId };

            try
            {
                var cfg = BuildConfig();
                var lan = new LAN(cfg);

                var envio = new EnvioProcesoConfigPinPad(cfg)
                {
                    DireccionIP = request.DireccionIP ?? string.Empty,
                    Mascara = request.Mascara ?? string.Empty,
                    Gateway = request.Gateway ?? string.Empty,
                    PrincipalHost = request.PrincipalHost ?? string.Empty,
                    PrincipalPuerto = request.PrincipalPuerto ?? string.Empty,
                    AlternoHost = request.AlternoHost ?? string.Empty,
                    AlternoPuerto = request.AlternoPuerto ?? string.Empty,
                    PuertoEscucha = request.PuertoEscucha ?? string.Empty
                };

                _logger.RegistrarEvento("Info", nameof(PinPadService),
                    $"Iniciando ConfiguracionRed. IP={request.DireccionIP}, PrincipalHost={request.PrincipalHost}:{request.PrincipalPuerto}",
                    transaccionId);

                RespuestaProcesoConfigPinPad respuesta = lan.ProcesoConfigPinPad(envio);

                _logger.GuardarTrama(transaccionId, "ENVIADA", lan.TramaEnviada);
                _logger.GuardarTrama(transaccionId, "RESPUESTA", lan.TramaRespuesta);

                result.CodigoRespuesta = respuesta?.CodigoRespuesta;
                result.MensajeRespuesta = respuesta?.MensajeRespuesta;
                result.Exitoso = respuesta != null && respuesta.CodigoRespuesta == "00";

                _logger.GuardarDetalleConfigRed(transaccionId, request);
                _logger.FinalizarTransaccion(transaccionId, result.CodigoRespuesta, result.MensajeRespuesta,
                    result.Exitoso, null);

                return result;
            }
            catch (Exception ex)
            {
                result.Exitoso = false;
                result.ExcepcionMensaje = ex.ToString();
                _logger.FinalizarTransaccion(transaccionId, null, null, false, ex.ToString());
                _logger.RegistrarEvento("Error", nameof(PinPadService), ex.ToString(), transaccionId);
                throw;
            }
        }

        // =================================================================
        // 7) PROCESO DE CONTROL ("PC")
        //    LAN.ProcesoControl(EnvioProcesoControl) -> RespuestaProcesoControl
        //    Identifica la transacción por Lote + Referencia (ambos numéricos, máx. 6
        //    dígitos, PadLeft(6,'0') interno — confirmado por reflection de ObtenerTrama()).
        // =================================================================
        public ProcesoControlResult ProcesarControl(ProcesoControlRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            long transaccionId = _logger.IniciarTransaccion("ProcesoControl", request.UsuarioSistema, _settings.CajaID);
            var result = new ProcesoControlResult { TransaccionLogId = transaccionId };

            try
            {
                var cfg = BuildConfig();
                var lan = new LAN(cfg);

                string lote = (request.Lote ?? string.Empty).Trim();
                string referencia = (request.Referencia ?? string.Empty).Trim();

                if (lote.Length > 6 || (lote.Length > 0 && !lote.All(char.IsDigit)))
                    throw new ArgumentException($"Lote debe ser numérico de máximo 6 dígitos. Valor recibido: '{lote}'");
                if (referencia.Length > 6 || (referencia.Length > 0 && !referencia.All(char.IsDigit)))
                    throw new ArgumentException($"Referencia debe ser numérica de máximo 6 dígitos. Valor recibido: '{referencia}'");

                var envio = new EnvioProcesoControl(cfg)
                {
                    Lote = lote,
                    Referencia = referencia,
                    MID = string.IsNullOrWhiteSpace(request.MID) ? _settings.MID : request.MID.Trim(),
                    TID = string.IsNullOrWhiteSpace(request.TID) ? _settings.TID : request.TID.Trim(),
                    CajaID = string.IsNullOrWhiteSpace(request.CajaID) ? _settings.CajaID : request.CajaID.Trim()
                };

                _logger.RegistrarEvento("Info", nameof(PinPadService),
                    $"Iniciando ProcesoControl. Lote={lote}, Referencia={referencia}", transaccionId);

                RespuestaProcesoControl respuesta = lan.ProcesoControl(envio);

                _logger.GuardarTrama(transaccionId, "ENVIADA", lan.TramaEnviada);
                _logger.GuardarTrama(transaccionId, "RESPUESTA", lan.TramaRespuesta);

                result.CodigoRespuesta = respuesta?.CodigoRespuesta;
                result.MensajeRespuesta = respuesta?.MensajeRespuesta;
                result.TramaPinPad = lan.TramaRespuesta; // TramaPinPad es privado en la DLL; usamos LAN.TramaRespuesta (público)
                result.Exitoso = respuesta != null && respuesta.CodigoRespuesta == "00";

                _logger.GuardarDetalleProcesoControl(transaccionId, lote, referencia);
                _logger.FinalizarTransaccion(transaccionId, result.CodigoRespuesta, result.MensajeRespuesta,
                    result.Exitoso, null);

                return result;
            }
            catch (Exception ex)
            {
                result.Exitoso = false;
                result.ExcepcionMensaje = ex.ToString();
                _logger.FinalizarTransaccion(transaccionId, null, null, false, ex.ToString());
                _logger.RegistrarEvento("Error", nameof(PinPadService), ex.ToString(), transaccionId);
                throw;
            }
        }

        // =================================================================
        // Helpers de mapeo
        //
        // IMPORTANTE: Los valores de TipoTransaccion y RedAdquirente que se envían a
        // EnvioProcesoPago NO corresponden a las clases públicas "TipoTrx" (0-based) ni
        // "Red" (0-based) — esas se usan para otra cosa internamente. Los valores reales
        // que espera el campo son 1-based, confirmados tanto por los enums internos
        // ETipoTransaccion/ERedAdquirente (no públicos, por eso van como literales aquí)
        // como por el propio simulador oficial "Simulador PinPad .NET", que documenta
        // textualmente en su pantalla de Proceso de Pago:
        //   "Tipo Trx: 1 - Corriente, 2 - Diferidos"
        //   "Red Adq: 1 - Datafast, 2 - Medianet, 3 - B. Austro"
        // =================================================================

        private static string MapTipoTransaccion(string tipoTransaccion)
        {
            switch ((tipoTransaccion ?? "Corriente").Trim().ToLowerInvariant())
            {
                case "corriente": return "1";
                case "diferido": return "2";
                case "anulacion": return "3";
                case "reverso": return "4";
                case "pagoelectronico": return "5";
                case "consulta": return "6";
                default:
                    throw new ArgumentException($"TipoTransaccion no reconocido: '{tipoTransaccion}'");
            }
        }

        /// <summary>
        /// IMPORTANTE: a diferencia de TipoTransaccion (que la DLL rellena internamente a 2
        /// dígitos con PadLeft antes de armar la trama), el campo RedAdquirente se inserta
        /// TAL CUAL en la trama, sin ningún relleno interno (confirmado en el IL de
        /// EnvioProcesoPago.ObtenerTrama). Por eso aquí debemos devolver ya el valor
        /// pre-rellenado a 2 dígitos nosotros mismos, o el campo queda corto y desalinea
        /// todo lo que sigue en la trama.
        /// </summary>
        /// <summary>
        /// IMPORTANTE — CORREGIDO: a diferencia de lo que se pensó en una iteración anterior,
        /// RedAdquirente va SIN relleno (1 solo carácter). Confirmado comparando byte a byte
        /// una trama real que sí llegó hasta Datafast (capturada del log de otro simulador):
        /// la posición de RedAdquirente ahí es un único dígito ("1"), no dos. Rellenarlo a 2
        /// dígitos desalinea todo el resto de la trama.
        /// </summary>
        private static string MapRed(string red)
        {
            switch ((red ?? "Datafast").Trim().ToLowerInvariant())
            {
                case "datafast": return "1";
                case "medianet": return "2";
                case "austro": return "3";
                default:
                    throw new ArgumentException($"Red no reconocida: '{red}'");
            }
        }

        /// <summary>
        /// Mapea al campo CodigoDiferido de EnvioProcesoPago usando DF_PinPad.TipoCredito
        /// (esta clase SÍ es pública, se referencia directamente — valores confirmados por
        /// reflection: Corriente=0, DiferidoCorriente=1, DiferidoConIntereses=2,
        /// DiferidoSinIntereses=3, DiferidoConInteresesConMesesDeGracia=4,
        /// DiferidoSinInteresesConMesesDeGracia=5, DiferidoPlus=6, DiferidoPlusCuotas=7).
        /// Si la transacción es Corriente, siempre se envía Corriente (0), independientemente
        /// de lo que traiga request.TipoCredito.
        /// </summary>
        private static string MapTipoCredito(string tipoTransaccion, string tipoCredito)
        {
            if (!string.Equals((tipoTransaccion ?? "").Trim(), "Diferido", StringComparison.OrdinalIgnoreCase))
                return ((int)TipoCredito.Corriente).ToString(CultureInfo.InvariantCulture);

            switch ((tipoCredito ?? "DiferidoCorriente").Trim().ToLowerInvariant())
            {
                case "diferidocorriente": return ((int)TipoCredito.DiferidoCorriente).ToString(CultureInfo.InvariantCulture);
                case "diferidoconintereses": return ((int)TipoCredito.DiferidoConIntereses).ToString(CultureInfo.InvariantCulture);
                case "diferidosinintereses": return ((int)TipoCredito.DiferidoSinIntereses).ToString(CultureInfo.InvariantCulture);
                case "diferidoconinteresesconmesesdegracia": return ((int)TipoCredito.DiferidoConInteresesConMesesDeGracia).ToString(CultureInfo.InvariantCulture);
                case "diferidosininteresesconmesesdegracia": return ((int)TipoCredito.DiferidoSinInteresesConMesesDeGracia).ToString(CultureInfo.InvariantCulture);
                case "diferidoplus": return ((int)TipoCredito.DiferidoPlus).ToString(CultureInfo.InvariantCulture);
                case "diferidopluscuotas": return ((int)TipoCredito.DiferidoPlusCuotas).ToString(CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentException($"TipoCredito no reconocido: '{tipoCredito}'");
            }
        }

        /// <summary>
        /// Mapea al campo OTTProveedor de EnvioProcesoPago. La DLL internamente usa
        /// DF_PinPad.TipoBilletera (OTTDiners=0, OTTPacifico=1), pero el simulador oficial
        /// muestra estas mismas 2 opciones con sus nombres comerciales reales:
        /// "PayClub" (=0) y "BDP Wallet" (=1). Vacío/null para pagos normales con tarjeta.
        /// </summary>
        private static string MapBilletera(string billetera)
        {
            if (string.IsNullOrWhiteSpace(billetera)) return string.Empty;

            switch (billetera.Trim().ToLowerInvariant())
            {
                case "payclub": return ((int)TipoBilletera.OTTDiners).ToString(CultureInfo.InvariantCulture);
                case "bdp wallet": return ((int)TipoBilletera.OTTPacifico).ToString(CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentException($"Billetera no reconocida: '{billetera}'");
            }
        }

        /// <summary>
        /// Genera una referencia numérica de 6 dígitos (formato exigido por la DLL:
        /// PadLeft(6,'0') interno — no acepta letras ni más de 6 dígitos). Usa los
        /// últimos 6 dígitos del tick count actual para evitar colisiones frecuentes.
        /// </summary>
        private static string GenerarReferenciaNumerica()
        {
            long ticks = DateTime.Now.Ticks;
            string digits = (ticks % 1000000).ToString("D6", CultureInfo.InvariantCulture);
            return digits;
        }

        /// <summary>
        /// Formato de monto enviado a la DLL. AJUSTAR si tu proveedor/versión LAN
        /// espera otro formato (no se puede confirmar solo con reflection de metadata;
        /// requiere una prueba real contra el pin pad).
        /// </summary>
        private static string FormatMonto(decimal monto)
        {
            return monto.ToString("0.00", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Para Servicio/Propina/MontoFijo: la DLL trata "" (vacío) distinto de un valor
        /// explícito. "" → se rellena internamente con ESPACIOS (12) = "no aplica".
        /// Cualquier otro valor → se rellena con CEROS (12) = "aplica, valor explícito".
        /// Confirmado comparando byte a byte una trama real que sí fue aceptada por
        /// Datafast: estos 3 campos iban en espacios, no en "000000000000".
        /// Por eso, si el monto es 0, se envía vacío en vez de "0.00".
        /// </summary>
        private static string FormatMontoOpcional(decimal monto)
        {
            return monto == 0 ? string.Empty : FormatMonto(monto);
        }

        public void VincularNumeroFactura(long transaccionId, string numeroFactura)
        {
            _logger.VincularNumeroFactura(transaccionId, numeroFactura);
        }
    }
}
