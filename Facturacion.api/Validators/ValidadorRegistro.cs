using System;
using System.Collections.Generic;
using FluentValidation;
using Facturacion.api.Models.Solicitudes;

namespace Facturacion.api.Validators
{
    /// <summary>
    /// Mapa tipo de solicitud -> validador. WebAPI 2 no tiene integracion
    /// automatica de FluentValidation, asi que el registro es explicito y
    /// lo consume <see cref="ValidacionActionFilter"/>.
    /// </summary>
    public static class ValidadorRegistro
    {
        private static readonly Dictionary<Type, IValidator> _validadores =
            new Dictionary<Type, IValidator>
            {
                { typeof(SolicitudLogin), new SolicitudLoginValidator() },
                { typeof(SolicitudFactura), new SolicitudFacturaValidator() },
                { typeof(SolicitudProcesarPendienteDocumento), new SolicitudProcesarPendienteDocumentoValidator() },
                { typeof(SolicitudProcesarFacturaConsumidorFinal), new SolicitudProcesarFacturaConsumidorFinalValidator() },
                { typeof(SolicitudNotaCredito), new SolicitudNotaCreditoValidator() },
                { typeof(SolicitudCrearRetencion), new SolicitudCrearRetencionValidator() },
            };

        public static IValidator ObtenerValidador(Type tipo)
        {
            IValidator validador;
            return _validadores.TryGetValue(tipo, out validador) ? validador : null;
        }
    }
}
