using GenerarXml.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicaNegocios.Procesos
{
    /// <summary>
    /// Punto único de clasificación para los identificadores devueltos por el SRI.
    /// No ejecuta reintentos ni modifica documentos: solo indica al proceso llamador
    /// cuál es la acción correcta para el error recibido.
    /// </summary>
    public class ProcesosErroresSri
    {
        private static readonly Dictionary<string, DefinicionErrorSri> ErroresConocidos =
            new Dictionary<string, DefinicionErrorSri>(StringComparer.OrdinalIgnoreCase)
            {
                { "2", Definir("RUC del emisor no activo.", "Verifique que el RUC del emisor se encuentre en estado ACTIVO.", ValidacionSri.Autorizacion, AccionErrorSri.NoReintentar) },
                { "10", Definir("Establecimiento del emisor clausurado.", "El SRI no autoriza comprobantes de establecimientos clausurados.", ValidacionSri.Autorizacion, AccionErrorSri.NoReintentar) },
                { "26", Definir("Tamaño máximo superado.", "El archivo enviado supera el tamaño permitido por el SRI.", ValidacionSri.Recepcion, AccionErrorSri.NoReintentar) },
                { "27", Definir("Clase de contribuyente no permitida.", "La clase del contribuyente no puede emitir comprobantes electrónicos.", ValidacionSri.Autorizacion, AccionErrorSri.NoReintentar) },
                { "28", Definir("Acuerdo de medios electrónicos no aceptado.", "El contribuyente debe aceptar el acuerdo de medios electrónicos.", ValidacionSri.Recepcion, AccionErrorSri.NoReintentar) },
                { "35", Definir("Documento inválido.", "El XML no pasa la validación del esquema XSD.", ValidacionSri.Recepcion, AccionErrorSri.NoReintentar) },
                { "36", Definir("Versión de esquema descontinuada.", "La versión del esquema XML no es la correcta.", ValidacionSri.Recepcion, AccionErrorSri.NoReintentar) },
                { "37", Definir("RUC sin autorización de emisión.", "El RUC no cuenta con autorización para emitir comprobantes electrónicos.", ValidacionSri.Autorizacion, AccionErrorSri.NoReintentar) },
                { "39", Definir("Firma inválida.", "La firma electrónica del emisor no es válida.", ValidacionSri.Autorizacion, AccionErrorSri.NoReintentar) },
                { "40", Definir("Error en el certificado.", "No se encontró el certificado o no se puede convertir a X509.", ValidacionSri.Autorizacion, AccionErrorSri.NoReintentar) },
                { "43", Definir("Clave de acceso registrada.", "La clave de acceso ya existe en el SRI.", ValidacionSri.Recepcion, AccionErrorSri.CambiarSecuencial) },
                { "45", Definir("Secuencial registrado.", "El secuencial del comprobante ya existe en el SRI.", ValidacionSri.Recepcion, AccionErrorSri.CambiarSecuencial) },
                { "46", Definir("RUC no existe.", "El RUC del emisor no existe en el Registro Único de Contribuyentes.", ValidacionSri.Autorizacion, AccionErrorSri.NoReintentar) },
                { "47", Definir("Tipo de comprobante no existe.", "El tipo de comprobante enviado no existe en el catálogo del SRI.", ValidacionSri.Recepcion, AccionErrorSri.NoReintentar) },
                { "48", Definir("Esquema XSD no existe.", "No existe un esquema para el tipo de comprobante enviado.", ValidacionSri.Recepcion, AccionErrorSri.NoReintentar) },
                { "49", Definir("Argumentos del servicio web nulos.", "El servicio web fue consumido con argumentos nulos.", ValidacionSri.Recepcion, AccionErrorSri.NoReintentar) },
                { "50", Definir("Error interno general del SRI.", "El SRI reportó un error inesperado en su servidor.", ValidacionSri.Recepcion, AccionErrorSri.DejarPendienteParaReintento) },
                { "52", Definir("Error en diferencias.", "Existen diferencias en los cálculos del comprobante.", ValidacionSri.Autorizacion, AccionErrorSri.NoReintentar) },
                { "56", Definir("Establecimiento cerrado.", "El establecimiento desde el que se genera el comprobante se encuentra cerrado.", ValidacionSri.Autorizacion, AccionErrorSri.NoReintentar) },
                { "57", Definir("Autorización suspendida.", "La autorización de emisión electrónica se encuentra suspendida o no era válida en la fecha de emisión.", ValidacionSri.Autorizacion, AccionErrorSri.NoReintentar) },
                { "58", Definir("Error en la estructura de la clave de acceso.", "La clave de acceso tiene componentes diferentes a los del comprobante.", ValidacionSri.Autorizacion, AccionErrorSri.NoReintentar) },
                { "63", Definir("RUC clausurado.", "El RUC del emisor se encuentra clausurado por la Administración Tributaria.", ValidacionSri.Autorizacion, AccionErrorSri.NoReintentar) },
                { "65", Definir("Fecha de emisión extemporánea.", "El comprobante fue enviado fuera del plazo permitido para su tipo de emisión.", ValidacionSri.EmisorORecepcion, AccionErrorSri.NoReintentar) },
                { "67", Definir("Fecha inválida.", "La fecha enviada tiene un formato inválido.", ValidacionSri.Recepcion, AccionErrorSri.NoReintentar) },
                { "70", Definir("Clave de acceso en procesamiento.", "El comprobante ya fue enviado y el SRI aún lo está procesando.", ValidacionSri.Recepcion, AccionErrorSri.ConsultarAutorizacion) },
                { "80", Definir("Error en la estructura de la clave de acceso.", "La clave consultada supera 49 dígitos, contiene caracteres inválidos o está vacía.", ValidacionSri.Autorizacion, AccionErrorSri.NoReintentar) },
                { "82", Definir("Error en fecha de inicio de transporte.", "La fecha de inicio de transporte es menor a la fecha de emisión de la guía de remisión.", ValidacionSri.Recepcion, AccionErrorSri.NoReintentar) },
                { "92", Definir("Error al validar monto de devolución del IVA.", "El monto de devolución del IVA no coincide con el autorizado por el servicio web DIG.", ValidacionSri.Recepcion, AccionErrorSri.NoReintentar) }
            };

        public ResultadoErrorSri Clasificar(IEnumerable<SriMensajeDto> mensajesSri)
        {
            if (mensajesSri == null)
                return ResultadoErrorSri.SinCodigo();

            var mensajes = mensajesSri
                .Where(m => m != null)
                .ToList();

            var errores = mensajes
                .Where(m => !string.IsNullOrWhiteSpace(m.Identificador))
                .Select(Clasificar)
                .ToList();

            if (errores.Count == 0)
                return mensajes.Count == 0
                    ? ResultadoErrorSri.SinCodigo()
                    : Clasificar(mensajes[0]);

            // Una clave en procesamiento tiene prioridad: jamás debe reenviarse el XML.
            return errores.FirstOrDefault(e => e.Accion == AccionErrorSri.ConsultarAutorizacion)
                ?? errores.FirstOrDefault(e => e.Accion == AccionErrorSri.CambiarSecuencial)
                ?? errores.FirstOrDefault(e => e.Accion == AccionErrorSri.DejarPendienteParaReintento)
                ?? errores[0];
        }

        public ResultadoErrorSri Clasificar(SriMensajeDto mensajeSri)
        {
            if (mensajeSri == null)
                return ResultadoErrorSri.SinCodigo();

            string codigo = NormalizarCodigo(mensajeSri.Identificador);
            DefinicionErrorSri definicion;

            if (!string.IsNullOrWhiteSpace(codigo) && ErroresConocidos.TryGetValue(codigo, out definicion))
            {
                return new ResultadoErrorSri
                {
                    Codigo = codigo,
                    Conocido = true,
                    Descripcion = definicion.Descripcion,
                    Motivo = definicion.Motivo,
                    Validacion = definicion.Validacion,
                    Accion = definicion.Accion,
                    MensajeSri = mensajeSri.Mensaje ?? "",
                    InformacionAdicionalSri = mensajeSri.InformacionAdicional ?? "",
                    TipoSri = mensajeSri.Tipo ?? ""
                };
            }

            return new ResultadoErrorSri
            {
                Codigo = codigo,
                Conocido = false,
                Descripcion = "Código de error SRI no catalogado.",
                Motivo = "El SRI devolvió un identificador que aún no está manejado por el sistema.",
                Validacion = ValidacionSri.Desconocida,
                Accion = AccionErrorSri.RevisionManual,
                MensajeSri = mensajeSri.Mensaje ?? "",
                InformacionAdicionalSri = mensajeSri.InformacionAdicional ?? "",
                TipoSri = mensajeSri.Tipo ?? ""
            };
        }

        private static string NormalizarCodigo(string codigo)
        {
            return string.IsNullOrWhiteSpace(codigo) ? "" : codigo.Trim();
        }

        private static DefinicionErrorSri Definir(
            string descripcion,
            string motivo,
            ValidacionSri validacion,
            AccionErrorSri accion)
        {
            return new DefinicionErrorSri
            {
                Descripcion = descripcion,
                Motivo = motivo,
                Validacion = validacion,
                Accion = accion
            };
        }

        private class DefinicionErrorSri
        {
            public string Descripcion { get; set; }
            public string Motivo { get; set; }
            public ValidacionSri Validacion { get; set; }
            public AccionErrorSri Accion { get; set; }
        }
    }

    public enum AccionErrorSri
    {
        RevisionManual,
        NoReintentar,
        CambiarSecuencial,
        ConsultarAutorizacion,
        DejarPendienteParaReintento
    }

    public enum ValidacionSri
    {
        Desconocida,
        Recepcion,
        Autorizacion,
        EmisorORecepcion
    }

    public class ResultadoErrorSri
    {
        public string Codigo { get; set; }
        public bool Conocido { get; set; }
        public string Descripcion { get; set; }
        public string Motivo { get; set; }
        public ValidacionSri Validacion { get; set; }
        public AccionErrorSri Accion { get; set; }
        public string MensajeSri { get; set; }
        public string InformacionAdicionalSri { get; set; }
        public string TipoSri { get; set; }

        public static ResultadoErrorSri SinCodigo()
        {
            return new ResultadoErrorSri
            {
                Codigo = "",
                Conocido = false,
                Descripcion = "El SRI no devolvió un código de error.",
                Motivo = "No existe un identificador numérico que permita clasificar la respuesta.",
                Validacion = ValidacionSri.Desconocida,
                Accion = AccionErrorSri.RevisionManual,
                MensajeSri = "",
                InformacionAdicionalSri = "",
                TipoSri = ""
            };
        }
    }
}
