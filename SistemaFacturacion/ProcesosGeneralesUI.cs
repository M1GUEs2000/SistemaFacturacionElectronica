using LogicaNegocios.Services;
using System;
using System.IO;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public class ProcesosGeneralesUI
    {

        private readonly AppServices _services;

        public ProcesosGeneralesUI(AppServices services)
        {
            _services = services;
        }

        public static void AbrirDocumentoPorGrid(
 DataGridViewRow fila,
 string nombreColumnaFecha,
 string numeroDocumento,
 string carpetaBase,          // ← ahora recibe la ruta base real
 string subcarpeta,           // PDF / XML / XMLFIRMADOS
 string extension,
 string nombreDocumentoUsuario
)
        {
            try
            {
                if (fila == null || string.IsNullOrWhiteSpace(numeroDocumento))
                    return;

                string fechaTexto = fila.Cells[nombreColumnaFecha]?.Value?.ToString() ?? "";

                if (!DateTime.TryParse(fechaTexto, out DateTime fecha))
                {
                    MessageBox.Show("Fecha inválida para localizar el documento.");
                    return;
                }

                string fechaFormato = fecha.ToString("ddMMyyyy");

                // patrón: fecha + número + extensión
                string patron = $"*{fechaFormato}*{numeroDocumento}*.{extension}";

                string carpeta = Path.Combine(carpetaBase, subcarpeta);

                if (!Directory.Exists(carpeta))
                {
                    MessageBox.Show($"No existe la carpeta de {nombreDocumentoUsuario}.");
                    return;
                }

                string[] archivos = Directory.GetFiles(carpeta, patron);

                if (archivos.Length > 0)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = archivos[0],
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show($"No se encontró el {extension.ToUpper()} de la {nombreDocumentoUsuario}.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir {extension.ToUpper()} de la {nombreDocumentoUsuario}:\n{ex.Message}");
            }
        }

        public static void PintarColumnasDocumento(
            DataGridView grid,
            string nombreColumnaNumero,
            string nombreColumnaBoton,
            string tipoDocumento,
            AppServices services,
            bool manejarConsumidorFinal = false
        )
        {
            if (grid == null || services == null)
                return;

            if (!grid.Columns.Contains(nombreColumnaNumero) ||
                !grid.Columns.Contains(nombreColumnaBoton) ||
                !grid.Columns.Contains("colPDF") ||
                !grid.Columns.Contains("colXML"))
                return;

            foreach (DataGridViewRow fila in grid.Rows)
            {
                if (fila.IsNewRow)
                    continue;

                string numero = fila.Cells[nombreColumnaNumero].Value?
                    .ToString()?
                    .Trim()
                    .ToUpperInvariant() ?? "";

                // LIMPIAR
                fila.Cells[nombreColumnaBoton].Value = "";
                fila.Cells["colPDF"].Value = null;
                fila.Cells["colXML"].Value = null;

                if (string.IsNullOrWhiteSpace(numero))
                    continue;

                // ----------------------------------------------------
                // CASO ESPECIAL: CONSUMIDOR FINAL
                // ----------------------------------------------------
                if (manejarConsumidorFinal && numero.StartsWith("FINAL"))
                {
                    fila.Cells[nombreColumnaBoton].Value = "PROCESAR";
                    continue;
                }

                var accion = services.Pendientes.ConsultarAccionPendienteDocumento(
                    numero,
                    tipoDocumento
                );

                if (accion != null)
                {
                    if (accion.Existe && !string.IsNullOrWhiteSpace(accion.TextoBoton))
                    {
                        fila.Cells[nombreColumnaBoton].Value = accion.TextoBoton;
                    }

                    if (accion.MostrarPdf)
                    {
                        fila.Cells["colPDF"].Value = Properties.Resources.pdf;
                    }

                    if (accion.MostrarXml)
                    {
                        fila.Cells["colXML"].Value = Properties.Resources.xml;
                    }
                }
            }
        }
        public static void AgregarColumnasAccionesReportes(
    DataGridView grid,
    string nombreColumnaBoton,
    string headerBoton,
    int anchoBoton,
    bool limpiarAntes = true
)
        {
            if (limpiarAntes)
            {
                if (grid.Columns.Contains(nombreColumnaBoton))
                    grid.Columns.Remove(nombreColumnaBoton);

                if (grid.Columns.Contains("colPDF"))
                    grid.Columns.Remove("colPDF");

                if (grid.Columns.Contains("colXML"))
                    grid.Columns.Remove("colXML");
            }

            // BOTÓN
            DataGridViewButtonColumn colBoton = new DataGridViewButtonColumn
            {
                Name = nombreColumnaBoton,
                HeaderText = headerBoton,
                UseColumnTextForButtonValue = false,
                Width = anchoBoton,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };

            grid.Columns.Add(colBoton);

            // PDF
            DataGridViewImageColumn colPDF = new DataGridViewImageColumn
            {
                Name = "colPDF",
                HeaderText = "PDF",
                Width = 50,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };

            grid.Columns.Add(colPDF);

            // XML
            DataGridViewImageColumn colXML = new DataGridViewImageColumn
            {
                Name = "colXML",
                HeaderText = "XML",
                Width = 50,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };

            grid.Columns.Add(colXML);

            // ORDEN FINAL
            int last = grid.Columns.Count - 1;

            grid.Columns[nombreColumnaBoton].DisplayIndex = last - 2;
            grid.Columns["colPDF"].DisplayIndex = last - 1;
            grid.Columns["colXML"].DisplayIndex = last;
        }
    }
}
