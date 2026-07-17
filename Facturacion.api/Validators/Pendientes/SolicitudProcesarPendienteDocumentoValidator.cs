using FluentValidation;
using Facturacion.api.Models.Solicitudes;

namespace Facturacion.api.Validators
{
    public sealed class SolicitudProcesarPendienteDocumentoValidator
        : AbstractValidator<SolicitudProcesarPendienteDocumento>
    {
        public SolicitudProcesarPendienteDocumentoValidator()
        {
            RuleFor(x => x.TipoDocumento)
                .NotEmpty().WithMessage("El tipo de documento es obligatorio.");

            RuleFor(x => x.NumeroDocumento)
                .NotEmpty().WithMessage("El numero de documento es obligatorio.");
        }
    }
}
