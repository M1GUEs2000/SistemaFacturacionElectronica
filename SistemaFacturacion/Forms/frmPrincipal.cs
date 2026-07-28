using LogicaNegocios.Services;
using LogicaNegocios.Procesos;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SistemaFacturacion
{
    public partial class frmPrincipal : Form
    {
        private readonly AppServices _services;


        public frmPrincipal(
            string Nombre,
            string IP,
            AppServices services
        )
        {

            InitializeComponent();
            _services = services;

            lblNombreEmpresa.Text = Nombre.ToUpper();
            UsuarioActual = Nombre;
            IPActual = IP;

            txtCliente.MaxLength = 13;
            txtCliente.Text = "9999999999999";
            txtCliente.TextChanged += txtCliente_TextChanged_Buscar;
            txtCliente.KeyDown += txtCliente_KeyDown;

            lstClientes.Visible = false;
            lstClientes.Click += lstClientes_Click;

            this.FormClosing += frmPrincipal_FormClosing;
            button3.Hide();
        }

        DataTable TB_DETALLEFACTURA = new DataTable();
        DataTable TB_TRASFACTURADAS = new DataTable();
        int Contador = 0;
        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }
        private bool _suppressSearch = false;


        private void Timer1_Tick(object sender, EventArgs e)
        {
            //do whatever you want 
            lblHora.Text = DateTime.Now.ToString("hh:mm:ss");
            lblFecha.Text = DateTime.Now.ToString("yyyy/MM/dd");
        }
        private void frmPrincipal_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = (1 * 1000);//(10 * 1000); // 10 seconds in milliseconds
            timer.Tick += new EventHandler(Timer1_Tick);
            timer.Start();
            lblTotalVP.Visible = false;
            btnLimpiar.Visible = false;
            CargarDatosEmpresa();
            txtComentario.Text = "";
            lblHora.Text = DateTime.Now.ToString("hh:mm:ss");
            lblFecha.Text = DateTime.Now.ToString("yyyy/MM/dd");
            CargarBotonesFormasPago();

            btnProcesar.Visible = false;
            btnPDF.Visible = false;

            _services.CargarTarifaIva(lblNombreEmpresa.Text);

            ConsultaProducto();
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("CANTIDAD",       typeof(string)));
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("VALOR",          typeof(double)));
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("PRODUCTO",       typeof(string)));
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("TOTAL",          typeof(double)));
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("IVA",            typeof(string)));
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("CODIGO",         typeof(string)));
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("TOTAL_DISPLAY",  typeof(double)));
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("APLICA_ICE",     typeof(string)));
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("ICE_TIPO",       typeof(string)));
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("ICE_PORCENTAJE", typeof(string)));
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("ICE_VALOR",      typeof(string)));
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("ICE_UNIDAD",     typeof(string)));
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("AZUCAR_GRAMOS",  typeof(string)));
            TB_DETALLEFACTURA.Columns.Add(new DataColumn("VOLUMEN",        typeof(string)));

            // Mostrar TOTAL_DISPLAY (con ICE+IVA) en la columna TOTAL del grid
            this.TOTAL.DataPropertyName = "TOTAL_DISPLAY";
            gvDetalleFactura.AutoGenerateColumns = false;

            // TB_DETALLEFACTURA.Columns.Add(new DataColumn("ELIMINAR", typeof(DataGridViewImageColumn)));
            TB_TRASFACTURADAS.Columns.Add(new DataColumn("CNT", typeof(int)));
            TB_TRASFACTURADAS.Columns.Add(new DataColumn("VALOR", typeof(Double)));
            TB_TRASFACTURADAS.Columns.Add(new DataColumn("PRODUCTO", typeof(string)));
            TB_TRASFACTURADAS.Columns.Add(new DataColumn("TOTAL", typeof(Double)));
            TB_TRASFACTURADAS.Columns.Add(new DataColumn("FORMADEPAGO", typeof(string)));
            TB_TRASFACTURADAS.Columns.Add(new DataColumn("HORA", typeof(string)));
            TB_TRASFACTURADAS.Columns.Add(new DataColumn("NUMEROFACTURA", typeof(string)));

            this.reportViewer1.RefreshReport();
        }
        public void ConsultaProducto()
        {
            DataSet dsProductos = _services.Producto.MostrarProductos();
            if (dsProductos.Tables[0].Rows.Count > 0)
            {
                AgregarColumnaTarifaIva(dsProductos.Tables[0]);
                gvProductos.DataSource = dsProductos.Tables[0];
                OcultarColumnasIce();
                OrdenarColumnasProductos();
            }
        }

        public void ConsultaProductoOrden()
        {
            DataSet dsProductos = _services.Producto.MostrarProductosOrden();
            if (dsProductos.Tables[0].Rows.Count > 0)
            {
                AgregarColumnaTarifaIva(dsProductos.Tables[0]);
                gvProductos.DataSource = dsProductos.Tables[0];
                OcultarColumnasIce();
                OrdenarColumnasProductos();
            }
        }

        private void OcultarColumnasIce()
        {
            foreach (string col in new[] { "ICE_CODIGO", "ICE_TIPO", "ICE_PORCENTAJE", "ICE_VALOR", "ICE_UNIDAD", "AZUCAR_GRAMOS", "VOLUMEN", "TARIFA_IVA" })
                if (gvProductos.Columns[col] != null)
                    gvProductos.Columns[col].Visible = false;
        }

        private void OrdenarColumnasProductos()
        {
            foreach (DataGridViewColumn col in gvProductos.Columns)
            {
                if (col.DataPropertyName == "NOMBRE")
                    col.Width = 350;
                else if (col.DataPropertyName == "VALOR")
                    col.Width = 115;
            }

            if (gvProductos.Columns["VALOR_SIN_IMPUESTOS"] != null)
                gvProductos.Columns["VALOR_SIN_IMPUESTOS"].HeaderText = "VALOR";
            foreach (DataGridViewColumn col in gvProductos.Columns)
                if (col.DataPropertyName == "VALOR")
                    col.HeaderText = "VALOR CON IVA";

            // Orden deseado por DataPropertyName: NOMBRE, VALOR_SIN_IMPUESTOS, VALOR, IVA, APLICA_ICE, CODIGO
            string[] orden = { "NOMBRE", "VALOR_SIN_IMPUESTOS", "VALOR", "IVA", "APLICA_ICE", "CODIGO" };
            int idx = 0;
            foreach (string prop in orden)
            {
                foreach (DataGridViewColumn col in gvProductos.Columns)
                {
                    if (col.DataPropertyName == prop && col.Visible)
                    {
                        col.DisplayIndex = idx++;
                        break;
                    }
                }
            }
        }

        private void AgregarColumnaTarifaIva(DataTable dt)
        {
            if (dt.Columns.Contains("TARIFA_IVA")) return;
            dt.Columns.Add("TARIFA_IVA", typeof(decimal));
            if (!dt.Columns.Contains("VALOR_SIN_IMPUESTOS"))
                dt.Columns.Add("VALOR_SIN_IMPUESTOS", typeof(decimal));

            foreach (DataRow row in dt.Rows)
            {
                decimal valorSinImpuestos = Math.Round(Convert.ToDecimal(row["VALOR"]), 2);
                string iva = row["IVA"]?.ToString().Trim().ToUpper();
                decimal tarifa = iva == "SI" ? _services.TarifaIva : 0m;
                row["TARIFA_IVA"] = tarifa;
                row["VALOR_SIN_IMPUESTOS"] = valorSinImpuestos;
                if (tarifa > 0m)
                    row["VALOR"] = Math.Round(valorSinImpuestos * (1 + tarifa), 2);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CargarDatosEmpresa()
        {
            long secReal = _services.Param.ObtenerSecuencialReal("01"); // FACTURA
            long siguiente = secReal;
            lblNumeroFactura.Text = siguiente.ToString("D9");
            DataSet em = _services.Empresa.MostrarEmpresa(lblNombreEmpresa.Text);
            if (em != null && em.Tables.Count > 0 && em.Tables[0].Rows.Count > 0)
            {
                DataRow row = em.Tables[0].Rows[0];

                lblImpresion.Text = row["IMPRESION"].ToString();
                lblFactura.Text = row["FACTURACION"].ToString();
                lblNombreEmpresa.Text = row["NOMBRE"].ToString();
                AjustarTextoLabel(lblNombreEmpresa);
            }
            else
            {
                lblImpresion.Text = "Impresión: ---";
                lblFactura.Text = "Facturación: ---";
                lblNombreEmpresa.Text = "";
            }
        }

        private void AjustarTextoLabel(Label label)
        {
            if (label.Parent == null) return;
            label.AutoSize = false;
            label.Width = label.Parent.ClientSize.Width;
            label.TextAlign = ContentAlignment.MiddleCenter;
            float tamano = label.Font.Size;
            while (tamano > 4)
            {
                label.Font = new Font(label.Font.FontFamily, tamano, label.Font.Style);
                if (label.PreferredWidth <= label.Parent.ClientSize.Width)
                    break;
                tamano *= 0.75f;
            }
        }

        private string FormaPagoCodigo = "";

        private async void btnProcesar_Click(object sender, EventArgs e)
        {
            try
            {
                // =============================================================
                // 0) VALIDAR AMBIENTE (PRUEBAS / PRODUCCIÓN)
                //    SOLO SI NO ES CONSUMIDOR FINAL
                // =============================================================
                bool esConsumidorFinal = GetCedulaActual().ToUpper() == "FINAL";

                if (!esConsumidorFinal)
                {
                    bool esProduccion = _services.ParamFactura
                        .EsProduccion(lblNombreEmpresa.Text);

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
                                    .CambiarAProduccion(lblNombreEmpresa.Text, UsuarioActual, IPActual);

                                Notificaciones.Show(
                                    this,
                                    "Sistema cambiado a PRODUCCIÓN correctamente.\n" +
                                    "Vuelva a presionar Procesar.",
                                    "exito"
                                );
                            }

                            return;
                        }
                    }
                }

                // =============================================================
                // 1) VALIDACIONES UI
                // =============================================================
                var errores = Validar();
                if (errores.Count > 0)
                {
                    foreach (var err in errores)
                        Notificaciones.Show(this, err, "advertencia");
                    return;
                }

                // Empresa
                var em = _services.Empresa.MostrarEmpresa(lblNombreEmpresa.Text);
                if (em == null || em.Tables.Count == 0 || em.Tables[0].Rows.Count == 0)
                {
                    Notificaciones.Show(this, "Empresa no encontrada", "advertencia");
                    return;
                }

                // Cliente
                var cm = _services.Cliente.ConsultarCedula(GetCedulaActual());
                if (cm == null || cm.Tables.Count == 0 || cm.Tables[0].Rows.Count == 0)
                {
                    Notificaciones.Show(
                        this,
                        "Cliente no encontrado. Proceda a registrarlo.",
                        "advertencia"
                    );

                    Invoke(new Action(() =>
                    {
                        AbrirRegistroCliente(GetCedulaActual().ToUpper());
                    }));
                    return;
                }

                DataRow rowCliente = cm.Tables[0].Rows[0];

                // =============================================================
                // 2) RESPALDAR ANTES DE LIMPIAR
                // =============================================================
                DataTable copiaDetalle = TB_DETALLEFACTURA.Copy();

                string clienteOriginal = GetCedulaActual();
                string cedulaOriginal = GetCedulaActual();
                string formaPagoOriginal = txtFormaPago.Text.Trim();
                string totalOriginal = lblTotalVP.Text;
                string comentarioOriginal = txtComentario.Text.Trim();
                string fechaOriginal = lblFecha.Text;
                string horaOriginal = lblHora.Text;

                // =============================================================
                // 2.5) COBRO CON PINPAD (solo si la forma de pago es TARJETA)
                //      Se cobra SÍNCRONO aquí, ANTES de plasmar/encolar la factura.
                //      Si el datafast rechaza o no responde, se ABORTA: no se emite.
                // =============================================================
                long pinpadLogId = 0;
                bool cobradoConTarjeta = false;

                // Se conserva el resultado completo del cobro (no solo el Id de auditoría):
                // los datos de la tarjeta y los totales cobrados se necesitan después para
                // el recibo y para el baucher.
                ResultadoCobroTarjeta cobro = null;

                if (ProcesosTarjetas.EsPagoTarjeta(formaPagoOriginal))
                {
                    Notificaciones.Show(this, "Inserte / acerque la tarjeta en el datafast…", "proceso");

                    try
                    {
                        // Toda la orquestación del cobro + auditoría vive en ProcesosTarjetas;
                        // el form solo muestra el toast y decide si emite o aborta.
                        cobro = await Task.Run(() => _services.ProcesosTarjetas.CobrarFactura(
                            copiaDetalle,
                            clienteOriginal,          // ProcesosTarjetas resuelve el número real
                            formaPagoOriginal,        // (mismo secuencial que emitirá la factura)
                            TarjetaTipoPago,
                            TarjetaDiferidoNombre,
                            TarjetaCuotas,
                            TarjetaMesesGracia,
                            UsuarioActual));
                    }
                    finally
                    {
                        Notificaciones.CerrarProceso(this);
                    }

                    if (!cobro.Aprobado)
                    {
                        Notificaciones.Show(this, "Cobro con tarjeta NO aprobado:\n" + cobro.Motivo, "error");
                        return; // ABORTAR: la factura no se emite
                    }

                    pinpadLogId = cobro.PinpadLogId;
                    cobradoConTarjeta = true;

                    Notificaciones.Show(this,
                        "Transacción Aprobada\n" +
                        "Número autorización: " + (cobro.Detalle?.Autorizacion ?? "") + "\n" +
                        "Número tarjeta: " + (cobro.Detalle?.NumeroTarjeta ?? ""),
                        "exito");
                }

                // =============================================================
                // 3) LIMPIAR UI
                // =============================================================
                SetCedulaCliente("9999999999999");
                txtComentario.Text = "";
                txtFormaPago.Text = "";
                OcultarPanelTarjeta();
                TB_DETALLEFACTURA.Clear();
                lblTotalVP.Text = "TOTAL: 0.00";
                btnProcesar.Visible = false;
                btnLimpiar.Visible = false;

                // =============================================================
                // 4) PLASMAR INMEDIATAMENTE CON NÚMERO PROVISIONAL
                // =============================================================
                decimal tarifaIVAInmediata = ObtenerTarifaIVA();
                PlasmarFacturaDesdeDetalle(
                    copiaDetalle,
                    fechaOriginal,
                    horaOriginal,
                    formaPagoOriginal,
                    clienteOriginal,
                    cedulaOriginal,
                    "PENDIENTE",
                    tarifaIVAInmediata
                );

                Notificaciones.Show(
                    this,
                    "Factura enviada a cola de procesamiento para cliente: " + clienteOriginal,
                    "exito"
                );

                // =============================================================
                // 5) ENCOLAR PROCESO
                // =============================================================
                _services.FacturacionQueue.Enqueue(async () =>
                {
                    try
                    {
                        var pfm = _services.ParamFactura
                            .ConsultarNombre(lblNombreEmpresa.Text);

                        var pm = _services.Param
                            .ConsultarPorTipo("01");

                        var resultado = await _services.ProcesosFacturacion
                            .ProcesarFacturaElectronicaCompleta(
                                em,
                                pfm,
                                pm,
                                rowCliente,
                                copiaDetalle,
                                clienteOriginal,
                                lblFactura.Text,
                                fechaOriginal,
                                totalOriginal,
                                formaPagoOriginal,
                                comentarioOriginal,
                                UsuarioActual,
                                IPActual
                            );

                        this.Invoke(new Action(() =>
                        {
                            // ==================================================
                            // ACTUALIZAR EL NÚMERO DE FACTURA EN LA TABLA
                            // ==================================================
                            ActualizarNumeroFacturaEnTabla(horaOriginal, resultado.NumeroFactura);

                            // Correlacionar el cobro del pinpad con el número REAL de
                            // factura (el secuencial previsto pudo cambiar por reintento).
                            if (cobradoConTarjeta)
                            {
                                _services.ProcesosTarjetas.VincularFactura(pinpadLogId, resultado.NumeroFactura);
                            }

                            if (!resultado.Exito)
                            {
                                Notificaciones.Show(
                                    this,
                                    resultado.Mensaje,
                                    "error",
                                    UsuarioActual,
                                    IPActual
                                );

                                CargarDatosEmpresa();
                                return;
                            }

                            // ==================================================
                            // PREVIEW + IMPRESIÓN (igual condición que la tabla)
                            // ==================================================
                            DataTable enc = new DataTable();
                            enc.Columns.Add("NOMBRE");
                            enc.Columns.Add("FECHA");
                            enc.Columns.Add("HORA");
                            enc.Columns.Add("CLIENTE");
                            enc.Columns.Add("FORMAPAGO");
                            // Vacíos si la venta no fue con tarjeta: el recibo los omite.
                            enc.Columns.Add("APROBACION");
                            enc.Columns.Add("TARJETA");
                            enc.Rows.Add(
                                lblNombreEmpresa.Text,
                                fechaOriginal,
                                horaOriginal,
                                clienteOriginal,
                                formaPagoOriginal,
                                cobradoConTarjeta ? (cobro.Detalle?.Autorizacion ?? "") : "",
                                cobradoConTarjeta ? ProcesosTarjetas.NumeroTarjetaVisible(cobro.Detalle) : ""
                            );

                            reportViewer1.LocalReport.DataSources.Clear();
                            reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", resultado.TB_Venta));
                            reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet2", enc));
                            reportViewer1.RefreshReport();
                            reportViewer1.Visible = true;

                            if (lblImpresion.Text.Trim().ToUpper() == "SI")
                            {
                                LocalReport rdlc = new LocalReport();
                                rdlc.ReportPath = @"rptRecibo.rdlc";
                                rdlc.DataSources.Add(new ReportDataSource("DataSet1", resultado.TB_Venta));
                                rdlc.DataSources.Add(new ReportDataSource("DataSet2", enc));
                                var imp = new Impresora();
                                try { imp.Imprime(rdlc); } finally { imp.Dispose(); }

                                // ==============================================
                                // SEGUNDA TIRA: BAUCHER CON FIRMA
                                // Solo si se cobró con tarjeta Y el consumo alcanza
                                // MINIMOFIRMA. El recibo de arriba sale siempre.
                                // ==============================================
                                if (cobradoConTarjeta
                                    && cobro.Totales != null
                                    && _services.ProcesosTarjetas.RequiereFirmaBaucher(cobro.Totales.Total))
                                {
                                    try
                                    {
                                        BaucherTarjeta.Imprimir(
                                            _services,
                                            em.Tables[0].Rows[0],
                                            cobro.Detalle,
                                            cobro.Totales,
                                            BaucherTarjeta.RotuloOriginal
                                        );

                                        // La COPIA se pregunta DESPUÉS de que el ORIGINAL ya
                                        // salió: si el original falla, entra al catch y no
                                        // tiene sentido ofrecer una copia de algo que no
                                        // se imprimió. El toast "confirmacion" es bloqueante
                                        // (bombea mensajes hasta que el cajero responde), así
                                        // que aquí se detiene el flujo un momento; no hay
                                        // problema porque ya estamos de vuelta en el hilo de
                                        // UI y el cobro y la emisión están cerrados.
                                        bool quiereCopia = Notificaciones.Show(
                                            this,
                                            "¿Desea imprimir la copia del baucher?",
                                            "confirmacion"
                                        );

                                        if (quiereCopia)
                                        {
                                            BaucherTarjeta.Imprimir(
                                                _services,
                                                em.Tables[0].Rows[0],
                                                cobro.Detalle,
                                                cobro.Totales,
                                                BaucherTarjeta.RotuloCopia
                                            );
                                        }
                                    }
                                    catch (Exception exBaucher)
                                    {
                                        // La venta ya se cobró y se emitió: un fallo de
                                        // impresión no puede tumbar el flujo, pero el
                                        // cajero tiene que enterarse de que falta la firma.
                                        Notificaciones.Show(
                                            this,
                                            "No se pudo imprimir el baucher de la tarjeta:\n" +
                                            exBaucher.Message,
                                            "error",
                                            UsuarioActual,
                                            IPActual
                                        );
                                    }
                                }
                            }

                            // ==================================================
                            // LUEGO MENSAJE SEGÚN EL RESULTADO
                            // ==================================================
                            if (!resultado.EsElectronica)
                            {
                                Notificaciones.Show(
                                    this,
                                    "Factura de CONSUMIDOR FINAL registrada correctamente.\n" +
                                    "Número: " + resultado.NumeroFactura,
                                    "exito"
                                );

                                CargarDatosEmpresa();
                                return;
                            }

                            if (!resultado.Autorizado)
                            {
                                Notificaciones.Show(
                                    this,
                                    "⚠ FACTURACIÓN ELECTRÓNICA Nº: " + resultado.NumeroFactura + "\n\n" +
                                    "Documento enviado al SRI.\n" +
                                    "Estado: PENDIENTE DE AUTORIZACIÓN.\n\n" +
                                    "Puede consultarlo más tarde desde Consultas.",
                                    "advertencia"
                                );

                                _services.Log.CrearLog(
                                    "FACTURA PENDIENTE AUTORIZACION",
                                    UsuarioActual,
                                    IPActual,
                                    "ClaveAcceso: " + resultado.ClaveAcceso
                                );

                                CargarDatosEmpresa();
                                return;
                            }

                            // ==================================================
                            // AUTORIZADO
                            // ==================================================
                            if (!resultado.EnvioCorreoExitoso)
                            {
                                Notificaciones.Show(
                                    this,
                                    "⚠ FACTURA AUTORIZADA\n\n" +
                                    "La factura fue autorizada correctamente,\n" +
                                    "pero NO se pudo enviar el correo.\n\n" +
                                    "Puede reenviarlo desde Consultas.",
                                    "advertencia"
                                );

                                _services.Log.CrearLog(
                                    "FACTURA AUTORIZADA SIN CORREO",
                                    UsuarioActual,
                                    IPActual,
                                    "ClaveAcceso: " + resultado.ClaveAcceso
                                );
                            }
                            else
                            {
                                Notificaciones.Show(
                                    this,
                                    "Factura electrónica AUTORIZADA: " + resultado.NumeroFactura +
                                    "\nCliente: " + clienteOriginal,
                                    "exito"
                                );
                            }

                            CargarDatosEmpresa();
                        }));
                    }
                    catch (Exception exHilo)
                    {
                        this.Invoke(new Action(() =>
                        {
                            Notificaciones.Show(
                                this,
                                "ERROR GENERAL (QUEUE): " + exHilo.Message,
                                "error",
                                UsuarioActual,
                                IPActual
                            );
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                Notificaciones.Show(
                    this,
                    "ERROR GENERAL: " + ex.Message,
                    "error",
                    UsuarioActual,
                    IPActual
                );
            }
        }
        private void ActualizarNumeroFacturaEnTabla(string hora, string numeroFactura)
        {
            try
            {
                foreach (DataRow row in TB_TRASFACTURADAS.Rows)
                {
                    if (row["HORA"].ToString() == hora &&
                        row["NUMEROFACTURA"].ToString() == "PENDIENTE")
                    {
                        row["NUMEROFACTURA"] = numeroFactura;
                    }
                }

                DataView dv = TB_TRASFACTURADAS.DefaultView;
                dv.Sort = "HORA DESC, NUMEROFACTURA DESC";
                gvTransacccionesFacturadas.DataSource = dv.ToTable();
                gvTransacccionesFacturadas.Refresh();
            }
            catch { }
        }

        private void PlasmarFacturaDesdeDetalle(
            DataTable detalleOriginal,
            string fecha,
            string hora,
            string formaPago,
            string nombreCliente,
            string cedulaCliente,
            string numeroFactura,
            decimal tarifaIVA = 0m
        )
        {
            try
            {
                if (detalleOriginal == null || detalleOriginal.Rows.Count == 0)
                    return;

                if (TB_TRASFACTURADAS == null)
                    return;

                foreach (DataRow filaDetalle in detalleOriginal.Rows)
                {
                    DataRow nuevaFila = TB_TRASFACTURADAS.NewRow();

                    // Calcular TOTAL con ICE + IVA para esta fila
                    double totalConImpuestos = CalcularTotalConImpuestos(filaDetalle, tarifaIVA);

                    // COMPRAS se registran como débito (valor negativo)
                    if (formaPago.Trim().ToUpper() == "COMPRAS")
                        totalConImpuestos = totalConImpuestos * -1;

                    foreach (DataColumn colDestino in TB_TRASFACTURADAS.Columns)
                    {
                        string nombreColumna = colDestino.ColumnName.ToUpper().Trim();

                        // TOTAL se calcula con ICE+IVA, no se copia directo
                        if (nombreColumna == "TOTAL")
                        {
                            nuevaFila[colDestino.ColumnName] = totalConImpuestos;
                            continue;
                        }

                        if (detalleOriginal.Columns.Contains(colDestino.ColumnName))
                        {
                            nuevaFila[colDestino.ColumnName] = filaDetalle[colDestino.ColumnName];
                            continue;
                        }

                        switch (nombreColumna)
                        {
                            case "FECHA":
                                nuevaFila[colDestino.ColumnName] = fecha;
                                break;

                            case "HORA":
                                nuevaFila[colDestino.ColumnName] = hora;
                                break;

                            case "FORMADEPAGO":
                            case "FORMA DE PAGO":
                                nuevaFila[colDestino.ColumnName] = formaPago;
                                break;

                            case "CLIENTE":
                                nuevaFila[colDestino.ColumnName] = nombreCliente;
                                break;

                            case "CEDULA":
                                nuevaFila[colDestino.ColumnName] = cedulaCliente;
                                break;

                            case "NUMEROFACTURA":
                                nuevaFila[colDestino.ColumnName] = numeroFactura;
                                break;

                            default:
                                nuevaFila[colDestino.ColumnName] = DBNull.Value;
                                break;
                        }
                    }

                    TB_TRASFACTURADAS.Rows.Add(nuevaFila);
                }

                DataView dv = TB_TRASFACTURADAS.DefaultView;
                dv.Sort = "HORA DESC, NUMEROFACTURA DESC";
                gvTransacccionesFacturadas.DataSource = dv.ToTable();
                gvTransacccionesFacturadas.Refresh();
            }
            catch (Exception ex)
            {
                Notificaciones.Show(
                    this,
                    "No se pudo plasmar la factura en la tabla: " + ex.Message,
                    "advertencia"
                );
            }
        }

        private void gvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            btnProcesar.Visible = true;
            btnPDF.Visible = true;
            btnLimpiar.Visible = true;
            reportViewer1.Visible = false;

            var row = gvProductos.CurrentRow;
            string Cantidad = "1";
            string Producto = row.Cells[0].Value.ToString();


            // 1) LEER DATOS DEL PRODUCTO

            decimal precioConIva = Convert.ToDecimal(row.Cells[1].Value); // precio con IVA (display)
            string CodigoProd    = row.Cells[2].Value.ToString();
            string ivaProducto   = row.Cells[3].Value.ToString().Trim().ToUpper();

            // 2) Tarifa IVA pre-calculada al cargar la grilla
            decimal tarifaDecimal = row.Cells["TARIFA_IVA"].Value != null
                ? Convert.ToDecimal(row.Cells["TARIFA_IVA"].Value)
                : 0m;

            // 3) EXTRAER BASE REAL (sin IVA) y calcular IVA

            decimal baseReal;
            decimal ivaReal;

            if (ivaProducto == "NO" || tarifaDecimal == 0m)
            {
                baseReal = precioConIva;
                ivaReal  = 0m;
            }
            else
            {
                baseReal = Math.Round(precioConIva / (1 + tarifaDecimal), 4);
                ivaReal  = Math.Round(baseReal * tarifaDecimal, 2);
            }


            // 6) CALCULAR ICE + TOTAL CON IMPUESTOS

            decimal cantDec   = Convert.ToDecimal(Cantidad);
            decimal baseTotal = Math.Round(baseReal * cantDec, 2);

            // Reusar CalcularTotalConImpuestos vía fila temporal
            DataRow filaTemp = TB_DETALLEFACTURA.NewRow();
            filaTemp["CANTIDAD"]       = Cantidad;
            filaTemp["VALOR"]          = (double)baseReal;
            filaTemp["PRODUCTO"]       = Producto;
            filaTemp["TOTAL"]          = (double)baseTotal;
            filaTemp["IVA"]            = ivaProducto;
            filaTemp["CODIGO"]         = CodigoProd;
            filaTemp["APLICA_ICE"]     = row.Cells["APLICA_ICE"].Value?.ToString() ?? "";
            filaTemp["ICE_TIPO"]       = row.Cells["ICE_TIPO"].Value?.ToString() ?? "";
            filaTemp["ICE_PORCENTAJE"] = row.Cells["ICE_PORCENTAJE"].Value?.ToString() ?? "";
            filaTemp["ICE_VALOR"]      = row.Cells["ICE_VALOR"].Value?.ToString() ?? "";
            filaTemp["ICE_UNIDAD"]     = row.Cells["ICE_UNIDAD"].Value?.ToString() ?? "";
            filaTemp["AZUCAR_GRAMOS"]  = row.Cells["AZUCAR_GRAMOS"].Value?.ToString() ?? "";
            filaTemp["VOLUMEN"]        = row.Cells["VOLUMEN"].Value?.ToString() ?? "";
            double totalConImp = (double)Math.Round(precioConIva * cantDec, 2);

            string Valor = baseReal.ToString("0.00");
            string Total = totalConImp.ToString("0.00");


            // 7) AGREGAR A DETALLE Y ACTUALIZAR LA GRID

            TB_DETALLEFACTURA.Rows.Add(
                Cantidad, Valor, Producto, (double)baseTotal, ivaProducto, CodigoProd, totalConImp,
                filaTemp["APLICA_ICE"], filaTemp["ICE_TIPO"], filaTemp["ICE_PORCENTAJE"],
                filaTemp["ICE_VALOR"],  filaTemp["ICE_UNIDAD"], filaTemp["AZUCAR_GRAMOS"], filaTemp["VOLUMEN"]
            );
            gvDetalleFactura.DataSource = TB_DETALLEFACTURA;
            ActualizarBotonProcesar();


            // 8) CALCULAR TOTAL FINAL (suma de TOTAL_DISPLAY de cada fila)

            lblTotalVP.Visible = true;

            decimal sumaTotal = 0m;
            foreach (DataRow r in TB_DETALLEFACTURA.Rows)
                sumaTotal += Convert.ToDecimal(r["TOTAL_DISPLAY"]);

            lblTotalVP.Text = "TOTAL: " + sumaTotal.ToString("0.00");
        }
        private void gvDetalleFactura_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string colName = gvDetalleFactura.Columns[e.ColumnIndex].Name;
            if (colName == "ELIMINAR")
            {
                gvDetalleFactura.Rows.RemoveAt(e.RowIndex);

                decimal sumaTotal = 0m;
                foreach (DataGridViewRow r in gvDetalleFactura.Rows)
                    if (r.Cells["TOTAL"].Value != null)
                        sumaTotal += Convert.ToDecimal(r.Cells["TOTAL"].Value);

                lblTotalVP.Text = sumaTotal > 0m
                    ? "TOTAL: " + sumaTotal.ToString("0.00")
                    : "TOTAL: 0.00";

                ActualizarBotonProcesar();
            }
        }
        private void gvDetalleFactura_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string nombrecolumna = gvDetalleFactura.Columns[e.ColumnIndex].Name;
            if (nombrecolumna != "CANTIDAD" && nombrecolumna != "VALOR") return;

            string cantStr  = gvDetalleFactura.Rows[e.RowIndex].Cells["CANTIDAD"].Value?.ToString() ?? "0";
            string valorStr = gvDetalleFactura.Rows[e.RowIndex].Cells["VALOR"].Value?.ToString() ?? "0";

            if (!decimal.TryParse(cantStr, out decimal cant) ||
                !decimal.TryParse(valorStr, out decimal vlr)) return;

            decimal nuevaBase = Math.Round(cant * vlr, 2);

            // Actualizar base en DataTable (para XML)
            TB_DETALLEFACTURA.Rows[e.RowIndex]["TOTAL"]    = (double)nuevaBase;
            TB_DETALLEFACTURA.Rows[e.RowIndex]["CANTIDAD"] = cantStr;

            // Recalcular con ICE + IVA
            decimal tarifaIVA = ObtenerTarifaIVA();
            double totalConImp = CalcularTotalConImpuestos(TB_DETALLEFACTURA.Rows[e.RowIndex], tarifaIVA);
            TB_DETALLEFACTURA.Rows[e.RowIndex]["TOTAL_DISPLAY"] = totalConImp;

            // Sumar todos los totales con ICE+IVA
            double suma = 0;
            foreach (DataRow r in TB_DETALLEFACTURA.Rows)
                suma += Convert.ToDouble(r["TOTAL_DISPLAY"]);

            lblTotalVP.Text = "TOTAL: " + suma.ToString("0.00");
        }

        private decimal ObtenerTarifaIVA() => _services.TarifaIva;
        public List<string> Validar()
        {
            List<string> errores = new List<string>();

            if (gvDetalleFactura.Rows.Count == 0)
                errores.Add("Debe ingresar productos.");

            if (string.IsNullOrWhiteSpace(GetCedulaActual()))
                errores.Add("Debe ingresar cliente.");

            if (string.IsNullOrWhiteSpace(txtFormaPago.Text))
                errores.Add("Debe seleccionar forma de pago.");

            return errores;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            frmClaveTotales frmReporteT = new frmClaveTotales(_services);
            frmReporteT.UsuarioActual = this.UsuarioActual;   //
            frmReporteT.IPActual = this.IPActual;             //
            frmReporteT.Show();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            frmClaveConsulta frmReporteCF = new frmClaveConsulta(_services);
            frmReporteCF.UsuarioActual = this.UsuarioActual;  //
            frmReporteCF.IPActual = this.IPActual;            //
            frmReporteCF.Show();
        }
        private void bntTablas_Click(object sender, EventArgs e)
        {
            frmClaveTabla frmReporteT = new frmClaveTabla(_services);
            frmReporteT.UsuarioActual = this.UsuarioActual;   //
            frmReporteT.IPActual = this.IPActual;             //
            frmReporteT.Show();
        }
        private void btnLimpiarProducto_Click(object sender, EventArgs e)
        {
            txtBuscarProductos.Clear();
            txtBuscarProductos.Focus();
        }

        private void txtBuscarProductos_TextChanged(object sender, EventArgs e)
        {
            string termino = txtBuscarProductos.Text.Trim();

            if (string.IsNullOrEmpty(termino))
            {
                ConsultaProducto();
                return;
            }

            DataSet ds = _services.Producto.BuscarPorNombre(termino);
            if (ds.Tables[0].Rows.Count > 0)
            {
                AgregarColumnaTarifaIva(ds.Tables[0]);
                gvProductos.DataSource = ds.Tables[0];
                OcultarColumnasIce();
                OrdenarColumnasProductos();
            }
            else
            {
                gvProductos.DataSource = null;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                _services.CargarTarifaIva(lblNombreEmpresa.Text);
                ConsultaProducto();
                CargarBotonesFormasPago();
                Notificaciones.Show(this, "Formas de Pago y Productos refrescados correctamente.", "exito");
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "No se pudo refrescar: " + ex.Message, "advertencia");
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            TB_DETALLEFACTURA.Clear();
            Contador = 0;
            lblTotalVP.Text = "TOTAL: 0.00";
            txtFormaPago.Text = "";
            OcultarPanelTarjeta();
            SetCedulaCliente("");
            btnProcesar.Visible = false;
            btnLimpiar.Visible = false;
            ActualizarBotonProcesar();
        }
        private void btnClientes_Click(object sender, EventArgs e)
        {
            AbrirRegistroCliente(GetCedulaActual().ToUpper());
            ActualizarBotonProcesar();
        }

        private string GetCedulaActual() => txtCliente.Text.Trim();

        private void SetCedulaCliente(string valor)
        {
            _suppressSearch = true;
            txtCliente.Text = valor;
            lstClientes.Visible = false;
            _suppressSearch = false;
        }

        private void txtCliente_TextChanged_Buscar(object sender, EventArgs e)
        {
            if (_suppressSearch) return;

            string termino = txtCliente.Text.Trim();

            if (termino.Length < 2)
            {
                lstClientes.Items.Clear();
                lstClientes.Visible = false;
                return;
            }

            var ds = _services.Cliente.BuscarLike(termino);
            lstClientes.Items.Clear();

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                    lstClientes.Items.Add(row["CEDULA"] + " — " + row["NOMBRE"]);

                lstClientes.Visible = true;
            }
            else
            {
                lstClientes.Visible = false;
            }
        }

        private void lstClientes_Click(object sender, EventArgs e)
        {
            if (lstClientes.SelectedItem == null) return;

            string item = lstClientes.SelectedItem.ToString();
            string cedula = item.Split(new[] { " — " }, StringSplitOptions.None)[0].Trim();

            SetCedulaCliente(cedula);
            ActualizarBotonProcesar();
            txtCliente.Focus();
        }

        private void txtCliente_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && lstClientes.Visible && lstClientes.Items.Count > 0)
            {
                lstClientes.Focus();
                lstClientes.SelectedIndex = 0;
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                lstClientes.Visible = false;
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                lstClientes.Visible = false;
                btnClientes_Click(sender, EventArgs.Empty);
            }
        }
        private void ActualizarBotonProcesar()
        {
            bool esFinal = GetCedulaActual().ToUpper() == "FINAL";

            // Debe estar validado el cliente

            // Debe haber productos en la factura
            bool hayProductos = TB_DETALLEFACTURA.Rows.Count > 0;

            // El botón solo aparece si AMBOS están OK
            btnProcesar.Visible = (hayProductos);
            btnPDF.Visible = (hayProductos);
        }
        private void btnConsumidor_Click(object sender, EventArgs e)
        {
            SetCedulaCliente("FINAL");
            ActualizarBotonProcesar();
        }
        private void frmPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing &&
                _services.FacturacionQueue.HayTrabajoPendiente)
            {
                e.Cancel = true;
                MessageBox.Show(
                    this,
                    "No se puede cerrar mientras hay facturas en cola procesándose.\nEspere a que finalicen.",
                    "Cola activa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            try
            {
                _services.Log.CrearLog(
                    "Salida del sistema",
                    this.UsuarioActual,
                    this.IPActual,
                    "El usuario salió del sistema"
                );
            }
            catch { }

            Application.Exit();
        }
        private void CargarBotonesFormasPago()
        {
            DataSet ds = _services.FormaPago.MostrarFormaPago("");
            DataTable dt = ds.Tables[0];

            panelFormasPago.Controls.Clear();
            panelFormasPago.AutoScroll = true;

            int x = 10;
            int y = 10;

            foreach (DataRow row in dt.Rows)
            {
                string nombre = row["FORMAS"].ToString();
                string img = row["IMAGEN"].ToString();
                string codigo = row["CODIGO"].ToString();  // ← Nuevo

                string ruta = Path.Combine(Application.StartupPath, "ImagenesFormas", img);

                int ancho = 100;
                int alto = 65;

                // Crear botón
                var btn = new System.Windows.Forms.Button();
                btn.Width = ancho;
                btn.Height = alto;
                btn.Text = "";
                btn.Tag = codigo + "|" + nombre;

                if (File.Exists(ruta))
                {
                    System.Drawing.Image original = System.Drawing.Image.FromFile(ruta);
                    System.Drawing.Image resized = new Bitmap(original, new Size(ancho, alto - 10));
                    btn.Image = resized;
                    btn.ImageAlign = ContentAlignment.MiddleCenter;
                }

                btn.Click += btnFormasPago_Click;

                // Crear label debajo del botón
                var lbl = new System.Windows.Forms.Label();
                lbl.Text = nombre;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.Font = new Font("Segoe UI", 8, FontStyle.Bold);
                lbl.Width = ancho;
                lbl.Height = 20;
                lbl.Location = new Point(x, y + alto);
                lbl.ForeColor = Color.Black;

                // Ubicación del botón
                btn.Location = new Point(x, y);

                // Agregar controles al panel
                panelFormasPago.Controls.Add(btn);
                panelFormasPago.Controls.Add(lbl);

                // Mover posición horizontal
                x += ancho + 10;
            }
        }
        private void btnFormasPago_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button btn = (System.Windows.Forms.Button)sender;

            // "01|EFECTIVO"
            string[] datos = btn.Tag.ToString().Split('|');
            string codigo = datos[0];
            string nombre = datos[1];

            // ✔ Guardar el código REAL para toda la clase
            FormaPagoCodigo = codigo;

            // ✔ Mostrar el nombre al usuario
            txtFormaPago.Text = nombre;

            // TARJETA despliega el panel de tipo de pago / diferido / cuotas
            if (nombre.Trim().ToUpper() == "TARJETA")
                MostrarPanelTarjeta();
            else
                OcultarPanelTarjeta();

            // COMPRAS usa catálogo especial (ORDEN >= 9001)
            if (nombre.Trim().ToUpper() == "COMPRAS")
                ConsultaProductoOrden();
            else
                ConsultaProducto();
        }
        // =====================================================================
        //  PANEL DE TARJETA — Tipo de Pago / Tipo de Diferido / Nº de Cuotas
        //  Se alimenta de PARAMETROS_TRANSACCIONES (ver ParametrosTransaccionesManejador).
        //  Solo se muestra cuando la forma de pago seleccionada es TARJETA; ensancha
        //  el form a la derecha y encadena los tres combos según lo que esté ACTIVO.
        // =====================================================================

        private GroupBox _gbTarjeta;
        private ComboBox _cmbTarjetaTipoPago;
        private ComboBox _cmbTarjetaDiferido;
        private ComboBox _cmbTarjetaCuotas;
        private ComboBox _cmbTarjetaMesesGracia;
        private Label _lblTarjetaDiferido;
        private Label _lblTarjetaCuotas;
        private Label _lblTarjetaMesesGracia;
        private int _anchoClientOriginal;
        private bool _tarjetaExpandida;

        // Selección resultante — disponible para la emisión de la factura
        public string TarjetaTipoPago { get; private set; }        // "C" = corriente / "D" = diferido
        public string TarjetaDiferidoCodigo { get; private set; }  // T1..T7
        public string TarjetaDiferidoNombre { get; private set; }
        public int TarjetaCuotas { get; private set; }
        public int TarjetaMesesGracia { get; private set; }

        private void ConstruirPanelTarjeta()
        {
            if (_gbTarjeta != null) return;

            _anchoClientOriginal = this.ClientSize.Width;

            _gbTarjeta = new GroupBox
            {
                Text = "PAGO CON TARJETA",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Size = new Size(264, 254),
                Visible = false
            };

            _cmbTarjetaTipoPago = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 48),
                Size = new Size(240, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            _cmbTarjetaTipoPago.SelectedIndexChanged += CmbTarjetaTipoPago_Changed;

            _lblTarjetaDiferido = new Label
            {
                Text = "Tipo de Diferido:",
                Location = new Point(12, 82),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
                Visible = false
            };
            _cmbTarjetaDiferido = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 102),
                Size = new Size(240, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Visible = false
            };
            _cmbTarjetaDiferido.SelectedIndexChanged += CmbTarjetaDiferido_Changed;

            _lblTarjetaCuotas = new Label
            {
                Text = "Nº de Cuotas:",
                Location = new Point(12, 136),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
                Visible = false
            };
            _cmbTarjetaCuotas = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 156),
                Size = new Size(240, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Visible = false
            };
            _cmbTarjetaCuotas.SelectedIndexChanged += (s, e) => GuardarSeleccionTarjeta();

            _lblTarjetaMesesGracia = new Label
            {
                Text = "Meses de Gracia:",
                Location = new Point(12, 190),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
                Visible = false
            };
            _cmbTarjetaMesesGracia = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 210),
                Size = new Size(240, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Visible = false
            };
            for (int i = 1; i <= 24; i++)
                _cmbTarjetaMesesGracia.Items.Add(i.ToString("00"));
            _cmbTarjetaMesesGracia.SelectedIndexChanged += (s, e) => GuardarSeleccionTarjeta();

            var lblTipoPago = new Label
            {
                Text = "Tipo de Pago:",
                Location = new Point(12, 28),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular)
            };

            _gbTarjeta.Controls.Add(lblTipoPago);
            _gbTarjeta.Controls.Add(_cmbTarjetaTipoPago);
            _gbTarjeta.Controls.Add(_lblTarjetaDiferido);
            _gbTarjeta.Controls.Add(_cmbTarjetaDiferido);
            _gbTarjeta.Controls.Add(_lblTarjetaCuotas);
            _gbTarjeta.Controls.Add(_cmbTarjetaCuotas);
            _gbTarjeta.Controls.Add(_lblTarjetaMesesGracia);
            _gbTarjeta.Controls.Add(_cmbTarjetaMesesGracia);

            // Ubicado a la derecha del carrito, en el área que se agrega al ensanchar
            _gbTarjeta.Location = new Point(pnlVariosProductos.Right + 12, panelFormasPago.Top);

            this.Controls.Add(_gbTarjeta);
            _gbTarjeta.BringToFront();
        }

        private void MostrarPanelTarjeta()
        {
            ConstruirPanelTarjeta();

            // Ensanchar el form para que quepa el panel (una sola vez)
            if (!_tarjetaExpandida)
            {
                int anchoNecesario = _gbTarjeta.Right + 12;
                if (this.ClientSize.Width < anchoNecesario)
                    this.ClientSize = new Size(anchoNecesario, this.ClientSize.Height);
                _tarjetaExpandida = true;
            }

            _gbTarjeta.Visible = true;
            _gbTarjeta.BringToFront();

            CargarTarjeta_TipoPago();
        }

        private void OcultarPanelTarjeta()
        {
            if (_gbTarjeta != null)
                _gbTarjeta.Visible = false;

            // Devolver el form a su ancho original
            if (_tarjetaExpandida)
            {
                this.ClientSize = new Size(_anchoClientOriginal, this.ClientSize.Height);
                _tarjetaExpandida = false;
            }

            TarjetaTipoPago = "";
            TarjetaDiferidoCodigo = "";
            TarjetaDiferidoNombre = "";
            TarjetaCuotas = 0;
            TarjetaMesesGracia = 0;
        }

        // TIPO DE PAGO: CORRIENTE es SIEMPRE el default. DIFERIDO solo se ofrece
        // cuando en los parámetros está activo AMBOS (CODIGO 'A', ACTIVO=1).
        // Si no está AMBOS, el combo queda bloqueado en CORRIENTE.
        private void CargarTarjeta_TipoPago()
        {
            _cmbTarjetaTipoPago.Items.Clear();

            DataSet ds = _services.ParamTransaccion.ObtenerTiposPago();

            string valorCorriente = "CORRIENTE";
            string valorDiferido = "DIFERIDO";
            bool ambosActivo = false;

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string cod = row["CODIGO"].ToString().Trim().ToUpper();
                    string val = row["VALOR"].ToString().Trim();

                    if (cod == "C") valorCorriente = val;
                    else if (cod == "D") valorDiferido = val;

                    if (cod == "A" && Convert.ToBoolean(row["ACTIVO"]))
                        ambosActivo = true;
                }
            }

            // CORRIENTE siempre presente y seleccionado por default
            _cmbTarjetaTipoPago.Items.Add(new ItemCombo(valorCorriente, "C"));

            // DIFERIDO únicamente si AMBOS está activo
            if (ambosActivo)
                _cmbTarjetaTipoPago.Items.Add(new ItemCombo(valorDiferido, "D"));

            // Solo se puede cambiar si existe la opción DIFERIDO
            _cmbTarjetaTipoPago.Enabled = ambosActivo;

            _cmbTarjetaTipoPago.SelectedIndex = 0; // CORRIENTE => combos de diferido ocultos
        }

        private void CmbTarjetaTipoPago_Changed(object sender, EventArgs e)
        {
            bool esDiferido = (_cmbTarjetaTipoPago.SelectedItem as ItemCombo)?.Value == "D";

            // Tipo de diferido: visible solo cuando el pago es DIFERIDO
            _lblTarjetaDiferido.Visible = esDiferido;
            _cmbTarjetaDiferido.Visible = esDiferido;

            // Cuotas: ocultas hasta que se escoja un tipo de diferido
            _lblTarjetaCuotas.Visible = false;
            _cmbTarjetaCuotas.Visible = false;
            _cmbTarjetaCuotas.Items.Clear();
            _lblTarjetaMesesGracia.Visible = false;
            _cmbTarjetaMesesGracia.Visible = false;
            _cmbTarjetaMesesGracia.SelectedIndex = -1;

            if (esDiferido)
                CargarTarjeta_Diferidos();
            else
                _cmbTarjetaDiferido.Items.Clear();

            GuardarSeleccionTarjeta();
        }

        // TIPO DE DIFERIDO: solo los activos. Se traen todos y se filtra en C# con
        // Convert.ToBoolean, porque en Access el campo Sí/No guarda True como -1 y la
        // consulta 'ACTIVO = 1' de ObtenerTiposDiferidoActivos no matchea nada.
        // No se auto-selecciona: las cuotas aparecen solo cuando el usuario escoge uno.
        private void CargarTarjeta_Diferidos()
        {
            _cmbTarjetaDiferido.Items.Clear();

            DataSet ds = _services.ParamTransaccion.ObtenerTiposDiferido();
            if (ds != null && ds.Tables.Count > 0)
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    if (!Convert.ToBoolean(row["ACTIVO"])) continue;

                    _cmbTarjetaDiferido.Items.Add(new ItemCombo(
                        row["VALOR"].ToString().Trim(),
                        row["CODIGO"].ToString().Trim()));
                }

            // Default: SIN INTERESES (si está activo); si no, el primero disponible
            int idxDefault = -1;
            for (int i = 0; i < _cmbTarjetaDiferido.Items.Count; i++)
                if (((ItemCombo)_cmbTarjetaDiferido.Items[i]).Text
                        .Equals("SIN INTERESES", StringComparison.OrdinalIgnoreCase))
                {
                    idxDefault = i;
                    break;
                }

            if (idxDefault < 0 && _cmbTarjetaDiferido.Items.Count > 0)
                idxDefault = 0;

            _cmbTarjetaDiferido.SelectedIndex = idxDefault; // dispara la carga de cuotas
        }

        private void CmbTarjetaDiferido_Changed(object sender, EventArgs e)
        {
            bool hayDiferido = _cmbTarjetaDiferido.SelectedItem is ItemCombo;

            // Cuotas: aparecen solo cuando ya hay un tipo de diferido escogido
            _lblTarjetaCuotas.Visible = hayDiferido;
            _cmbTarjetaCuotas.Visible = hayDiferido;

            if (hayDiferido)
                CargarTarjeta_Cuotas();
            else
                _cmbTarjetaCuotas.Items.Clear();

            bool aplicaMesesGracia = hayDiferido && EsDiferidoConMesesGracia(_cmbTarjetaDiferido.SelectedItem as ItemCombo);
            _lblTarjetaMesesGracia.Visible = aplicaMesesGracia;
            _cmbTarjetaMesesGracia.Visible = aplicaMesesGracia;
            if (aplicaMesesGracia && _cmbTarjetaMesesGracia.SelectedIndex < 0)
                _cmbTarjetaMesesGracia.SelectedIndex = 0; // 01 por defecto
            else if (!aplicaMesesGracia)
                _cmbTarjetaMesesGracia.SelectedIndex = -1;

            GuardarSeleccionTarjeta();
        }

        // Nº DE CUOTAS: solo múltiplos de tres (03..24), limitados por la cuota
        // configurada para el tipo de diferido.
        private void CargarTarjeta_Cuotas()
        {
            _cmbTarjetaCuotas.Items.Clear();

            if (!(_cmbTarjetaDiferido.SelectedItem is ItemCombo dif))
                return;

            DataSet ds = _services.ParamTransaccion.ObtenerCuotasPorDiferido(dif.Value);

            int maxCuotas = 0;
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                int.TryParse(ds.Tables[0].Rows[0]["VALOR"].ToString().Trim(), out maxCuotas);

            int limiteCuotas = Math.Min(maxCuotas, 24);
            for (int i = 3; i <= limiteCuotas; i += 3)
                _cmbTarjetaCuotas.Items.Add(i.ToString("00"));

            if (_cmbTarjetaCuotas.Items.Count > 0)
                _cmbTarjetaCuotas.SelectedIndex = 0;
        }

        private void GuardarSeleccionTarjeta()
        {
            TarjetaTipoPago = (_cmbTarjetaTipoPago.SelectedItem as ItemCombo)?.Value ?? "";

            if (TarjetaTipoPago == "D")
            {
                var dif = _cmbTarjetaDiferido.SelectedItem as ItemCombo;
                TarjetaDiferidoCodigo = dif?.Value ?? "";
                TarjetaDiferidoNombre = dif?.Text ?? "";
                TarjetaCuotas = int.TryParse(_cmbTarjetaCuotas.SelectedItem?.ToString(), out int cuotas)
                    ? cuotas
                    : 0;
                TarjetaMesesGracia = EsDiferidoConMesesGracia(dif)
                    && int.TryParse(_cmbTarjetaMesesGracia.SelectedItem?.ToString(), out int mesesGracia)
                    ? mesesGracia
                    : 0;
            }
            else
            {
                TarjetaDiferidoCodigo = "";
                TarjetaDiferidoNombre = "";
                TarjetaCuotas = 0;
                TarjetaMesesGracia = 0;
            }
        }

        private static bool EsDiferidoConMesesGracia(ItemCombo diferido)
        {
            if (diferido == null) return false;

            return diferido.Value == "07"
                || diferido.Value == "09"
                || diferido.Text.IndexOf("GRACIA", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private sealed class ItemCombo
        {
            public string Text { get; }
            public string Value { get; }

            public ItemCombo(string text, string value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString() => Text;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AAAPrueba ventana = new AAAPrueba();
            ventana.UsuarioActual = this.UsuarioActual;
            ventana.IPActual = this.IPActual;
            ventana.Show();

        }
        private void AbrirRegistroCliente(string cedula)
        {
            frmClientesDatos ventana = new frmClientesDatos(_services, true);
            ventana.UsuarioActual = this.UsuarioActual;
            ventana.IPActual = this.IPActual;

            // Si la cédula es FINAL o consumidor final genérico → abrir vacío
            if (cedula.Trim().ToUpper() == "FINAL" || cedula.Trim() == "9999999999999")
            {
                ventana.txtCedula.Text = "";
                ventana.txtNombre.Text = "";
                ventana.txtCorreo.Text = "";
                ventana.txtDireccion.Text = "";
                ventana.txtTelefono.Text = "";

                ventana.btnGuardar.Visible = true;
                ventana.btnModificar.Visible = false;
                ventana.btnEliminar.Visible = false;
            }
            else
            {
                DataSet ds = _services.Cliente.ConsultarCedula(cedula);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    // Ya existe → editar
                    DataRow row = ds.Tables[0].Rows[0];

                    ventana.txtCedula.Text = row["CEDULA"].ToString();
                    ventana.txtNombre.Text = row["NOMBRE"].ToString();
                    ventana.txtCorreo.Text = row["CORREO"].ToString();
                    ventana.txtDireccion.Text = row["DIRECCION"].ToString();
                    ventana.txtTelefono.Text = row["TELEFONO"].ToString();

                    ventana.btnGuardar.Visible = false;
                    ventana.btnModificar.Visible = true;
                    ventana.btnEliminar.Visible = true;
                }
                else
                {
                    // NO existe → mostrar cédula y permitir crear
                    ventana.txtCedula.Text = cedula;
                    ventana.btnGuardar.Visible = true;
                    ventana.btnModificar.Visible = false;
                    ventana.btnEliminar.Visible = false;
                }
            }

            // mostrar
            if (ventana.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(ventana.CedulaRegistrada))
            {
                SetCedulaCliente(ventana.CedulaRegistrada);
                ActualizarBotonProcesar();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            frmNuevaNotaCredito ventana = new frmNuevaNotaCredito(_services);
            ventana.UsuarioActual = this.UsuarioActual;
            ventana.IPActual = this.IPActual;
            ventana.Show();
        }

        private void btnRetenciones_Click(object sender, EventArgs e)
        {
            frmProveedorRetenciones ventana = new frmProveedorRetenciones(_services);
            ventana.UsuarioActual = this.UsuarioActual;
            ventana.IPActual = this.IPActual;
            ventana.Show();
        }

        private async void btnPDF_Click(object sender, EventArgs e)
        {
            await VisualizarPDF();
        }

        private async Task VisualizarPDF()
        {

            try
            {

                // ==============================================================
                // 1) VALIDACIONES UI (IGUAL QUE btnProcesar)
                // ==============================================================


                var errores = Validar();
                if (errores.Count > 0)
                {
                    foreach (var err in errores)

                        Notificaciones.Show(this, err, "advertencia");

                    return;
                }

                var em = _services.Empresa.MostrarEmpresa(lblNombreEmpresa.Text);
                if (em == null || em.Tables[0].Rows.Count == 0)
                {
                    Notificaciones.Show(this, "Empresa no encontrada", "advertencia");
                    return;
                }

                var cm = _services.Cliente.ConsultarCedula(GetCedulaActual());
                if (cm == null || cm.Tables[0].Rows.Count == 0)
                {
                    Notificaciones.Show(this, "Cliente no encontrado.", "advertencia");
                    return;
                }

                DataRow rowCliente = cm.Tables[0].Rows[0];

                DataTable copiaDetalle = TB_DETALLEFACTURA.Copy();

                var pfm = _services.ParamFactura
                    .ConsultarNombre(lblNombreEmpresa.Text);

                var pm = _services.Param
                    .ConsultarPorTipo("01");

                // ==============================================================
                // 2) PREVIEW CORE (SIN INSERTAR / SIN SRI / SIN FIRMAR)
                // ==============================================================
                Notificaciones.Show(
                    this,
                    "Generando vista previa del PDF...",
                    "proceso",
                    UsuarioActual,
                    IPActual
                    );

                var resultado = await _services.ProcesosFacturacion.ProcesarFacturaPreview(
                    em,
                    pfm,
                    pm,
                    rowCliente,
                    copiaDetalle,
                    GetCedulaActual(),
                    lblFactura.Text,
                    lblFecha.Text,
                    lblTotalVP.Text,
                    txtFormaPago.Text.Trim(),
                    txtComentario.Text.Trim()
                );

                Notificaciones.CerrarProceso(this);

                if (!resultado.Exito)
                {
                    Notificaciones.Show(this, resultado.Mensaje, "error");
                    return;
                }

                // ==============================================================
                // 3) VISUALIZAR
                // ==============================================================
                Notificaciones.Show(this, "Visualizacion del PDF generada con éxito", "exito");

                System.Diagnostics.Process.Start(resultado.RutaPdf);
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, ex.Message, "error");
            }
        }

        private double CalcularTotalConImpuestos(DataRow filaDetalle, decimal tarifaIVA)
        {
            try
            {
                decimal baseTotal = Convert.ToDecimal(filaDetalle["TOTAL"]);
                string ivaProducto = filaDetalle["IVA"].ToString().Trim().ToUpperInvariant();
                string nombreProducto = filaDetalle["PRODUCTO"].ToString();
                decimal cantidad = Convert.ToDecimal(filaDetalle["CANTIDAD"]);

                decimal iceValor = 0m;

                if (filaDetalle.Table.Columns.Contains("APLICA_ICE"))
                {
                    var vIce = filaDetalle["APLICA_ICE"];
                    bool aplicaIce = false;
                    if (vIce != null && vIce != DBNull.Value)
                    {
                        if (vIce is bool bv) aplicaIce = bv;
                        else { string s = vIce.ToString().Trim().ToUpperInvariant(); aplicaIce = (s == "SI" || s == "1" || s == "TRUE" || s == "YES"); }
                    }

                    if (aplicaIce)
                    {
                        string iceTipo = filaDetalle["ICE_TIPO"]?.ToString().Trim().ToUpperInvariant() ?? "";
                        decimal icePct = 0m, iceVlr = 0m, azucar = 0m, volumen = 0m;
                        string iceUnidad = "";

                        decimal.TryParse(NormFrm(filaDetalle["ICE_PORCENTAJE"]?.ToString()), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out icePct);
                        decimal.TryParse(NormFrm(filaDetalle["ICE_VALOR"]?.ToString()),      System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out iceVlr);
                        decimal.TryParse(NormFrm(filaDetalle["AZUCAR_GRAMOS"]?.ToString()),  System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out azucar);
                        decimal.TryParse(NormFrm(filaDetalle["VOLUMEN"]?.ToString()),         System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out volumen);
                        iceUnidad = filaDetalle["ICE_UNIDAD"]?.ToString().Trim().ToUpperInvariant() ?? "";

                        if (iceTipo == "AD_VALOREM")
                            iceValor = Math.Round(baseTotal * (icePct / 100m), 2);
                        else if (iceTipo == "ESPECIFICO")
                        {
                            decimal unidades = (iceUnidad == "LITRO" && volumen > 0m) ? cantidad * volumen : cantidad;
                            iceValor = Math.Round(unidades * iceVlr, 2);
                        }
                        else if (iceTipo == "AZUCAR")
                            iceValor = Math.Round((azucar / 100m) * cantidad * iceVlr, 2);
                        else if (iceTipo == "MIXTO")
                        {
                            decimal litros = volumen > 0m ? cantidad * volumen : cantidad;
                            iceValor = Math.Round(baseTotal * (icePct / 100m), 2) + Math.Round(litros * iceVlr, 2);
                        }
                    }
                }

                decimal baseIVA = baseTotal + iceValor;
                decimal ivaValor = (ivaProducto == "SI" && tarifaIVA > 0m) ? Math.Round(baseIVA * tarifaIVA, 2) : 0m;

                return (double)Math.Round(baseTotal + iceValor + ivaValor, 2);
            }
            catch
            {
                return Convert.ToDouble(filaDetalle["TOTAL"]);
            }
        }

        private static string NormFrm(string s)
        {
            s = (s ?? "").Trim();
            if (s.Contains(",") && !s.Contains("."))
                s = s.Replace(",", ".");
            return s;
        }

        private void btnRefresh_Click_1(object sender, EventArgs e)
        {
            try
            {
                _services.CargarTarifaIva(lblNombreEmpresa.Text);
                ConsultaProducto();
                CargarBotonesFormasPago();
                CargarDatosEmpresa();
                Notificaciones.Show(this, "Productos, formas de pago y número de factura actualizados.", "exito");
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "No se pudo recargar: " + ex.Message, "advertencia");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SetCedulaCliente("9999999999999");
        }
    }
}
