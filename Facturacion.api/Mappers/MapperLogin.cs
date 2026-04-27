using Facturacion.api.Models.DTOs;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Facturacion.api.Mappers
{
    public static class MapperLogin
    {
        public static DtoLogin ToDto(DataRow row)
        {
            if (row == null)
                return null;

            return new DtoLogin
            {
                Empresa = row["NOMBRE"]?.ToString(),
                ClaveIngreso = row["CLAVEINGRESO"]?.ToString(),
                ClaveEliminacion = row["CLAVEELIMINACION"]?.ToString(),
                ClaveTotales = row["CLAVETOTALES"]?.ToString(),
                ClaveConsulta = row["CLAVECONSULTA"]?.ToString(),
                ClaveTablas = row["CLAVETABLAS"]?.ToString(),
                Usuario = row["USUARIO"]?.ToString()
            };
        }
    }
}