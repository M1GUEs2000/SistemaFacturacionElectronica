using LogicaNegocios.Services;
using System;
using System.Windows.Forms;


namespace SistemaFacturacion
{
    public partial class frmMantenimientoTablas : Form
    {
        private readonly AppServices _services;

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        public frmMantenimientoTablas(AppServices services
)
        {
            InitializeComponent();
            _services = services;

        }
        private void btnProductos_Click(object sender, EventArgs e)
        {
            frmProductos frmProducto = new frmProductos(_services);
            frmProducto.UsuarioActual = this.UsuarioActual;
            frmProducto.IPActual = this.IPActual;
            frmProducto.Show();
        }

        private void btnEmpresa_Click(object sender, EventArgs e)
        {
            frmEmpresas frmEmpresa = new frmEmpresas(_services);
            frmEmpresa.UsuarioActual = this.UsuarioActual;
            frmEmpresa.IPActual = this.IPActual;
            frmEmpresa.Show();
        }

        private void btnFormaPago_Click(object sender, EventArgs e)
        {
            frmFormaPago frmFormaPago = new frmFormaPago(_services);
            frmFormaPago.UsuarioActual = this.UsuarioActual;
            frmFormaPago.IPActual = this.IPActual;
            frmFormaPago.Show();
        }

        private void btnClientes_Click(object sender, EventArgs e)
        {
            frmClientes frmClientes = new frmClientes(_services);
            frmClientes.UsuarioActual = this.UsuarioActual;
            frmClientes.IPActual = this.IPActual;
            frmClientes.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            frmParametrosFactura frmParametrosFactura = new frmParametrosFactura(_services);
            frmParametrosFactura.UsuarioActual = this.UsuarioActual;
            frmParametrosFactura.IPActual = this.IPActual;
            frmParametrosFactura.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmParametrosSRI frmParametrosSRI = new frmParametrosSRI(_services);
            frmParametrosSRI.UsuarioActual = this.UsuarioActual;
            frmParametrosSRI.IPActual = this.IPActual;
            frmParametrosSRI.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            frmConsultaLog frmParametrosSRI = new frmConsultaLog(_services);
            frmParametrosSRI.UsuarioActual = this.UsuarioActual;
            frmParametrosSRI.IPActual = this.IPActual;
            frmParametrosSRI.Show();
        }

        private void btnParamTrans_Click(object sender, EventArgs e)
        {
            frmParametrosTransaccion frmParametrosTransaccion = new frmParametrosTransaccion(_services);
            frmParametrosTransaccion.UsuarioActual = this.UsuarioActual;
            frmParametrosTransaccion.IPActual = this.IPActual;
            frmParametrosTransaccion.Show();
        }
    }
}
