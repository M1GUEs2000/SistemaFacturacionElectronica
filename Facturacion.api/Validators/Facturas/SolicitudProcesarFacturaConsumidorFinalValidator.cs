using FluentValidation;
using Facturacion.api.Models.Solicitudes;

namespace Facturacion.api.Validators
{
    public sealed class SolicitudProcesarFacturaConsumidorFinalValidator
        : AbstractValidator<SolicitudProcesarFacturaConsumidorFinal>
    {
        public SolicitudProcesarFacturaConsumidorFinalValidator()
        {
            RuleFor(x => x.NumeroFactura)
                .NotEmpty().WithMessage("El numero de factura es obligatorio.");
        }
    }
}
