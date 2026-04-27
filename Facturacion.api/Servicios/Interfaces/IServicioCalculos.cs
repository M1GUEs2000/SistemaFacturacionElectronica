using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using System.Threading.Tasks;

namespace Facturacion.api.Servicios.Interfaces
{
    public interface IServicioCalculos
    {
        // Detecta el tipo de ICE a partir del nombre del producto
        Task<RespuestaGeneral<DtoResultadoICE>> DetectarICEAsync(string nombreProducto);

        // Recalcula el código ICE de vehículo según el PVP ingresado
        Task<RespuestaGeneral<DtoResultadoICE>> ObtenerICEVehiculoPorPrecioAsync(
            double pvp,
            string tipoVehiculo);
    }
}
