namespace Facturacion.api.Configurations
{
    public static class ScriptCrearBase
    {
        public static string Obtener()
        {
            return @"
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.CLIENTE', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CLIENTE](
        [CEDULA]    [nvarchar](15)  NOT NULL,
        [NOMBRE]    [nvarchar](100) NULL,
        [CORREO]    [nvarchar](200) NULL,
        [DIRECCION] [nvarchar](200) NULL,
        [TELEFONO]  [nvarchar](20)  NULL
    )
END
GO

IF OBJECT_ID('dbo.EMPRESA', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[EMPRESA](
        [NOMBRE]                   [nvarchar](max) NOT NULL,
        [DIRECCION]                [nvarchar](max) NULL,
        [USUARIO]                  [nvarchar](255) NULL,
        [CLAVEINGRESO]             [nvarchar](255) NULL,
        [CLAVETOTALES]             [nvarchar](255) NULL,
        [CLAVEELIMINACION]         [nvarchar](255) NULL,
        [CLAVECONSULTA]            [nvarchar](255) NULL,
        [CLAVETABLAS]              [nvarchar](255) NULL,
        [FACTURACION]              [nvarchar](255) NULL,
        [IMPRESION]                [nvarchar](max) NULL,
        [TELEFONO]                 [nvarchar](max) NULL,
        [PROPIETARIO]              [nvarchar](max) NULL,
        [EMAIL]                    [nvarchar](max) NULL,
        [UBICACIONARCHIVOP12]      [nvarchar](max) NULL,
        [CONTRASENA]               [nvarchar](max) NULL,
        [IMAGEN]                   [nvarchar](max) NULL,
        [ESTADORUC]                [nvarchar](max) NULL,
        [RUC]                      [nvarchar](max) NULL,
        [CREDENCIALES_MODIFICADAS] [bit] NOT NULL CONSTRAINT [DF_EMPRESA_CRED_MOD] DEFAULT (0)
    )
END
GO

IF OBJECT_ID('dbo.FACTURACION', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[FACTURACION](
        [FECHA]         [nvarchar](255) NULL,
        [FORMADEPAGO]   [nvarchar](255) NULL,
        [PRODUCTO]      [nvarchar](255) NULL,
        [CANTIDAD]      [int]           NULL,
        [TOTAL]         [varchar](20)   NULL,
        [CLIENTE]       [nvarchar](50)  NULL,
        [HORA]          [nvarchar](255) NULL,
        [NUMEROFACTURA] [nvarchar](max) NULL
    )
END
GO

IF OBJECT_ID('dbo.FACTURAS_PENDIENTES', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[FACTURAS_PENDIENTES](
        [NUMEROFACTURA]  [nvarchar](30)  NOT NULL,
        [CLAVEACCESO]    [nvarchar](100) NULL,
        [RUTAXMLFIRMADO] [nvarchar](255) NULL,
        [FECHAREGISTRO]  [datetime]      NULL,
        [INTENTOS]       [int]           NULL,
        [ESTADO]         [nvarchar](100) NULL,
        [TIPO]           [nvarchar](255) NULL
    )
END
GO

IF OBJECT_ID('dbo.FORMASDEPAGO', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[FORMASDEPAGO](
        [FORMAS] [nvarchar](255) NOT NULL,
        [IMAGEN] [nvarchar](255) NULL,
        [CODIGO] [nvarchar](255) NULL
    )
END
GO

IF OBJECT_ID('dbo.LOG', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[LOG](
        [PROCESO] [nvarchar](max) NULL,
        [USUARIO] [nvarchar](max) NULL,
        [IP]      [nvarchar](max) NULL,
        [TEXTO]   [nvarchar](max) NULL,
        [FECHA]   [nvarchar](max) NULL
    )
END
GO

IF OBJECT_ID('dbo.NOTASCREDITO', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[NOTASCREDITO](
        [NUMERONOTA]         [nvarchar](max) NULL,
        [CLAVEACCESO]        [nvarchar](max) NULL,
        [FECHAEMISION]       [nvarchar](max) NULL,
        [HORAEMISION]        [nvarchar](max) NULL,
        [AMBIENTE]           [nvarchar](max) NULL,
        [ESTADO]             [nvarchar](max) NULL,
        [CODIGO]             [nvarchar](255) NULL,
        [TIPOEMISION]        [nvarchar](max) NULL,
        [NUMEROFACTURA]      [nvarchar](max) NULL,
        [CLAVEACCESOFACTURA] [nvarchar](max) NULL,
        [FECHAFACTURA]       [nvarchar](max) NULL,
        [MOTIVO]             [nvarchar](max) NULL,
        [CLIENTE]            [nvarchar](50)  NULL,
        [TOTALSINIMPUESTOS]  [nvarchar](max) NULL,
        [TOTALIVA]           [nvarchar](max) NULL,
        [TOTALCONIMPUESTOS]  [nvarchar](max) NULL,
        [CREDITOUSADO]       [nvarchar](255) NULL
    )
END
GO

IF OBJECT_ID('dbo.NOTASCREDITO_DETALLE', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[NOTASCREDITO_DETALLE](
        [NUMERONOTA]    [nvarchar](max) NULL,
        [PRODUCTO]      [nvarchar](max) NULL,
        [PRECIO]        [nvarchar](255) NULL,
        [CANTIDAD]      [nvarchar](255) NULL,
        [IVA]           [nvarchar](255) NULL,
        [NUMEROFACTURA] [nvarchar](max) NULL
    )
END
GO

IF OBJECT_ID('dbo.PARAMETROS_FACTURAS', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PARAMETROS_FACTURAS](
        [NOMBRE]                     [nvarchar](max) NOT NULL,
        [AMBIENTE]                   [nvarchar](10)  NULL,
        [TIPOEMISION]                [nvarchar](10)  NULL,
        [AGENTERETENCION]            [nvarchar](max) NULL,
        [CONTRIBUYENTERIMPE]         [nvarchar](max) NULL,
        [CODDOC]                     [nvarchar](10)  NULL,
        [ESTAB]                      [nvarchar](10)  NULL,
        [PUNTOEMISION]               [nvarchar](10)  NULL,
        [NUMERODIGITOS]              [int]           NULL,
        [PRUEBAPRODUCCION]           [nvarchar](10)  NULL,
        [CONTRIBUYENTEESPECIAL]      [nvarchar](max) NULL,
        [OBLIGADOCONTABILIDAD]       [nvarchar](max) NULL,
        [TIPOIDENTIFICADORCOMPRADOR] [nvarchar](max) NULL,
        [MONEDA]                     [nvarchar](10)  NULL,
        [CODIGOIMPUESTO]             [nvarchar](255) NULL,
        [CODIGOPORCENTAJE]           [nvarchar](255) NULL,
        [FECHAACTUALIZACION]         [nvarchar](30)  NULL,
        [SMTPSERVER]                 [nvarchar](max) NULL,
        [SMTPPORT]                   [nvarchar](255) NULL,
        [SMTPUSER]                   [nvarchar](max) NULL,
        [SMTPPASS]                   [nvarchar](255) NULL
    )
END
GO

IF OBJECT_ID('dbo.PARAMETROS_SRI', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PARAMETROS_SRI](
        [IDFACTURA]          [int]          NOT NULL,
        [TIPOCOMPROBANTE]    [nvarchar](2)  NOT NULL,
        [SECUENCIAL]         [int]          NOT NULL,
        [CODIGONUMERICO]     [nvarchar](9)  NOT NULL,
        [FECHAACTUALIZACION] [nvarchar](40) NULL
    )
END
GO

IF OBJECT_ID('dbo.PRODUCTOS', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PRODUCTOS](
        [NOMBRE]         [nvarchar](max)  NOT NULL,
        [VALOR]          [money]          NOT NULL,
        [ESTATUS]        [nvarchar](255)  NULL,
        [ORDEN]          [int]            NULL,
        [CODIGO]         [nvarchar](max)  NULL,
        [IVA]            [nvarchar](255)  NULL,
        [APLICA_ICE]     [bit]            NULL,
        [ICE_CODIGO]     [varchar](10)    NULL,
        [ICE_TIPO]       [varchar](20)    NULL,
        [ICE_PORCENTAJE] [decimal](10, 4) NULL,
        [ICE_VALOR]      [decimal](18, 6) NULL,
        [ICE_UNIDAD]     [varchar](50)    NULL,
        [AZUCAR_GRAMOS]  [decimal](10, 2) NULL,
        [VOLUMEN]        [decimal](10, 2) NULL,
        [PVP]            [decimal](18, 2) NULL,
        [TIPO_VEHICULO]  [varchar](20)    NULL
    )
END
GO

IF OBJECT_ID('dbo.PROVEEDORES', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PROVEEDORES](
        [IDPROVEEDOR]        [int]           IDENTITY(1,1) NOT NULL,
        [TIPOIDENTIFICACION] [nvarchar](255) NULL,
        [IDENTIFICACION]     [nvarchar](255) NULL,
        [RAZONSOCIAL]        [nvarchar](255) NULL,
        [CORREO]             [nvarchar](255) NULL,
        [DIRECCION]          [nvarchar](255) NULL,
        [TELEFONO]           [nvarchar](255) NULL,
        [TIPOPERSONA]        [nvarchar](255) NULL,
        [ESRIMPE]            [bit] NOT NULL CONSTRAINT [DF_PROVEEDORES_ESRIMPE]       DEFAULT ((0)),
        [TIPORIMPE]          [nvarchar](255) NULL,
        [ESPROFESIONAL]      [bit] NOT NULL CONSTRAINT [DF_PROVEEDORES_ESPROFESIONAL] DEFAULT ((0)),
        [ESARRENDADOR]       [bit] NOT NULL CONSTRAINT [DF_PROVEEDORES_ESARRENDADOR]  DEFAULT ((0)),
        [ESTADO]             [nvarchar](255) NULL,
        [FECHAREGISTRO]      [datetime]      NULL,
        CONSTRAINT [PK_PROVEEDORES] PRIMARY KEY CLUSTERED ([IDPROVEEDOR] ASC)
    )
END
GO

IF OBJECT_ID('dbo.RETENCIONES', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RETENCIONES](
        [NUMERORETENCION]          [nvarchar](50)  NULL,
        [CLAVEACCESO]              [nvarchar](255) NULL,
        [FECHAEMISION]             [nvarchar](20)  NULL,
        [HORAEMISION]              [nvarchar](20)  NULL,
        [AMBIENTE]                 [nvarchar](5)   NULL,
        [ESTADO]                   [nvarchar](50)  NULL,
        [CODIGO]                   [nvarchar](20)  NULL,
        [TIPOEMISION]              [nvarchar](5)   NULL,
        [NUMEROFACTURA]            [nvarchar](50)  NULL,
        [CLAVEACCESOFACTURA]       [nvarchar](255) NULL,
        [FECHAFACTURA]             [nvarchar](20)  NULL,
        [SUJETORETENIDO]           [nvarchar](255) NULL,
        [IDENTIFICACIONSUJETO]     [nvarchar](50)  NULL,
        [TIPOIDENTIFICACIONSUJETO] [nvarchar](5)   NULL,
        [DIRECCIONSUJETO]          [nvarchar](255) NULL,
        [REGIMENSUJETO]            [nvarchar](50)  NULL,
        [TOTALBASEIMPONIBLE]       [nvarchar](50)  NULL,
        [TOTALRETENCIONRENTA]      [nvarchar](50)  NULL,
        [TOTALRETENCIONIVA]        [nvarchar](50)  NULL,
        [TOTALRETENIDO]            [nvarchar](50)  NULL,
        [OBSERVACIONES]            [nvarchar](255) NULL
    )
END
GO

IF OBJECT_ID('dbo.RETENCIONES_DETALLE', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RETENCIONES_DETALLE](
        [NUMERORETENCION]     [nvarchar](50)  NULL,
        [TIPOIMPUESTO]        [nvarchar](5)   NULL,
        [CODIGOIMPUESTO]      [nvarchar](10)  NULL,
        [BASEIMPONIBLE]       [nvarchar](50)  NULL,
        [PORCENTAJERETENCION] [nvarchar](20)  NULL,
        [VALORRETENIDO]       [nvarchar](50)  NULL,
        [TIPOOPERACION]       [nvarchar](50)  NULL
    )
END
GO
";
        }
    }
}
