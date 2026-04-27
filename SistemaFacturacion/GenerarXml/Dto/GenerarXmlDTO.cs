
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenerarXml.Dto
{
    public class GenerarXmlDTO
    {
     
        public int Id { get; set; }


        public string ComprobantevTipo { get; set; }
   
        public string RucEmpresa { get; set; }
    }

    public class ViewXmlDto
    {
        public int Id { get; set; }
        public string RutaXml { get; set; }
        public string Error { get; set; }

    }


    public class ArchivoP12Dto
    {
        public int FkEmpresa { get; set; }
        public string Dfecontribuyente { get; set; }
        public string Dfeubicacionarchivop12 { get; set; }
        public string Dfecontrasena { get; set; }
        public string Dfeimagen { get; set; }
        public bool? Dfepruebaproduccion { get; set; }

    }
    public class ViewArchivoP12Dto
    {
        public int Id { get; set; }

    }
    public class PdfDTO:RecepcionDto
    {
       


    }

    public class ViewPathPDFDTo
    {
        public int Id { get; set; }
        public string RutaPDF { get; set; }
        public string Error { get; set; }
    }


    public class FirmaXmlDto : GenerarXmlDTO
    {


    }

    public class RecepcionDto
    {

        public string ClaveAcceso { get; set; }


        public string RucEmpresa { get; set; }


    }
    public class AutorizacionDto :RecepcionDto
    {

   

    }


    public class RespuestaRecepcionSri
    {

        public string Error { get; set; }
        public string RespuestaRecepcion { get; set; }

    }

    public class RespuestaAutorizacionSri
    {
        public string RespuestaAutorizacion { get; set; }
        public string Error { get; set; }
        public string Estado { get; set; }
    }

}
