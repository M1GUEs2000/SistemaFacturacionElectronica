using Facturacion.api.Models.Respuestas;
using Facturacion.api.Models.Solicitudes;
using LogicaNegocios.Services;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;


namespace Facturacion.api.Servicios
{
    public class ServicioFactura : IServicioFactura
    {
        private readonly AppServices _services;

        public ServicioFactura(
            AppServices services
        )
        {
            _services = services;
        }
        public async Task<RespuestaFactura> CrearFacturaAsync(
            SolicitudFactura solicitud
        )
        {
            try
            {
                if (solicitud == null)
                    return Error("Solicitud inválida.");

                if (solicitud.Detalles == null ||
                    solicitud.Detalles.Count == 0)
                    return Error("No existen productos en la factura.");

                // ===============================
                // 1) CARGAR DATOS BASE
                // ===============================

                DataSet em = _services.Empresa.MostrarEmpresa(solicitud.Usuario);
                if (em == null || em.Tables.Count == 0 || em.Tables[0].Rows.Count == 0)
                    return Error("Empresa no encontrada.");

                DataSet pfm = _services.ParamFactura
                    .ConsultarNombre(solicitud.Usuario);
                if (pfm == null || pfm.Tables.Count == 0 || pfm.Tables[0].Rows.Count == 0)
                    return Error("Parámetros de facturación no encontrados.");

                DataSet pm = _services.Param.Mostrar();
                if (pm == null || pm.Tables.Count == 0 || pm.Tables[0].Rows.Count == 0)
                    return Error("Parámetros generales no encontrados.");

                var dsCliente = _services.Cliente
                    .ConsultarCedula(solicitud.CedulaCliente);

                if (dsCliente == null ||
                    dsCliente.Tables.Count == 0 ||
                    dsCliente.Tables[0].Rows.Count == 0)
                    return Error("Cliente no encontrado.");

                DataRow rowCliente = dsCliente.Tables[0].Rows[0];

                // ===============================
                // 2) CONVERTIR DETALLE A DATATABLE
                // ===============================

                DataTable detalle = ConvertirADetalleDataTable(
                    solicitud.Detalles
                );

                // ===============================
                // 3) EJECUTAR PROCESO COMPLETO
                // ===============================

                var resultado = await _services.ProcesosFacturacion
                    .ProcesarFacturaElectronicaCompleta(
                        em,
                        pfm,
                        pm,
                        rowCliente,
                        detalle,
                        solicitud.ClienteTexto,
                        solicitud.EsFacturaElectronica ? "SI" : "NO",
                        solicitud.FechaPago,
                        solicitud.TotalVisual,
                        solicitud.FormaPago,
                        solicitud.Comentario,
                        solicitud.Usuario,
                        solicitud.Ip
                    );

                if (resultado == null)
                    return Error("El backend no devolvió respuesta.");

                // ===============================
                // 4) MAPEAR RESPUESTA FINAL
                // ===============================

                return new RespuestaFactura
                {
                    Exito = resultado.Exito,
                    Autorizado = resultado.Autorizado,
                    SecuencialRepetido = resultado.SecuencialRepetido,
                    EsElectronica = resultado.EsElectronica,
                    NumeroDocumento = resultado.NumeroFactura,
                    ClaveAcceso = resultado.ClaveAcceso,
                    RutaPdf = resultado.RutaPdf,
                    RutaXmlAutorizado = resultado.RutaXmlAutorizado,
                    EnvioCorreoExitoso = resultado.EnvioCorreoExitoso,
                    Mensaje = resultado.Mensaje
                };
            }
            catch (Exception ex)
            {
                return Error("Error interno API: " + ex.Message);
            }
        }

