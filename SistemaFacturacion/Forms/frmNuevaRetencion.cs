using LogicaNegocios.Services;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static LogicaNegocios.Procesos.ProcesosGenerales;
using static LogicaNegocios.Procesos.ProcesosRetenciones;


namespace SistemaFacturacion
{
    public partial class frmNuevaRetencion : Form
    {

        private readonly AppServices _services;
        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        // ==========================
        // DTO
        // ==========================
        private readonly DtoRetencionManual _datos;

        // ==========================
        // BASES
        // ==========================
        private decimal _baseRenta;
        private decimal _baseIva;
        private decimal _totalFactura;

        // ==========================
        // RENTA
        // ==========================
        private string _codigoRenta;
        private decimal _porcentajeRenta;
        private decimal _valorRetenidoRenta;

        // ==========================
        // IVA
        // ==========================
        private string _codigoIva;
        private decimal _porcentajeIva;
        private decimal _valorRetenidoIva;

        // ==========================
        // CATÁLOGO
        // ==========================
        private DataTable _dtCatalogo;
        private bool _cargando;

        // ==========================
        // CONSTRUCTOR
        // ==========================
        public frmNuevaRetencion(DtoRetencionManual datos, AppServices services)
        {
            InitializeComponent();
            _services = services;

            _datos = datos ?? throw new Exception("No se recibieron datos.");

            VincularEventos();
            CargarCatalogos();

            // 🔑 CLAVE: cargar combo de RENTA
            CargarCombo(cbTipoImpuestoRenta, "RENTA");

            PintarDatos();
            InicializarVista();

        }

        // ======================================================
        // EVENTOS
        // ======================================================
        private void VincularEventos()
        {
            cbTipoImpuestoRenta.SelectedIndexChanged += CbTipoImpuestoRenta_SelectedIndexChanged;
            cbTipoImpuestoIva.SelectedIndexChanged += CbTipoImpuestoIva_SelectedIndexChanged;


        }

        // ======================================================
        // INICIALIZAR VISTA
        // ======================================================
        private void InicializarVista()
        {
            _cargando = true;

            panelIva.Visible = false;
            cbTipoImpuestoRenta.SelectedIndex = -1;
            cbTipoImpuestoIva.SelectedIndex = -1;

            LimpiarRenta();
            LimpiarIva();

            _cargando = false;
        }

