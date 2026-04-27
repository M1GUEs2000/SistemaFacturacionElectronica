using Facturacion.api.Models.DTOs;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace Facturacion.api.Mappers
{
    public static class MapperEmpresa
    {
        public static DtoEmpresa ToDto(DataRow row)
        {
            if (row == null) return null;

            return new DtoEmpresa
            {
                Nombre = row["NOMBRE"]?.ToString(),
                Direccion = row["DIRECCION"]?.ToString(),
                UsuarioLogin = row["USUARIO"]?.ToString(),

                ClaveIngreso = row["CLAVEINGRESO"]?.ToString(),
                ClaveTotales = row["CLAVETOTALES"]?.ToString(),
                ClaveEliminar = row["CLAVEELIMINACION"]?.ToString(),
                ClaveConsulta = row["CLAVECONSULTA"]?.ToString(),
                ClaveTabla = row["CLAVETABLAS"]?.ToString(),

                Facturacion = row["FACTURACION"]?.ToString(),
                Impresion = row["IMPRESION"]?.ToString(),

                Telefono = row["TELEFONO"]?.ToString(),
                Propietario = row["PROPIETARIO"]?.ToString(),
                Email = row["EMAIL"]?.ToString(),

                UbicacionArchivoP12 = row["UBICACIONARCHIVOP12"]?.ToString(),
                Contrasena = row["CONTRASENA"]?.ToString(),

                Imagen = row["IMAGEN"]?.ToString(),
                EstadoRuc = row["ESTADORUC"]?.ToString(),
                Ruc = row["RUC"]?.ToString()
            };
        }

        public static List<DtoEmpresa> ToList(DataTable table)
        {
            if (table == null) return new List<DtoEmpresa>();

            return table.AsEnumerable()
                        .Select(ToDto)
                        .ToList();
        }
    }
}