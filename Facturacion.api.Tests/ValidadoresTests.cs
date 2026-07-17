using System.Collections.Generic;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Validators;
using FluentValidation;
using Xunit;

namespace Facturacion.api.Tests
{
    public class ValidadoresTests
    {
        // ==========================================================
        // LOGIN — el servicio solo usa ClaveIngreso, no Usuario.
        // ==========================================================
        [Fact]
        public void Login_SinClave_EsInvalido()
        {
            var r = new SolicitudLoginValidator().Validate(
                new SolicitudLogin { Usuario = "cualquiera", ClaveIngreso = "" });

            Assert.False(r.IsValid);
            Assert.Contains(r.Errors, e => e.PropertyName == "ClaveIngreso");
        }

        [Fact]
        public void Login_SinUsuarioPeroConClave_EsValido()
        {
            // Regresion: no debemos exigir Usuario, o rompemos el login real.
            var r = new SolicitudLoginValidator().Validate(
                new SolicitudLogin { Usuario = null, ClaveIngreso = "1234" });

            Assert.True(r.IsValid);
        }

        // ==========================================================
        // FACTURA
        // ==========================================================
        [Fact]
        public void Factura_SinDetalles_EsInvalida()
        {
            var r = new SolicitudFacturaValidator().Validate(
                new SolicitudFactura { Detalles = new List<DetalleFacturaSolicitud>() });

            Assert.False(r.IsValid);
        }

        [Fact]
        public void Factura_ConDetalleValido_EsValida()
        {
            var r = new SolicitudFacturaValidator().Validate(new SolicitudFactura
            {
                Detalles = new List<DetalleFacturaSolicitud>
                {
                    new DetalleFacturaSolicitud
                    {
                        Producto = "Coca Cola",
                        Cantidad = 2,
                        Valor = 1.50m,
                        Total = 3.00m,
                        Codigo = "001"
                    }
                }
            });

            Assert.True(r.IsValid);
        }

        [Fact]
        public void Factura_DetalleConCantidadCero_EsInvalida()
        {
            var r = new SolicitudFacturaValidator().Validate(new SolicitudFactura
            {
                Detalles = new List<DetalleFacturaSolicitud>
                {
                    new DetalleFacturaSolicitud { Producto = "X", Cantidad = 0, Valor = 1m, Total = 1m }
                }
            });

            Assert.False(r.IsValid);
        }

        [Fact]
        public void Factura_DetalleConValorNegativo_EsInvalida()
        {
            var r = new SolicitudFacturaValidator().Validate(new SolicitudFactura
            {
                Detalles = new List<DetalleFacturaSolicitud>
                {
                    new DetalleFacturaSolicitud { Producto = "X", Cantidad = 1, Valor = -1m, Total = 1m }
                }
            });

            Assert.False(r.IsValid);
        }

        // ==========================================================
        // PENDIENTES
        // ==========================================================
        [Fact]
        public void ProcesarPendiente_SinTipoNiNumero_EsInvalido()
        {
            var r = new SolicitudProcesarPendienteDocumentoValidator().Validate(
                new SolicitudProcesarPendienteDocumento { TipoDocumento = "", NumeroDocumento = "" });

            Assert.False(r.IsValid);
            Assert.Equal(2, r.Errors.Count);
        }

        [Fact]
        public void ProcesarPendiente_Completo_EsValido()
        {
            var r = new SolicitudProcesarPendienteDocumentoValidator().Validate(
                new SolicitudProcesarPendienteDocumento
                {
                    TipoDocumento = "FACTURA",
                    NumeroDocumento = "001-001-000000123"
                });

            Assert.True(r.IsValid);
        }

        // ==========================================================
        // CONSUMIDOR FINAL
        // ==========================================================
        [Fact]
        public void ConsumidorFinal_SinNumeroFactura_EsInvalido()
        {
            var r = new SolicitudProcesarFacturaConsumidorFinalValidator().Validate(
                new SolicitudProcesarFacturaConsumidorFinal { NumeroFactura = "" });

            Assert.False(r.IsValid);
        }

