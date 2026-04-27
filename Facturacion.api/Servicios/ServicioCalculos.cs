using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using Facturacion.api.Servicios.Interfaces;
using LogicaNegocios.Procesos;
using System;
using System.Threading.Tasks;

namespace Facturacion.api.Servicios
{
    public class ServicioCalculos : IServicioCalculos
    {
        // Sin dependencias de BD — ProcesosICE es estático y puro
        public ServicioCalculos() { }

        // ======================================================
        // DETECTAR ICE POR NOMBRE
        // ======================================================

        public async Task<RespuestaGeneral<DtoResultadoICE>> DetectarICEAsync(string nombreProducto)
        {
            try
            {
                var resultado = await Task.Run(() => ProcesosICE.DetectarICE(nombreProducto));
                return RespuestaGeneral<DtoResultadoICE>.Ok(MapearResultado(resultado));
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoResultadoICE>
                    .Fail("Error detectando ICE: " + ex.Message);
            }
        }

        // ======================================================
        // RECALCULAR CÓDIGO VEHÍCULO POR PVP
        // ======================================================

        public async Task<RespuestaGeneral<DtoResultadoICE>> ObtenerICEVehiculoPorPrecioAsync(
            double pvp,
            string tipoVehiculo)
        {
            try
            {
                if (pvp <= 0)
                    return RespuestaGeneral<DtoResultadoICE>
                        .Fail("El PVP debe ser mayor a cero.");

                var resultado = await Task.Run(() =>
                    ProcesosICE.ObtenerICEVehiculoPorPrecio(pvp, tipoVehiculo));

                return RespuestaGeneral<DtoResultadoICE>.Ok(MapearResultado(resultado));
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoResultadoICE>
                    .Fail("Error calculando ICE de vehículo: " + ex.Message);
            }
        }

        // ======================================================
        // MAPPER interno ResultadoICE → DtoResultadoICE
        // ======================================================

        private static DtoResultadoICE MapearResultado(ResultadoICE r)
        {
            return new DtoResultadoICE
            {
                Detectado     = r.Detectado,
                Categoria     = r.Categoria,
                AplicaIce     = r.AplicaIce,
                IceCodigo     = r.IceCodigo,
                IceTipo       = r.IceTipo,
                IcePorcentaje = r.IcePorcentaje,
                IceValor      = r.IceValor,
                IceUnidad     = r.IceUnidad,
                TipoVehiculo  = r.TipoVehiculo,
                Mensaje       = r.Mensaje
            };
        }
    }
}
