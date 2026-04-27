using System.Collections.Generic;

namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudFactura
    {
        // ==========================================
        // CLIENTE
        // ==========================================
        public string CedulaCliente { get; set; }      // Para _services.Cliente.ConsultarCedula(...)
        public string ClienteTexto { get; set; }       // Texto libre/visible del cliente (como en tu WinForms)

        // ==========================================
        // FACTURA
        // ==========================================
        public bool EsFacturaElectronica { get; set; } // En tu backend se pasa "SI"/"NO"
        public string FechaPago { get; set; }          // El backend lo maneja como string (respeta tu formato actual)
        public string TotalVisual { get; set; }        // Backend lo recibe como string (tal cual tu UI)
        public string FormaPago { get; set; }          // Texto/valor que usas actualmente (ej: "01", "EFECTIVO", etc.)
        public string Comentario { get; set; }         // Observación / comentario

        // ==========================================
        // DETALLE (JSON -> List -> DataTable)
        // ==========================================
        public List<DetalleFacturaSolicitud> Detalles { get; set; } = new List<DetalleFacturaSolicitud>();

        // ==========================================
        // CONTEXTO
        // ==========================================
        public string Usuario { get; set; }
        public string Ip { get; set; }
    }

    public class DetalleFacturaSolicitud
    {
        // Nombres alineados a las columnas que armas en ConvertirADetalleDataTable:
        // PRODUCTO, CANTIDAD, VALOR, TOTAL, CODIGO
        public string Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal Valor { get; set; }
        public decimal Total { get; set; }
        public string Codigo { get; set; }
    }
}