using FluentValidation;
using Facturacion.api.Models.Solicitudes;

namespace Facturacion.api.Validators
{
    public sealed class SolicitudCrearRetencionValidator : AbstractValidator<SolicitudCrearRetencion>
    {
        public SolicitudCrearRetencionValidator()
        {
            // No se valida el formato de la identificacion: el SRI admite pasaporte
            // (tipo 06) alfanumerico ademas de cedula/RUC.
            RuleFor(x => x.Identificacion)
                .NotEmpty().WithMessage("La identificacion del proveedor es obligatoria.");

            RuleFor(x => x.NumeroFactura)
                .NotEmpty().WithMessage("El numero de factura del proveedor es obligatorio.");

            RuleFor(x => x.BaseImponible)
                .GreaterThanOrEqualTo(0).WithMessage("La base imponible no puede ser negativa.");

            RuleFor(x => x.Total)
                .GreaterThanOrEqualTo(0).WithMessage("El total no puede ser negativo.");

            RuleFor(x => x.Conceptos)
                .NotNull().WithMessage("La retencion debe incluir al menos un concepto.")
                .Must(c => c != null && c.Count > 0)
                .WithMessage("La retencion debe incluir al menos un concepto.");

            RuleForEach(x => x.Conceptos)
                .SetValidator(new ConceptoRetencionSolicitudValidator());
        }
    }

    public sealed class ConceptoRetencionSolicitudValidator : AbstractValidator<ConceptoRetencionSolicitud>
    {
        public ConceptoRetencionSolicitudValidator()
        {
            RuleFor(x => x.Codigo)
                .NotEmpty().WithMessage("Cada concepto requiere un codigo.");

            RuleFor(x => x.BaseImponible)
                .GreaterThanOrEqualTo(0).WithMessage("La base imponible del concepto no puede ser negativa.");

            RuleFor(x => x.Porcentaje)
                .GreaterThanOrEqualTo(0).WithMessage("El porcentaje del concepto no puede ser negativo.");

            RuleFor(x => x.ValorRetenido)
                .GreaterThanOrEqualTo(0).WithMessage("El valor retenido no puede ser negativo.");
        }
    }
}
