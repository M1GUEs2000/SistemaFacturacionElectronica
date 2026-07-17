using FluentValidation;
using Facturacion.api.Models.Solicitudes;

namespace Facturacion.api.Validators
{
    public sealed class SolicitudFacturaValidator : AbstractValidator<SolicitudFactura>
    {
        public SolicitudFacturaValidator()
        {
            RuleFor(x => x.Detalles)
                .NotNull().WithMessage("La factura debe incluir al menos un detalle.")
                .Must(d => d != null && d.Count > 0)
                .WithMessage("La factura debe incluir al menos un detalle.");

            RuleForEach(x => x.Detalles)
                .SetValidator(new DetalleFacturaSolicitudValidator());
        }
    }

    public sealed class DetalleFacturaSolicitudValidator : AbstractValidator<DetalleFacturaSolicitud>
    {
        public DetalleFacturaSolicitudValidator()
        {
            RuleFor(x => x.Producto)
                .NotEmpty().WithMessage("Cada detalle requiere un producto.");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");

            RuleFor(x => x.Valor)
                .GreaterThanOrEqualTo(0).WithMessage("El valor unitario no puede ser negativo.");

            RuleFor(x => x.Total)
                .GreaterThanOrEqualTo(0).WithMessage("El total del detalle no puede ser negativo.");
        }
    }
}
