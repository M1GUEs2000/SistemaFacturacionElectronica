using Facturacion.api.DTOs;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Facturacion.api.Mappers
{
    public static class MapperSecuencial
    {
        public static DtoSecuencial ToDto(DataRow row, int numeroDigitos)
        {
            if (row == null) return null;

            long sec = 1;
            long.TryParse(row["SECUENCIAL"]?.ToString(), out sec);

            string codigoNumerico = row["CODIGONUMERICO"]?.ToString()?.PadLeft(8, '0');

            return new DtoSecuencial
            {
                TipoComprobante = row["TIPOCOMPROBANTE"]?.ToString(),
                Secuencial = sec,
                SecuencialFormateado = sec.ToString().PadLeft(numeroDigitos, '0'),
                CodigoNumerico = codigoNumerico,
                FechaActualizacion = row["FECHAACTUALIZACION"]?.ToString()
            };
        }

        public static List<DtoSecuencial> ToList(DataTable table, int numeroDigitos)
        {
            if (table == null)
                return new List<DtoSecuencial>();

            return table.AsEnumerable()
                        .Select(row => ToDto(row, numeroDigitos))
                        .ToList();
        }
    }
}