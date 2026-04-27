using Facturacion.api.DTOs;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Facturacion.api.Mappers
{
    public static class MapperProveedor
    {
        public static DtoProveedor ToDto(DataRow row)
        {
            return new DtoProveedor
            {
                IdProveedor = Convert.ToInt32(row["IDPROVEEDOR"]),
                TipoIdentificacion = row["TIPOIDENTIFICACION"]?.ToString(),
                Identificacion = row["IDENTIFICACION"]?.ToString(),
                RazonSocial = row["RAZONSOCIAL"]?.ToString(),
                Correo = row["CORREO"]?.ToString(),
                Direccion = row["DIRECCION"]?.ToString(),
                Telefono = row["TELEFONO"]?.ToString(),
                TipoPersona = row["TIPOPERSONA"]?.ToString(),
                EsRimpe = Convert.ToBoolean(row["ESRIMPE"]),
                TipoRimpe = row["TIPORIMPE"]?.ToString(),
                EsProfesional = Convert.ToBoolean(row["ESPROFESIONAL"]),
                EsArrendador = Convert.ToBoolean(row["ESARRENDADOR"]),
                Estado = row["ESTADO"]?.ToString(),
                FechaRegistro = row.Table.Columns.Contains("FECHAREGISTRO")
                    ? row["FECHAREGISTRO"] as DateTime?
                    : null
            };
        }

        public static List<DtoProveedor> ToList(DataTable table)
        {
            var lista = new List<DtoProveedor>();

            foreach (DataRow row in table.Rows)
            {
                lista.Add(ToDto(row));
            }

            return lista;
        }
    }
}