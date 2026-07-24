using LogicaNegocios.Services;
using System;
using System.IO;
using System.Reflection;
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

            // Sin doble búfer el grid se pinta celda por celda EN VIVO al hacer scroll,
            // y por eso la data "aparece con delay". Se activa aquí, que es el punto
            // central por donde pasan las 4 pantallas, sin tocar ningún form.
            HabilitarDobleBuffer(grid);

            // UNA sola consulta para toda la grilla: mapa numero→estado. Antes esto
            // consultaba la BD por CADA fila (N+1), y cada consulta abría y cerraba una
            // conexión OleDb — con 200 filas eran 200 viajes a Access. Ahora se busca
            // en memoria.
            var estadosPendientes = services.Pendientes.ConsultarEstadosPendientesPorTipo(tipoDocumento);

            // SuspendLayout evita que el grid recalcule y repinte fila por fila mientras
            // se asignan los valores; se repinta una vez al final con ResumeLayout.
            grid.SuspendLayout();
            try
            {
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

                    // Estado del mapa en memoria (null si el documento no está pendiente);
                    // la decisión de qué pintar es la misma que usa la consulta individual.
                    estadosPendientes.TryGetValue(numero, out string estado);

                    var accion = services.Pendientes.ConstruirAccionDesdeEstado(
                        numero,
                        tipoDocumento,
                        estado
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
            finally
            {
                grid.ResumeLayout();
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

        // Cache de grids ya bufferizados: la reflexión es barata pero setear la propiedad
        // una sola vez por grid es más limpio y evita repetirlo en cada repintado.
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<DataGridView, object> _yaBufferizados
            = new System.Runtime.CompilerServices.ConditionalWeakTable<DataGridView, object>();

        /// <summary>
        /// Activa el doble búfer del DataGridView para que el scroll no pinte celda por
        /// celda en vivo. La propiedad DoubleBuffered es PROTEGIDA en DataGridView (no
        /// sale en el diseñador ni se puede setear normal), así que se pone por reflexión.
        /// </summary>
        public static void HabilitarDobleBuffer(DataGridView grid)
        {
            if (grid == null)
                return;

            // Por Escritorio Remoto el doble búfer es CONTRAPRODUCENTE: transfiere el
            // bitmap completo por la red en vez de dejar que GDI mande solo los cambios.
            // En sesión RDP se deja el comportamiento normal.
            if (SystemInformation.TerminalServerSession)
                return;

            // Una vez por grid basta; DoubleBuffered persiste mientras viva el control.
            if (_yaBufferizados.TryGetValue(grid, out _))
                return;

            try
            {
                typeof(DataGridView).InvokeMember(
                    "DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty,
                    null,
                    grid,
                    new object[] { true });

                _yaBufferizados.Add(grid, null);
            }
            catch
            {
                // Si la reflexión fallara (permisos, versión rara del framework), el grid
                // sigue funcionando exactamente como hoy: solo se pierde la mejora visual.
            }
        }
    }
}