        // ======================================================
        // CATÁLOGOS
        // ======================================================
        private void CargarCatalogos()
        {
            // =========================
            // ESTRUCTURA ESTÁNDAR
            // =========================
            _dtCatalogo = new DataTable();
            _dtCatalogo.Columns.Add("TIPO", typeof(string));        // RENTA / IVA
            _dtCatalogo.Columns.Add("CODIGO", typeof(string));      // 303, 312, etc
            _dtCatalogo.Columns.Add("DESCRIPCION", typeof(string));
            _dtCatalogo.Columns.Add("PORCENTAJE", typeof(decimal));

            // =========================
            // RENTA – CATÁLOGO OFICIAL
            // =========================
            _dtCatalogo.Rows.Add("RENTA", "303", "Honorarios profesionales", 10m);
            _dtCatalogo.Rows.Add("RENTA", "303A", "Servicios profesionales sociedades", 3m);
            _dtCatalogo.Rows.Add("RENTA", "304", "Servicios intelectuales no relacionados", 10m);
            _dtCatalogo.Rows.Add("RENTA", "304A", "Comisiones servicios intelectuales", 10m);
            _dtCatalogo.Rows.Add("RENTA", "304B", "Notarios y registradores", 10m);
            _dtCatalogo.Rows.Add("RENTA", "304C", "Deportistas, árbitros, entrenadores", 8m);
            _dtCatalogo.Rows.Add("RENTA", "304D", "Artistas", 8m);
            _dtCatalogo.Rows.Add("RENTA", "304E", "Servicios de docencia", 10m);
            _dtCatalogo.Rows.Add("RENTA", "307", "Servicios mano de obra", 2m);
            _dtCatalogo.Rows.Add("RENTA", "308", "Uso de imagen o renombre", 10m);
            _dtCatalogo.Rows.Add("RENTA", "309", "Publicidad y medios", 2.75m);
            _dtCatalogo.Rows.Add("RENTA", "310", "Transporte pasajeros o carga", 1m);
            _dtCatalogo.Rows.Add("RENTA", "311", "Liquidación de compra", 2m);
            _dtCatalogo.Rows.Add("RENTA", "312", "Transferencia bienes muebles", 1.75m);
            _dtCatalogo.Rows.Add("RENTA", "312A", "Compras productor", 1m);
            _dtCatalogo.Rows.Add("RENTA", "312C", "Compras comercializador", 1.75m);
            _dtCatalogo.Rows.Add("RENTA", "314A", "Regalías franquicias PN", 10m);
            _dtCatalogo.Rows.Add("RENTA", "314B", "Derechos autor PN", 10m);
            _dtCatalogo.Rows.Add("RENTA", "314C", "Regalías franquicias sociedades", 10m);
            _dtCatalogo.Rows.Add("RENTA", "314D", "Derechos autor sociedades", 10m);
            _dtCatalogo.Rows.Add("RENTA", "319", "Arrendamiento mercantil", 2m);
            _dtCatalogo.Rows.Add("RENTA", "320", "Arrendamiento bienes inmuebles", 10m);
            _dtCatalogo.Rows.Add("RENTA", "322", "Seguros y reaseguros", 1m);
            _dtCatalogo.Rows.Add("RENTA", "323", "Rendimientos financieros", 2m);
            _dtCatalogo.Rows.Add("RENTA", "323E", "Depósitos plazo fijo gravados", 2m);
            _dtCatalogo.Rows.Add("RENTA", "323E2", "Depósitos plazo fijo exentos", 0m);
            _dtCatalogo.Rows.Add("RENTA", "325", "Anticipo dividendos", 25m);
            _dtCatalogo.Rows.Add("RENTA", "325A", "Préstamos accionistas", 25m);
            _dtCatalogo.Rows.Add("RENTA", "332", "No sujeto a retención", 0m);
            _dtCatalogo.Rows.Add("RENTA", "343", "Otras retenciones 1%", 1m);
            _dtCatalogo.Rows.Add("RENTA", "343A", "Energía eléctrica", 1m);
            _dtCatalogo.Rows.Add("RENTA", "343B", "Construcción", 1.75m);
            _dtCatalogo.Rows.Add("RENTA", "343C", "Botellas PET", 2m);
            _dtCatalogo.Rows.Add("RENTA", "3440", "Otras retenciones 2.75%", 2.75m);
            _dtCatalogo.Rows.Add("RENTA", "3480", "Pronósticos deportivos", 15m);
            _dtCatalogo.Rows.Add("RENTA", "3482", "Comisiones sociedades", 3m);

            // =========================
            // IVA – CATÁLOGO COMPLETO SRI
            // =========================
            _dtCatalogo.Rows.Add("IVA", "9", "Retención IVA 10%", 10m);
            _dtCatalogo.Rows.Add("IVA", "10", "Retención IVA 20%", 20m);
            _dtCatalogo.Rows.Add("IVA", "1", "Retención IVA 30%", 30m);
            _dtCatalogo.Rows.Add("IVA", "11", "Retención IVA 50%", 50m);
            _dtCatalogo.Rows.Add("IVA", "2", "Retención IVA 70%", 70m);
            _dtCatalogo.Rows.Add("IVA", "3", "Retención IVA 100%", 100m);
            _dtCatalogo.Rows.Add("IVA", "7", "Retención IVA 0%", 0m);   // Retención en cero
            _dtCatalogo.Rows.Add("IVA", "8", "No procede retención", 0m);
        }

        private void CargarCombo(ComboBox combo, string tipo)
        {
            _cargando = true;

            var rows = _dtCatalogo.AsEnumerable()
                .Where(r => r["TIPO"].ToString() == tipo);

            combo.DataSource = null;
            combo.DisplayMember = "DESCRIPCION";
            combo.ValueMember = "CODIGO";

            if (rows.Any())
                combo.DataSource = rows.CopyToDataTable();

            combo.SelectedIndex = -1;

            _cargando = false;
        }


