using FluentValidation;
using Facturacion.api.Models.Solicitudes;

namespace Facturacion.api.Validators
{
    /// <summary>
    /// El login por usuario resuelve la identidad a partir de la clave de ingreso;
    /// el campo Usuario no lo consume el servicio, por eso no se exige aqui.
    /// </summary>
    public sealed class SolicitudLoginValidator : AbstractValidator<SolicitudLogin>
    {
        public SolicitudLoginValidator()
        {
            RuleFor(x => x.ClaveIngreso)
                .NotEmpty().WithMessage("La clave de ingreso es obligatoria.");
        }
    }
}
