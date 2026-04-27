using Facturacion.api.Models.Respuestas;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios.Interfaces;
using LogicaNegocios.Procesos;
using LogicaNegocios.Services;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;


namespace Facturacion.api.Servicios
{
    public class ServicioNota : IServicioNota
    {
        private readonly AppServices _services;

        public ServicioNota(
            AppServices services
        )
        {
            _services = services;
        }

        public async Task<RespuestaNota> CrearNotaAsync(
            SolicitudNota solicitud
        )
        {
            try
            {
                if (solicitud == null)
                    return Error("Solicitud inválida.");

                if (solicitud.Productos == null ||
                    solicitud.Productos.Count == 0)
                    return Error("No existen productos.");

                DataTable productos = ConvertirProductos(
                    solicitud.Productos
                );

                var resultado = await _services.ProcesosNotaCredito.ProcesarNotaCreditoElectronicaCompletaAsync(
                        solicitud.NumeroFactura,
                        solicitud.Motivo,
                        productos,
                        solicitud.Usuario,
                        solicitud.Ip
                    );

                return Mapear(resultado);
            }
            catch (Exception ex)
            {
                return Error("Error interno API: " + ex.Message);
            }
        }

        public async Task<RespuestaNota> PreviewNotaAsync(
            SolicitudNota solicitud
        )
        {
            try
            {
                DataTable productos = ConvertirProductos(
                    solicitud.Productos
                );

                var resultado = await _services.ProcesosNotaCredito.ProcesarNotaCreditoPreviewAsync(
                        solicitud.NumeroFactura,
                        solicitud.Motivo,
                        productos,
                        solicitud.Usuario
                    );

                return new RespuestaNota
                {
                    Exito = resultado.Exito,
                    RutaPdf = resultado.RutaPdf,
                    Mensaje = resultado.Mensaje
                };
            }
            catch (Exception ex)
            {
                return Error("Error preview NC: " + ex.Message);
            }
        }

        public async Task<RespuestaNota> ProcesarNotaDesdeAutorizacionAsync(
            SolicitudProcesarPendienteDocumento solicitud
        )
        {
            try
            {
                if (solicitud == null)
                    return Error("Solicitud inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.NumeroDocumento))
                    return Error("Número de nota inválido.");

                if (string.IsNullOrWhiteSpace(solicitud.Fecha))
                    return Error("Fecha inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.Usuario))
                    return Error("Usuario inválido.");

                if (string.IsNullOrWhiteSpace(solicitud.Ip))
                    return Error("IP inválida.");

                var resultado = await Task.Run(() =>
                    _services.ProcesosNotaCredito.NotaPendienteAutorizacionDesdeApi(
                        solicitud.NumeroDocumento,
                        solicitud.Fecha,
                        solicitud.Usuario,
                        solicitud.Ip,
                        solicitud.Proceso
                    )
                );

                return Mapear(resultado);
            }
            catch (Exception ex)
            {
                return Error("Error reprocesando NC: " + ex.Message);
            }
        }

        private DataTable ConvertirProductos(
            List<ProductoNotaCreditoSolicitud> lista
        )
        {
            var dt = new DataTable();

            dt.Columns.Add("CODIGO");
            dt.Columns.Add("PRODUCTO");
            dt.Columns.Add("CANTIDAD");
            dt.Columns.Add("VALOR");
            dt.Columns.Add("TOTAL");

            foreach (var item in lista)
            {
                dt.Rows.Add(
                    item.Codigo,
                    item.Producto,
                    item.Cantidad,
                    item.Valor,
                    item.Total
                );
            }

            return dt;
        }

        private RespuestaNota Mapear(
            ResultadoFinalNotaCredito r
        )
        {
            return new RespuestaNota
            {
                Exito = r.Exito,
                Mensaje = r.Mensaje,
                NumeroNota = r.NumeroNota,
                NumeroFactura = r.NumeroFactura,
                ClaveAcceso = r.ClaveAcceso,
                RutaXmlGenerado = r.RutaXmlGenerado,
                RutaXmlFirmado = r.RutaXmlFirmado,
                RutaXmlAutorizado = r.RutaXmlAutorizado,
                RutaPdf = r.RutaPdf,
                EnvioCorreoExitoso = r.EnvioCorreoExitoso,
                SecuencialRepetido = r.SecuencialRepetido,
                Autorizado = r.Autorizado
            };
        }

        private RespuestaNota Error(string mensaje)
        {
            return new RespuestaNota
            {
                Exito = false,
                Mensaje = mensaje
            };
        }
    }
}