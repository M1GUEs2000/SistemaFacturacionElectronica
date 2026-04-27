using Facturacion.api.Models.DTOs;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Facturacion.api.Mappers
{
    public static class MapperCliente
    {
        public static DtoCliente ToDto(DataRow row)
        {
            if (row == null) return null;

            return new DtoCliente
            {
                Cedula = row["CEDULA"]?.ToString(),
                Nombre = row["NOMBRE"]?.ToString(),
                Correo = row["CORREO"]?.ToString(),
                Direccion = row["DIRECCION"]?.ToString(),
                Telefono = row["TELEFONO"]?.ToString()
            };
        }

        public static List<DtoCliente> ToList(DataTable table)
        {
            if (table == null) return new List<DtoCliente>();

            return table.AsEnumerable()
                        .Select(ToDto)
                        .ToList();
        }
    }
}