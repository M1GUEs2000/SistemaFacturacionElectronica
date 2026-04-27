using Facturacion.api.DTOs;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace Facturacion.api.Mappers
{
    public static class MapperProducto
    {
        public static DtoProducto ToDto(DataRow row)
        {
            if (row == null) return null;

            return new DtoProducto
            {
                Nombre        = row["NOMBRE"]?.ToString(),
                Valor         = Col(row, "VALOR"),
                Estatus       = Col(row, "ESTATUS"),
                Orden         = Col(row, "ORDEN"),
                Codigo        = Col(row, "CODIGO"),
                Iva           = Col(row, "IVA"),
                AplicaIce     = Col(row, "APLICA_ICE"),
                IceCodigo     = Col(row, "ICE_CODIGO"),
                IceTipo       = Col(row, "ICE_TIPO"),
                IcePorcentaje = Col(row, "ICE_PORCENTAJE"),
                IceValor      = Col(row, "ICE_VALOR"),
                IceUnidad     = Col(row, "ICE_UNIDAD"),
                AzucarGramos  = Col(row, "AZUCAR_GRAMOS"),
                Volumen       = Col(row, "VOLUMEN"),
                Pvp           = Col(row, "PVP"),
                TipoVehiculo  = Col(row, "TIPO_VEHICULO")
            };
        }

        public static List<DtoProducto> ToList(DataTable table)
        {
            if (table == null)
                return new List<DtoProducto>();

            return table.AsEnumerable()
                        .Select(ToDto)
                        .ToList();
        }

        private static string Col(DataRow row, string col)
        {
            return row.Table.Columns.Contains(col) ? row[col]?.ToString() : null;
        }
    }
}
