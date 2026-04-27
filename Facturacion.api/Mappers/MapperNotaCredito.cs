using Facturacion.api.DTOs;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Facturacion.api.Mappers
{
    public static class MapperNotaCredito
    {
        public static DtoNotaCredito ToDto(DataRow row)
        {
            if (row == null) return null;

            return new DtoNotaCredito
            {
                NumeroNota = row["NUMERONOTA"]?.ToString(),
                ClaveAcceso = row["CLAVEACCESO"]?.ToString(),
                FechaEmision = row["FECHAEMISION"]?.ToString(),
                HoraEmision = row["HORAEMISION"]?.ToString(),
                Ambiente = row["AMBIENTE"]?.ToString(),
                Estado = row["ESTADO"]?.ToString(),
                Codigo = row["CODIGO"]?.ToString(),
                TipoEmision = row["TIPOEMISION"]?.ToString(),
                NumeroFactura = row["NUMEROFACTURA"]?.ToString(),
                ClaveAccesoFactura = row["CLAVEACCESOFACTURA"]?.ToString(),
                FechaFactura = row["FECHAFACTURA"]?.ToString(),
                Motivo = row["MOTIVO"]?.ToString(),
                Cliente = row["CLIENTE"]?.ToString(),
                TotalSinImpuestos = row["TOTALSINIMPUESTOS"]?.ToString(),
                TotalConImpuestos = row["TOTALCONIMPUESTOS"]?.ToString(),
                CreditoUsado = row["CREDITOUSADO"]?.ToString()
            };
        }

        public static List<DtoNotaCredito> ToList(DataTable table)
        {
            if (table == null)
                return new List<DtoNotaCredito>();

            return table.AsEnumerable()
                        .Select(ToDto)
                        .ToList();
        }

        public static DtoNotaCreditoListado ToListadoDto(DataRow row)
        {
            if (row == null) return null;

            return new DtoNotaCreditoListado
            {
                NumeroNota = row["NUMERONOTA"]?.ToString(),
                NumeroFactura = row["NUMEROFACTURA"]?.ToString(),
                FechaEmision = row["FECHAEMISION"]?.ToString(),
                Cliente = row["CLIENTE"]?.ToString(),
                Cedula = row["CEDULA"]?.ToString(),
                Total = row["TOTAL"]?.ToString(),
                CreditoUsado = row["CREDITOUSADO"]?.ToString(),
                Motivo = row["MOTIVO"]?.ToString()
            };
        }

        public static List<DtoNotaCreditoListado> ToListadoList(DataTable table)
        {
            if (table == null)
                return new List<DtoNotaCreditoListado>();

            return table.AsEnumerable()
                        .Select(ToListadoDto)
                        .ToList();
        }

        public static DtoNotaCreditoDetalle ToDetalleDto(DataRow row)
        {
            if (row == null) return null;

            return new DtoNotaCreditoDetalle
            {
                Producto = row["PRODUCTO"]?.ToString(),
                Cantidad = row["CANTIDAD"]?.ToString(),
                Precio = row["PRECIO"]?.ToString(),
                Iva = row["IVA"]?.ToString()
            };
        }

        public static List<DtoNotaCreditoDetalle> ToDetalleList(DataTable table)
        {
            if (table == null)
                return new List<DtoNotaCreditoDetalle>();

            return table.AsEnumerable()
                        .Select(ToDetalleDto)
                        .ToList();
        }
    }
}