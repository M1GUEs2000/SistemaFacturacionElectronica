using System.Data;

namespace DF_PinPad.Wrapper.Logging
{
    public interface IAuditQueryService
    {
        /// <summary>
        /// Devuelve las últimas N filas de dbo.vw_PinPad_Auditoria, más recientes primero.
        /// Pensado para poblar directamente un DataGrid en la interfaz gráfica.
        /// </summary>
        DataTable ObtenerHistorial(int maxFilas = 200);
    }
}