        // ======================================================
        // PINTAR DATOS
        // ======================================================
        private void PintarDatos()
        {
            _baseRenta = _datos.BaseImponible;
            _baseIva = _datos.Iva;
            _totalFactura = _datos.Total;

            lblClienteRetenido.Text = _datos.RazonSocial;
            lblCedula.Text = _datos.Identificacion;
            lblDireccion.Text = _datos.Direccion;
            lblTipoPersona.Text = _datos.TipoPersona;
            lblRimpe.Text = _datos.EsRimpe == true ? "SI" : "NO";
            lblFechaFactura.Text = _datos.FechaFactura.ToString("dd/MM/yyyy");
            lblTipoRimpe.Text = _datos.TipoRimpe;
            lblArrendador.Text = _datos.EsArrendador == true ? "SI" : "NO";
            lblProfesional.Text = _datos.EsProfesional == true ? "SI" : "NO";

            lblBaseRenta.Text = NumericHelper.ToSri(_baseRenta);
            lblBaseIVA.Text = NumericHelper.ToSri(_baseIva);
            lblTotalFactura.Text = NumericHelper.ToSri(_totalFactura);

            lblNumeroFactura.Text = _datos.NumeroFactura;
            lblNumeroRetencion.Text =
                _services.Param.ObtenerSecuencialFormateado("07");
        }

        // ======================================================
        // RENTA
        // ======================================================
        private void CbTipoImpuestoRenta_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cargando || cbTipoImpuestoRenta.SelectedItem == null)
                return;

            DataRowView row = cbTipoImpuestoRenta.SelectedItem as DataRowView;
            if (row == null) return;

            _codigoRenta = row["CODIGO"].ToString();
            _porcentajeRenta = Convert.ToDecimal(row["PORCENTAJE"]);
            _valorRetenidoRenta =
                Math.Round(_baseRenta * _porcentajeRenta / 100m, 2);

            lblCodigoRenta.Text = _codigoRenta;
            lblPorcentajeRetencionRenta.Text =
                _porcentajeRenta.ToString("0.00", CultureInfo.InvariantCulture);
            lblValorRetenidoRenta.Text =
                NumericHelper.ToSri(_valorRetenidoRenta);
            lblTotalRenta.Text =
                NumericHelper.ToSri(_valorRetenidoRenta);