        public async Task<RespuestaFactura> AutorizarFacturaPendienteAsync(
            SolicitudProcesarPendienteDocumento solicitud
        )
        {
            try
            {
                if (solicitud == null)
                    return Error("Solicitud inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.NumeroDocumento))
                    return Error("Número de factura inválido.");

                if (string.IsNullOrWhiteSpace(solicitud.Fecha))
                    return Error("Fecha inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.Usuario))
                    return Error("Usuario inválido.");

                if (string.IsNullOrWhiteSpace(solicitud.Ip))
                    return Error("IP inválida.");

                var resultado = await Task.Run(() =>
                    _services.ProcesosFacturacion.FacturaPendienteAutorizacionDesdeApi(
                        solicitud.NumeroDocumento,
                        solicitud.Fecha,
                        solicitud.Usuario,
                        solicitud.Ip
                    )
                );

                return new RespuestaFactura
                {
                    Exito = resultado.Exito,
                    Autorizado = resultado.Exito,
                    NumeroDocumento = resultado.NumeroFactura,
                    ClaveAcceso = resultado.ClaveAcceso,
                    RutaPdf = resultado.RutaPdf,
                    RutaXmlAutorizado = resultado.RutaXmlAutorizado,
                    EnvioCorreoExitoso = resultado.EnvioCorreoExitoso,
                    Mensaje = resultado.Mensaje
                };
            }
            catch (Exception ex)
            {
                return Error("Error autorizando factura pendiente: " + ex.Message);
            }
        }
        public async Task<RespuestaFactura> PreviewFacturaAsync(
    SolicitudFactura solicitud
)
        {
            try
            {
                if (solicitud == null)
                    return Error("Solicitud inválida.");

                if (solicitud.Detalles == null || solicitud.Detalles.Count == 0)
                    return Error("No existen productos en la factura.");

                // ===============================
                // Cargar datasets
                // ===============================
                DataSet em = _services.Empresa.MostrarEmpresa(solicitud.Usuario);
                DataSet pfm = _services.ParamFactura.ConsultarNombre(solicitud.Usuario);
                DataSet pm = _services.Param.Mostrar();

                var dsCliente = _services.Cliente.ConsultarCedula(solicitud.CedulaCliente);
                if (dsCliente == null || dsCliente.Tables[0].Rows.Count == 0)
                    return Error("Cliente no encontrado.");

                DataRow rowCliente = dsCliente.Tables[0].Rows[0];

                // ===============================
                // Convertir detalle
                // ===============================
                DataTable detalle = ConvertirADetalleDataTable(solicitud.Detalles);

                // ===============================
                // Ejecutar preview real
                // ===============================
                var resultado = await _services.ProcesosFacturacion.ProcesarFacturaPreview(
                    em,
                    pfm,
                    pm,
                    rowCliente,
                    detalle,
                    solicitud.ClienteTexto,
                    solicitud.EsFacturaElectronica ? "SI" : "NO",
                    solicitud.FechaPago,
                    solicitud.TotalVisual,
                    solicitud.FormaPago,
                    solicitud.Comentario
                );

                return new RespuestaFactura
                {
                    Exito = resultado.Exito,
                    RutaPdf = resultado.RutaPdf,
                    Mensaje = resultado.Mensaje,
                    EsElectronica = true
                };
            }
            catch (Exception ex)
            {
                return Error("Error generando preview: " + ex.Message);
            }
        }

        public async Task<RespuestaFactura> ProcesarFacturaConsumidorFinalAsync(
    SolicitudProcesarFacturaConsumidorFinal solicitud
)
        {
            try
            {
                if (solicitud == null)
                    return Error("Solicitud inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.NumeroFactura))
                    return Error("Número de factura inválido.");

                if (string.IsNullOrWhiteSpace(solicitud.Fecha))
                    return Error("Fecha inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.Hora))
                    return Error("Hora inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.CedulaCliente))
                    return Error("Cédula del cliente inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.Usuario))
                    return Error("Usuario inválido.");

                if (string.IsNullOrWhiteSpace(solicitud.Ip))
                    return Error("IP inválida.");

                var resultado = await _services.ProcesosFacturacion
                    .ProcesarFacturaConsumidorFinalAsync(
                        solicitud.Fecha,
                        solicitud.Hora,
                        solicitud.NumeroFactura,
                        solicitud.CedulaCliente,
                        solicitud.Usuario,
                        solicitud.Ip
                    );

                if (resultado == null)
                    return Error("El backend no devolvió respuesta.");

                return new RespuestaFactura
                {
                    Exito = resultado.Exito,
                    Autorizado = resultado.Autorizado,
                    SecuencialRepetido = resultado.SecuencialRepetido,
                    EsElectronica = resultado.EsElectronica,
                    NumeroDocumento = resultado.NumeroFactura,
                    ClaveAcceso = resultado.ClaveAcceso,
                    RutaPdf = resultado.RutaPdf,
                    RutaXmlAutorizado = resultado.RutaXmlAutorizado,
                    EnvioCorreoExitoso = resultado.EnvioCorreoExitoso,
                    Mensaje = resultado.Mensaje
                };
            }
            catch (Exception ex)
            {
                return Error("Error procesando factura consumidor final: " + ex.Message);
            }
        }

        public async Task<RespuestaFactura> ReprocesarFacturaPendienteAsync(
            SolicitudProcesarFacturaConsumidorFinal solicitud
        )
        {
            try
            {
                if (solicitud == null)
                    return Error("Solicitud inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.NumeroFactura))
                    return Error("Número de factura inválido.");

                if (string.IsNullOrWhiteSpace(solicitud.Fecha))
                    return Error("Fecha inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.Hora))
                    return Error("Hora inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.CedulaCliente))
                    return Error("Cédula del cliente inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.Usuario))
                    return Error("Usuario inválido.");

                if (string.IsNullOrWhiteSpace(solicitud.Ip))
                    return Error("IP inválida.");

                var resultado = await _services.ProcesosFacturacion
                    .ReprocesarFacturaPendienteAsync(
                        solicitud.Fecha,
                        solicitud.Hora,
                        solicitud.NumeroFactura,
                        solicitud.CedulaCliente,
                        solicitud.Usuario,
                        solicitud.Ip
                    );

                if (resultado == null)
                    return Error("El backend no devolvió respuesta.");

                return new RespuestaFactura
                {
                    Exito = resultado.Exito,
                    Autorizado = resultado.Autorizado,
                    SecuencialRepetido = resultado.SecuencialRepetido,
                    EsElectronica = resultado.EsElectronica,
                    NumeroDocumento = resultado.NumeroFactura,
                    ClaveAcceso = resultado.ClaveAcceso,
                    RutaPdf = resultado.RutaPdf,
                    RutaXmlAutorizado = resultado.RutaXmlAutorizado,
                    EnvioCorreoExitoso = resultado.EnvioCorreoExitoso,
                    Mensaje = resultado.Mensaje
                };
            }
            catch (Exception ex)
            {
                return Error("Error reprocesando factura pendiente: " + ex.Message);
            }
        }

        // ==========================================================
        // CONVERTIR LISTA A DATATABLE (alineado a tu backend)
        // ==========================================================

        private DataTable ConvertirADetalleDataTable(
            List<DetalleFacturaSolicitud> detalles
        )
        {
            var dt = new DataTable();

            dt.Columns.Add("PRODUCTO", typeof(string));
            dt.Columns.Add("CANTIDAD", typeof(int));      // ← FIX
            dt.Columns.Add("VALOR", typeof(decimal));
            dt.Columns.Add("TOTAL", typeof(decimal));
            dt.Columns.Add("CODIGO", typeof(string));

            foreach (var item in detalles)
            {
                dt.Rows.Add(
                    item.Producto,
                    item.Cantidad,                         // ← FIX
                    Math.Round(item.Valor, 2),
                    Math.Round(item.Total, 2),
                    item.Codigo
                );
            }

            return dt;
        }

        private RespuestaFactura Error(string mensaje)
        {
            return new RespuestaFactura
            {
                Exito = false,
                Autorizado = false,
                EsElectronica = false,
                Mensaje = mensaje
            };
        }
    }
}