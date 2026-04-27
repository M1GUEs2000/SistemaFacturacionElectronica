using Facturacion.api.DTOs;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Facturacion.api.Mappers
{
    public static class MapperPendiente
    {
        public static DtoPendiente ToDto(DataRow row)
        {
            if (row == null) return null;

            return new DtoPendiente
            {
                NumeroFactura = row["NUMEROFACTURA"]?.ToString(),
                ClaveAcceso = row["CLAVEACCESO"]?.ToString(),
                RutaXmlFirmado = row["RUTAXMLFIRMADO"]?.ToString(),
                FechaRegistro = row["FECHAREGISTRO"] == DBNull.Value
                    ? DateTime.MinValue
                    : Convert.ToDateTime(row["FECHAREGISTRO"]),
                Intentos = row["INTENTOS"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(row["INTENTOS"]),
                Estado = row["ESTADO"]?.ToString(),
                Tipo = row["TIPO"]?.ToString()
            };
        }

        public static List<DtoPendiente> ToList(DataTable table)
        {
            if (table == null)
                return new List<DtoPendiente>();

            return table.AsEnumerable()
                        .Select(ToDto)
                        .ToList();
        }
    }
}