            // 👉 ACTIVAR IVA
            panelIva.Visible = true;
            CargarCombo(cbTipoImpuestoIva, "IVA");
            LimpiarIva();
        }

        // ======================================================
        // IVA
        // ======================================================
        private void CbTipoImpuestoIva_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cargando || cbTipoImpuestoIva.SelectedItem == null)
                return;

            DataRowView row = cbTipoImpuestoIva.SelectedItem as DataRowView;
            if (row == null) return;

            _codigoIva = row["CODIGO"].ToString();
            _porcentajeIva = Convert.ToDecimal(row["PORCENTAJE"]);
            _valorRetenidoIva =
                Math.Round(_baseIva * _porcentajeIva / 100m, 2);

            lblCodigoIva.Text = _codigoIva;
            lblPorcentajeRetencionIva.Text =
                _porcentajeIva.ToString("0.00", CultureInfo.InvariantCulture);
            lblValorRetenidoIva.Text =
                NumericHelper.ToSri(_valorRetenidoIva);
            lblTotalIva.Text =
                NumericHelper.ToSri(_valorRetenidoIva);
        }

        // ======================================================
        // LIMPIEZAS
        // ======================================================
        private void LimpiarRenta()
        {
            _codigoRenta = null;
            _porcentajeRenta = 0m;
            _valorRetenidoRenta = 0m;

            lblCodigoRenta.Text = "-";
            lblPorcentajeRetencionRenta.Text = "0.00";
            lblValorRetenidoRenta.Text = "0.00";
            lblTotalRenta.Text = "0.00";
        }

        private void LimpiarIva()
        {
            _codigoIva = null;
            _porcentajeIva = 0m;
            _valorRetenidoIva = 0m;

            lblCodigoIva.Text = "-";
            lblPorcentajeRetencionIva.Text = "0.00";
            lblValorRetenidoIva.Text = "0.00";
            lblTotalIva.Text = "0.00";
        }

        // ======================================================
        // BOTÓN GENERAR
        // ======================================================
        private async void btnGenerarRetencion_Click(object sender, EventArgs e)
        {
            try
            {
                // =============================================================
                // 0) VALIDAR AMBIENTE (PRUEBAS / PRODUCCIÓN)
                // =============================================================
                bool esProduccion = _services.ParamFactura
                    .EsProduccion(UsuarioActual);

                if (!esProduccion)
                {
                    bool continuar = Notificaciones.Show(
                        this,
                        "⚠ USTED ESTÁ EN AMBIENTE DE PRUEBAS.\n\n" +
                        "Los comprobantes NO tendrán validez tributaria.\n\n" +
                        "¿Desea continuar en PRUEBAS?",
                        "confirmacion"
                    );

                    if (!continuar)
                    {
                        bool cambiar = Notificaciones.Show(
                            this,
                            "¿Desea cambiar ahora a PRODUCCIÓN?",
                            "confirmacion"
                        );

                        if (cambiar)
                        {
                            _services.ParamFactura
                                .CambiarAProduccion(UsuarioActual, UsuarioActual, IPActual);

                            Notificaciones.Show(
                                this,
                                "Sistema cambiado a PRODUCCIÓN correctamente.\n" +
                                "Vuelva a presionar Procesar.",
                                "exito"
                            );
                        }

                        return; // Detener proceso
                    }
                }
                // =========================
                // VALIDACIONES PREVIAS
                // =========================
                if (string.IsNullOrWhiteSpace(_codigoRenta) ||
                    string.IsNullOrWhiteSpace(_codigoIva))
                {
                    Notificaciones.Show(
                        this,
                        "Debe seleccionar RENTA e IVA.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                DataTable conceptos = ConstruirConceptosRetencion();

                if (conceptos.Rows.Count == 0)
                    throw new Exception("No existen conceptos de retención válidos.");

                // =========================
                // PROCESO
                // =========================
                Notificaciones.Show(
                    this,
                    "Procesando Retención electrónica…",
                    "proceso",
                    UsuarioActual,
                    IPActual
                );

                ResultadoFinalRetencion resultado =
                    await _services.ProcesosRetenciones.ProcesarRetencionElectronicaCompleta(
                        _datos,
                        conceptos,
                        UsuarioActual,
                        IPActual
                    );

                Notificaciones.CerrarProceso(this);

                // ==========================================================
                // RESULTADO → UI THREAD
                // ==========================================================
                this.Invoke(new Action(() =>
                {
                    string numeroRetencion =
                        resultado?.NumeroRetencion ?? "(SIN NÚMERO)";

                    // -------------------------
                    // ERROR GENERAL
                    // -------------------------
                    if (!resultado.Exito)
                    {
                        Notificaciones.Show(
                            this,
                            "❌ RETENCIÓN ELECTRÓNICA\n\n" +
                            "Número: " + numeroRetencion + "\n\n" +
                            resultado.Mensaje,
                            "error",
                            UsuarioActual,
                            IPActual
                        );

                        return;
                    }

                    // ==================================================
                    // ELECTRÓNICA PENDIENTE DE AUTORIZACIÓN
                    // ==================================================
                    if (resultado.Exito && !resultado.Autorizado)
                    {
                        Notificaciones.Show(
                            this,
                            "⚠ RETENCIÓN ELECTRÓNICA Nº: " + numeroRetencion + "\n\n" +
                            "Documento enviado al SRI.\n" +
                            "Estado: PENDIENTE DE AUTORIZACIÓN.\n\n" +
                            "Puede consultarlo más tarde desde Consultas.",
                            "advertencia"
                        );

                        _services.Log.CrearLog(
                            "RETENCION PENDIENTE AUTORIZACION",
                            UsuarioActual,
                            IPActual,
                            "ClaveAcceso: " + resultado.ClaveAcceso
                        );

                        return;
                    }

                    // -------------------------
                    // AUTORIZADO
                    // -------------------------
                    if (resultado.Autorizado)
                    {
                        // =======================
                        // CORREO
                        // =======================
                        if (!resultado.EnvioCorreoExitoso)
                        {
                            Notificaciones.Show(
                                this,
                                "⚠ RETENCIÓN AUTORIZADA Nº: " + numeroRetencion + "\n\n" +
                                "La retención fue autorizada correctamente,\n" +
                                "pero NO se pudo enviar el correo.\n\n" +
                                "Puede reenviarlo desde Consultas.",
                                "advertencia"
                            );

                            _services.Log.CrearLog(
                                "RETENCION AUTORIZADA SIN CORREO",
                                UsuarioActual,
                                IPActual,
                                "ClaveAcceso: " + resultado.ClaveAcceso
                            );
                        }
                        else
                        {
                            Notificaciones.Show(
                                this,
                                "✅ RETENCIÓN ELECTRÓNICA AUTORIZADA\n" +
                                "Número: " + numeroRetencion + "\n" +
                                "Sujeto retenido: " + _datos.RazonSocial,
                                "exito"
                            );
                        }

                        return;
                    }
                }));
            }
            catch (Exception ex)
            {
                Notificaciones.CerrarProceso(this);

                Notificaciones.Show(
                    this,
                    "❌ ERROR GENERANDO RETENCIÓN\n\n" + ex.Message,
                    "error",
                    UsuarioActual,
                    IPActual
                );
            }
        }

        private DataTable ConstruirConceptosRetencion()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TIPOIMPUESTO");
            dt.Columns.Add("CODIGOIMPUESTO");
            dt.Columns.Add("BASEIMPONIBLE", typeof(decimal));
            dt.Columns.Add("PORCENTAJERETENCION", typeof(decimal));
            dt.Columns.Add("VALORRETENIDO", typeof(decimal));

            dt.Rows.Add("RENTA", _codigoRenta, _baseRenta, _porcentajeRenta, _valorRetenidoRenta);
            dt.Rows.Add("IVA", _codigoIva, _baseIva, _porcentajeIva, _valorRetenidoIva);

            return dt;
        }

        private async void btnPDF_Click(object sender, EventArgs e)
        {
            await VisualizarPDFRetencion();
        }

        private async Task VisualizarPDFRetencion()
        {
            try
            {
                // =========================
                // VALIDACIONES (IGUAL QUE GENERAR)
                // =========================
                if (string.IsNullOrWhiteSpace(_codigoRenta) ||
                    string.IsNullOrWhiteSpace(_codigoIva))
                {
                    Notificaciones.Show(
                        this,
                        "Debe seleccionar RENTA e IVA.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                DataTable conceptos = ConstruirConceptosRetencion();

                if (conceptos.Rows.Count == 0)
                {
                    Notificaciones.Show(
                        this,
                        "No existen conceptos de retención válidos.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                Notificaciones.Show(
                    this,
                    "Generando vista previa de Retención...",
                    "proceso",
                    UsuarioActual,
                    IPActual
                );

                // ==================================================
                // PREVIEW CORE
                // ==================================================
                var resultado = await _services.ProcesosRetenciones
                    .ProcesarRetencionPreviewAsync(
                        _datos,
                        conceptos,
                        UsuarioActual
                    );

                Notificaciones.CerrarProceso(this);

                if (!resultado.Exito)
                {
                    Notificaciones.Show(
                        this,
                        resultado.Mensaje,
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                Notificaciones.Show(
                    this,
                    "Vista previa generada correctamente.",
                    "exito",
                    UsuarioActual,
                    IPActual
                );

                System.Diagnostics.Process.Start(resultado.RutaPdf);
            }
            catch (Exception ex)
            {
                Notificaciones.CerrarProceso(this);

                Notificaciones.Show(
                    this,
                    "Error generando vista previa:\n" + ex.Message,
                    "error",
                    UsuarioActual,
                    IPActual
                );
            }
        }
    }
}
