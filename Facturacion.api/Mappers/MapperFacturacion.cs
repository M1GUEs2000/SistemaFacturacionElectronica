using Facturacion.api.DTOs;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace Facturacion.api.Mappers
{
    public static class MapperFacturacion
    {
        public static DtoFacturacion ToDto(DataRow row)
        {
            if (row == null) return null;

            return new DtoFacturacion
            {
                Fecha = row["FECHA"]?.ToString(),
                FormaDePago = row["FORMADEPAGO"]?.ToString(),
                Producto = row["PRODUCTO"]?.ToString(),
                Cantidad = row["CANTIDAD"]?.ToString(),
                Total = row["TOTAL"]?.ToString(),
                Cliente = row["CLIENTE"]?.ToString(),
                Hora = row["HORA"]?.ToString(),
                NumeroFactura = row["NUMEROFACTURA"]?.ToString()
            };
        }

        public static List<DtoFacturacion> ToList(DataTable table)
        {
            if (table == null) return new List<DtoFacturacion>();

            return table.AsEnumerable()
                        .Select(ToDto)
                        .ToList();
        }
    }
}