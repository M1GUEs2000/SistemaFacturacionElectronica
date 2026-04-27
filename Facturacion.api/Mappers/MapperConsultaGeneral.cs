using Facturacion.api.DTOs;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace Facturacion.api.Mappers
{
    public static class MapperConsultaGeneral
    {
        public static DtoConsultaGeneral ToDto(DataRow row, string tipoDocumento)
        {
            if (row == null) return null;

            return new DtoConsultaGeneral
            {
                TipoDocumento = tipoDocumento,

                Fecha = row.Table.Columns.Contains("FECHA") ? row["FECHA"]?.ToString() : "",
                FechaEmision = row.Table.Columns.Contains("FECHAEMISION") ? row["FECHAEMISION"]?.ToString() : "",
                FechaFactura = row.Table.Columns.Contains("FECHAFACTURA") ? row["FECHAFACTURA"]?.ToString() : "",

                NumeroFactura = row.Table.Columns.Contains("NUMEROFACTURA") ? row["NUMEROFACTURA"]?.ToString() : "",
                NumeroRetencion = row.Table.Columns.Contains("NUMERORETENCION") ? row["NUMERORETENCION"]?.ToString() : "",
                NumeroNota = row.Table.Columns.Contains("NUMERONOTA") ? row["NUMERONOTA"]?.ToString() : "",

                Cliente = row.Table.Columns.Contains("CLIENTE") ? row["CLIENTE"]?.ToString() : "",
                Cedula = row.Table.Columns.Contains("CEDULA") ? row["CEDULA"]?.ToString() : "",
                SujetoRetenido = row.Table.Columns.Contains("SUJETORETENIDO") ? row["SUJETORETENIDO"]?.ToString() : "",
                Identificacion = row.Table.Columns.Contains("IDENTIFICACION") ? row["IDENTIFICACION"]?.ToString() : "",

                Producto = row.Table.Columns.Contains("PRODUCTO")
                    ? row["PRODUCTO"]?.ToString()
                    : row.Table.Columns.Contains("PRODUCTOS")
                        ? row["PRODUCTOS"]?.ToString()
                        : row.Table.Columns.Contains("NOMBRES DE PRODUCTOS")
                            ? row["NOMBRES DE PRODUCTOS"]?.ToString()
                            : "",

                FormaPago = row.Table.Columns.Contains("FORMA DE PAGO") ? row["FORMA DE PAGO"]?.ToString() : "",
                Cantidad = row.Table.Columns.Contains("CANTIDAD") ? row["CANTIDAD"]?.ToString() : "",

                Total = row.Table.Columns.Contains("TOTAL") ? row["TOTAL"]?.ToString() : "",
                TotalBaseImponible = row.Table.Columns.Contains("TOTALBASEIMPONIBLE") ? row["TOTALBASEIMPONIBLE"]?.ToString() : "",
                TotalRetencionRenta = row.Table.Columns.Contains("TOTALRETENCIONRENTA") ? row["TOTALRETENCIONRENTA"]?.ToString() : "",
                TotalRetencionIVA = row.Table.Columns.Contains("TOTALRETENCIONIVA") ? row["TOTALRETENCIONIVA"]?.ToString() : "",
                TotalRetenido = row.Table.Columns.Contains("TOTALRETENIDO") ? row["TOTALRETENIDO"]?.ToString() : "",

                CreditoUsado = row.Table.Columns.Contains("CREDITOUSADO") ? row["CREDITOUSADO"]?.ToString() : "",
                Motivo = row.Table.Columns.Contains("MOTIVO") ? row["MOTIVO"]?.ToString() : "",

                Hora = row.Table.Columns.Contains("HORA") ? row["HORA"]?.ToString() : "",

                Estado = row.Table.Columns.Contains("ESTADO") ? row["ESTADO"]?.ToString() : "",
                Accion = row.Table.Columns.Contains("ACCION") ? row["ACCION"]?.ToString() : "",
                ClaveAcceso = row.Table.Columns.Contains("CLAVEACCESO") ? row["CLAVEACCESO"]?.ToString() : "",
                RutaXmlFirmado = row.Table.Columns.Contains("RUTAXMLFIRMADO") ? row["RUTAXMLFIRMADO"]?.ToString() : "",
                FechaRegistro = row.Table.Columns.Contains("FECHAREGISTRO") ? row["FECHAREGISTRO"]?.ToString() : "",
                Intentos = row.Table.Columns.Contains("INTENTOS") ? row["INTENTOS"]?.ToString() : ""
            };
        }

        public static List<DtoConsultaGeneral> ToList(DataTable table, string tipoDocumento)
        {
            if (table == null) return new List<DtoConsultaGeneral>();

            return table.AsEnumerable()
                        .Select(r => ToDto(r, tipoDocumento))
                        .ToList();
        }
    }
}