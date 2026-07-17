using FluentValidation;
using Facturacion.api.Models.Solicitudes;

namespace Facturacion.api.Validators
{
    public sealed class SolicitudNotaCreditoValidator : AbstractValidator<SolicitudNotaCredito>
    {
        public SolicitudNotaCreditoValidator()
        {
            // Una nota de credito siempre modifica una factura de origen: se exige
            // al menos un identificador de esa factura (numero o clave de acceso).
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.NumeroFactura)
                        || !string.IsNullOrWhiteSpace(x.ClaveAccesoFactura))
                .WithMessage("La nota de credito requiere identificar la factura de origen (numero o clave de acceso).");
        }
    }
}
