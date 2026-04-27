using Facturacion.api.DTOs;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Facturacion.api.Mappers
{
    public static class MapperParametrosFactura
    {
        public static DtoParametrosFactura ToDto(DataRow row)
        {
            if (row == null) return null;

            return new DtoParametrosFactura
            {
                Nombre = row["NOMBRE"]?.ToString(),
                Ambiente = row["AMBIENTE"]?.ToString(),
                TipoEmision = row["TIPOEMISION"]?.ToString(),
                AgenteRetencion = row["AGENTERETENCION"]?.ToString(),
                ContribuyenteRimpe = row["CONTRIBUYENTERIMPE"]?.ToString(),
                CodDoc = row["CODDOC"]?.ToString(),
                Estab = row["ESTAB"]?.ToString(),
                PuntoEmision = row["PUNTOEMISION"]?.ToString(),
                NumeroDigitos = row["NUMERODIGITOS"]?.ToString(),
                ContribuyenteEspecial = row["CONTRIBUYENTEESPECIAL"]?.ToString(),
                ObligadoContabilidad = row["OBLIGADOCONTABILIDAD"]?.ToString(),
                TipoIdentComprador = row["TIPOIDENTIFICADORCOMPRADOR"]?.ToString(),
                Moneda = row["MONEDA"]?.ToString(),
                CodigoImpuesto = row["CODIGOIMPUESTO"]?.ToString(),
                CodigoPorcentaje = row["CODIGOPORCENTAJE"]?.ToString(),
                FechaActualizacion = row["FECHAACTUALIZACION"]?.ToString(),
                SmtpServer = row["SMTPSERVER"]?.ToString(),
                SmtpPort = row["SMTPPORT"]?.ToString(),
                SmtpUser = row["SMTPUSER"]?.ToString(),
                SmtpPass = row["SMTPPASS"]?.ToString()
            };
        }

        public static List<DtoParametrosFactura> ToList(DataTable table)
        {
            if (table == null)
                return new List<DtoParametrosFactura>();

            return table.AsEnumerable()
                        .Select(ToDto)
                        .ToList();
        }
    }
}