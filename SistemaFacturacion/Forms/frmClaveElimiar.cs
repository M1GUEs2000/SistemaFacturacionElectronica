using LogicaNegocios.Services;
using System;
using System.Data;
using System.Windows.Forms;


namespace SistemaFacturacion
{
    public partial class frmClaveElimiar : Form
    {

        private readonly AppServices _services;

        public frmClaveElimiar(
            string Cadena,
           AppServices services

          )
        {
            _services = services;
            InitializeComponent();
            lblCadena.Text = Cadena;
        }

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        private void btnAceptarEli_Click(object sender, EventArgs e)
        {
            DataSet dsDato = new DataSet();
            if (txtClaveEliminar.Text != "")
            {
                dsDato = _services.Login.ConsultarClaveEliminar(txtClaveEliminar.Text);
                if (dsDato.Tables[0].Rows.Count > 0)
                {
                    string[] cadena = lblCadena.Text.Split(',');
                    string Fecha = cadena[0];
                    string formaPago = cadena[1];
                    string Producto = cadena[2];
                    string Cantidad = cadena[3];
                    string Total = cadena[4];
                    string Cliente = cadena[5];
                    string Hora = cadena[6];
                    string NumeroFactura = cadena[6];

                    int fila = _services.Facturacion.Eliminar(Fecha, formaPago, Producto, Cantidad, Total, Cliente, Hora, NumeroFactura, UsuarioActual, IPActual);
                    if (fila > 0)
                    {
                        this.Hide();
                        MessageBox.Show("Eliminado Correctamente.", "Mensaje");
                        DialogResult = DialogResult.OK;
                        //frmReportePorFechas frmReportecf = new frmReportePorFechas();
                        //frmReportecf.Show();
                    }
                    else
                    {
                        MessageBox.Show("No se Elimino.", "Mensaje");
                    }
                }
                else
                {
                    MessageBox.Show("Clave incorrecta", "Mensaje");
                }
            }
            else
            {
                MessageBox.Show("Ingrese una Clave ", "Mensaje");
            }
        }

        private void txtClaveEliminar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {


                DataSet dsDato = new DataSet();
                if (txtClaveEliminar.Text != "")
                {
                    dsDato = _services.Login.ConsultarClaveEliminar(txtClaveEliminar.Text);
                    if (dsDato.Tables[0].Rows.Count > 0)
                    {
                        string[] cadena = lblCadena.Text.Split(',');
                        string Fecha = cadena[0];
                        string formaPago = cadena[1];
                        string Producto = cadena[2];
                        string Cantidad = cadena[3];
                        string Total = cadena[4];
                        string Cliente = cadena[5];
                        string Hora = cadena[6];
                        string NumeroFactura = cadena[7];

                        int fila = _services.Facturacion.Eliminar(Fecha, formaPago, Producto, Cantidad, Total, Cliente, Hora, NumeroFactura, UsuarioActual, IPActual);
                        if (fila > 0)
                        {
                            this.Hide();
                            MessageBox.Show("Eliminado Correctamente.", "Mensaje");
                            DialogResult = DialogResult.OK;

                        }
                        else
                        {
                            MessageBox.Show("No se Elimino.", "Mensaje");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Clave incorrecta", "Mensaje");
                    }
                }
                else
                {
                    MessageBox.Show("Ingrese una Clave ", "Mensaje");
                }

            }
        }




    }
}
