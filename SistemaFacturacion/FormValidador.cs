using System.Collections.Generic;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    /// <summary>
    /// Helper de validación genérica para formularios WinForms.
    ///
    /// Dos patrones de uso:
    ///
    /// PATRÓN ACUMULADOR — muestra todos los errores de una vez:
    ///     var errores = new List&lt;string&gt;();
    ///     FormValidador.Requerido(txtNombre.Text, "Nombre", errores);
    ///     FormValidador.Requerido(txtCorreo.Text, "Correo", errores);
    ///     if (FormValidador.MostrarErrores(this, errores)) return;
    ///
    /// PATRÓN DIRECTO — detiene al primer campo vacío:
    ///     if (FormValidador.EsVacio(this, txtCedula.Text, "Cédula", txtCedula)) return false;
    /// </summary>
    internal static class FormValidador
    {
        // ─── PATRÓN ACUMULADOR ────────────────────────────────────────────

        /// <summary>Añade un error a la lista si el valor está vacío.</summary>
        public static void Requerido(string valor, string etiqueta, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(valor))
                errores.Add("- " + etiqueta + " es obligatorio");
        }

        /// <summary>
        /// Muestra todos los errores acumulados con Notificaciones y retorna
        /// true si había al menos uno (para que el caller pueda hacer return).
        /// </summary>
        public static bool MostrarErrores(Form form, List<string> errores)
        {
            if (errores.Count == 0) return false;
            Notificaciones.Show(form, string.Join("\n", errores), "advertencia");
            return true;
        }

        // ─── PATRÓN DIRECTO ───────────────────────────────────────────────

        /// <summary>
        /// Muestra aviso y enfoca el control al instante.
        /// Retorna true si el campo está vacío (hay error).
        /// </summary>
        public static bool EsVacio(Form form, string valor, string etiqueta, Control enfocar = null)
        {
            if (!string.IsNullOrWhiteSpace(valor)) return false;
            Notificaciones.Show(form, etiqueta + " es obligatorio.", "advertencia");
            enfocar?.Focus();
            return true;
        }
    }
}
