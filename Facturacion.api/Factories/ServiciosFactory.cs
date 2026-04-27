using Facturacion.api.Servicios;
using Facturacion.api.Servicios.Interfaces;
using LogicaNegocios.Services;

namespace Facturacion.api.Factories
{
    public static class ServiciosFactory
    {
        public static IServicioCalculos CreateServicioCalculos()
        {
            return new ServicioCalculos();
        }

        public static IServicioCliente CreateServicioCliente(AppServices appServices)
        {
            return new ServicioCliente(appServices);
        }

        public static IServicioCrearBase CreateServicioCrearBase(AppServices appServices)
        {
            return new ServicioCrearBase(appServices);
        }

        public static IServicioEmpresa CreateServicioEmpresa(AppServices appServices)
        {
            return new ServicioEmpresa(appServices);
        }

        public static IServicioFactura CreateServicioFactura(AppServices appServices)
        {
            return new ServicioFactura(appServices);
        }

        public static IServicioFacturacionTabla CreateServicioFacturacionTabla(AppServices appServices)
        {
            return new ServicioFacturacionTabla(appServices);
        }

        public static IServicioFormaPago CreateServicioFormaPago(AppServices appServices)
        {
            return new ServicioFormaPago(appServices);
        }

        public static IServicioNota CreateServicioNota(AppServices appServices)
        {
            return new ServicioNota(appServices);
        }

        public static IServicioNotasCredito CreateServicioNotasCredito(AppServices appServices)
        {
            return new ServicioNotasCredito(appServices);
        }

        public static IServicioParametrosFacturas CreateServicioParametrosFacturas(AppServices appServices)
        {
            return new ServicioParametrosFacturas(appServices);
        }

        public static IServicioPendientes CreateServicioPendientes(AppServices appServices)
        {
            return new ServicioPendientes(appServices);
        }

        public static IServicioLogin CreateServicioLogin(AppServices appServices)
        {
            return new ServicioLogin(appServices);
        }

        public static IServicioProductos CreateServicioProductos(AppServices appServices)
        {
            return new ServicioProductos(appServices);
        }

        public static IServicioProveedores CreateServicioProveedores(AppServices appServices)
        {
            return new ServicioProveedores(appServices);
        }

        public static IServicioRetenciones CreateServicioRetenciones(AppServices appServices)
        {
            return new ServicioRetenciones(appServices);
        }

        public static IServicioRetencion CreateServicioRetencion(AppServices appServices)
        {
            return new ServicioRetencion(appServices);
        }

        public static IServicioSecuenciales CreateServicioSecuenciales(AppServices appServices)
        {
            return new ServicioSecuenciales(appServices);
        }
    }
}