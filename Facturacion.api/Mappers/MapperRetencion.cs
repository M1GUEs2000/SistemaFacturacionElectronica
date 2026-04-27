using Facturacion.api.DTOs;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Facturacion.api.Mappers
{
    public static class MapperRetencion
    {
        public static DtoRetencion ToDto(DataRow row)
        {
            return new DtoRetencion
            {
                NumeroRetencion = row["NUMERORETENCION"]?.ToString(),
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

                SujetoRetenido = row["SUJETORETENIDO"]?.ToString(),
                IdentificacionSujeto = row["IDENTIFICACIONSUJETO"]?.ToString(),
                TipoIdentificacionSujeto = row["TIPOIDENTIFICACIONSUJETO"]?.ToString(),
                DireccionSujeto = row["DIRECCIONSUJETO"]?.ToString(),
                RegimenSujeto = row["REGIMENSUJETO"]?.ToString(),

                TotalBaseImponible = row["TOTALBASEIMPONIBLE"]?.ToString(),
                TotalRetencionRenta = row["TOTALRETENCIONRENTA"]?.ToString(),
                TotalRetencionIva = row["TOTALRETENCIONIVA"]?.ToString(),
                TotalRetenido = row["TOTALRETENIDO"]?.ToString(),

                Observaciones = row["OBSERVACIONES"]?.ToString()
            };
        }

        public static DtoRetencionDetalle ToDetalle(DataRow row)
        {
            return new DtoRetencionDetalle
            {
                NumeroRetencion = row["NUMERORETENCION"]?.ToString(),
                TipoImpuesto = row["TIPOIMPUESTO"]?.ToString(),
                CodigoImpuesto = row["CODIGOIMPUESTO"]?.ToString(),
                BaseImponible = row["BASEIMPONIBLE"]?.ToString(),
                PorcentajeRetencion = row["PORCENTAJERETENCION"]?.ToString(),
                ValorRetenido = row["VALORRETENIDO"]?.ToString(),
                TipoOperacion = row["TIPOOPERACION"]?.ToString()
            };
        }
    }
}