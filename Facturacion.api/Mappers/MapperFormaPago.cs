using Facturacion.api.DTOs;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Facturacion.api.Mappers
{
    public static class MapperFormaPago
    {
        public static DtoFormaPago ToDto(DataRow row)
        {
            if (row == null) return null;

            return new DtoFormaPago
            {
                Formas = row["FORMAS"]?.ToString(),
                Imagen = row["IMAGEN"]?.ToString(),
                Codigo = row["CODIGO"]?.ToString()
            };
        }

        public static List<DtoFormaPago> ToList(DataTable table)
        {
            if (table == null)
                return new List<DtoFormaPago>();

            return table.AsEnumerable()
                        .Select(ToDto)
                        .ToList();
        }

        public static DtoTotalesFormaPago ToTotalesDto(DataRow row)
        {
            if (row == null) return null;

            return new DtoTotalesFormaPago
            {
                Fecha = row["FECHA"]?.ToString(),
                FormaPago = row["FORMADEPAGO"]?.ToString(),
                Cantidades = row["CANTIDADES"] == DBNull.Value ? 0 : Convert.ToDecimal(row["CANTIDADES"]),
                Totales = row["TOTALES"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TOTALES"])
            };
        }

        public static List<DtoTotalesFormaPago> ToTotalesList(DataTable table)
        {
            if (table == null)
                return new List<DtoTotalesFormaPago>();

            return table.AsEnumerable()
                        .Select(ToTotalesDto)
                        .ToList();
        }
    }
}