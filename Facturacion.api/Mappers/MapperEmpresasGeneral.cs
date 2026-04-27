using AccesoDatosWeb.Configuracion;
using Facturacion.api.Models.DTOs;
using System;

public static class MapperEmpresasGeneral
{
    public static DatabaseSettings toDatabaseSettings(DtoEmpresasGeneral dto_empresas_general)
    {
        if (dto_empresas_general == null)
            throw new ArgumentNullException(nameof(dto_empresas_general));

        return new DatabaseSettings
        {
            Provider = dto_empresas_general.Provider,
            Server = dto_empresas_general.ServerName,
            Database = dto_empresas_general.DatabaseName,
            User = dto_empresas_general.DbUser,
            Password = dto_empresas_general.DbPassword,
            TrustServerCertificate = dto_empresas_general.TrustServerCertificate
        };
    }
}