        // ==========================================================
        // NOTA DE CREDITO — requiere identificar la factura de origen.
        // ==========================================================
        [Fact]
        public void NotaCredito_SinOrigen_EsInvalida()
        {
            var r = new SolicitudNotaCreditoValidator().Validate(
                new SolicitudNotaCredito { NumeroFactura = "", ClaveAccesoFactura = "" });

            Assert.False(r.IsValid);
        }

        [Fact]
        public void NotaCredito_ConClaveAccesoFactura_EsValida()
        {
            var r = new SolicitudNotaCreditoValidator().Validate(
                new SolicitudNotaCredito { NumeroFactura = "", ClaveAccesoFactura = "2207..." });

            Assert.True(r.IsValid);
        }

        // ==========================================================
        // RETENCION
        // ==========================================================
        [Fact]
        public void Retencion_SinConceptos_EsInvalida()
        {
            var r = new SolicitudCrearRetencionValidator().Validate(new SolicitudCrearRetencion
            {
                Identificacion = "1790012345001",
                NumeroFactura = "001-001-000000001",
                Conceptos = new List<ConceptoRetencionSolicitud>()
            });

            Assert.False(r.IsValid);
        }

        [Fact]
        public void Retencion_Completa_EsValida()
        {
            var r = new SolicitudCrearRetencionValidator().Validate(new SolicitudCrearRetencion
            {
                Identificacion = "1790012345001",
                NumeroFactura = "001-001-000000001",
                BaseImponible = 100m,
                Total = 108m,
                Conceptos = new List<ConceptoRetencionSolicitud>
                {
                    new ConceptoRetencionSolicitud
                    {
                        Codigo = "312",
                        Descripcion = "Bienes",
                        BaseImponible = 100m,
                        Porcentaje = 1m,
                        ValorRetenido = 1m,
                        TipoImpuesto = "RENTA"
                    }
                }
            });

            Assert.True(r.IsValid);
        }

        [Fact]
        public void Retencion_ConPasaporteAlfanumerico_EsValida()
        {
            // No validamos formato: el SRI admite pasaporte (tipo 06) alfanumerico.
            var r = new SolicitudCrearRetencionValidator().Validate(new SolicitudCrearRetencion
            {
                Identificacion = "AB123456",
                NumeroFactura = "001-001-000000001",
                Conceptos = new List<ConceptoRetencionSolicitud>
                {
                    new ConceptoRetencionSolicitud { Codigo = "312", BaseImponible = 10m, Porcentaje = 1m, ValorRetenido = 0.1m }
                }
            });

            Assert.True(r.IsValid);
        }

        // ==========================================================
        // REGISTRO tipo -> validador
        // ==========================================================
        [Fact]
        public void Registro_MapeaLosTiposEsperados()
        {
            Assert.NotNull(ValidadorRegistro.ObtenerValidador(typeof(SolicitudLogin)));
            Assert.NotNull(ValidadorRegistro.ObtenerValidador(typeof(SolicitudFactura)));
            Assert.NotNull(ValidadorRegistro.ObtenerValidador(typeof(SolicitudProcesarPendienteDocumento)));
            Assert.NotNull(ValidadorRegistro.ObtenerValidador(typeof(SolicitudProcesarFacturaConsumidorFinal)));
            Assert.NotNull(ValidadorRegistro.ObtenerValidador(typeof(SolicitudNotaCredito)));
            Assert.NotNull(ValidadorRegistro.ObtenerValidador(typeof(SolicitudCrearRetencion)));

            // Un tipo sin validador registrado no debe romper: retorna null.
            Assert.Null(ValidadorRegistro.ObtenerValidador(typeof(SolicitudFacturacion)));
        }
    }
}
