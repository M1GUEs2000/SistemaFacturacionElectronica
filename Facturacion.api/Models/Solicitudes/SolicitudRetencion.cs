using System;
using System.Collections.Generic;

namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudCrearRetencion
    {
        // ======================
        // PROVEEDOR
        // ======================
        public string TipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public string RazonSocial { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }

        public string TipoPersona { get; set; }
        public bool EsRimpe { get; set; }
        public string TipoRimpe { get; set; }
        public bool EsProfesional { get; set; }
        public bool EsArrendador { get; set; }

        // ======================
        // FACTURA PROVEEDOR
        // ======================
        public string NumeroFactura { get; set; }
        public string NumeroRetencion { get; set; }
        public DateTime FechaFactura { get; set; }
        public decimal BaseImponible { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }

        public string TipoOperacion { get; set; }

        // ======================
        // CONCEPTOS
        // ======================
        public List<ConceptoRetencionSolicitud> Conceptos { get; set; }
            = new List<ConceptoRetencionSolicitud>();

        // ======================
        // CONTEXTO
        // ======================
        public string Usuario { get; set; }
        public string Ip { get; set; }
    }

    public class ConceptoRetencionSolicitud
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public decimal BaseImponible { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal ValorRetenido { get; set; }
        public string TipoImpuesto { get; set; } // RENTA / IVA
    }
}