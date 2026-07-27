using FirmaXadesNet;
using FirmaXadesNet.Crypto;
using FirmaXadesNet.Signature.Parameters;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace LogicaNegocios.Services
{
    public class InformacionCertificadoFirma
    {
        public string Titular { get; set; }
        public DateTime VigenteDesde { get; set; }
        public DateTime VigenteHasta { get; set; }
    }

    public class FirmadorNativo
    {
        public InformacionCertificadoFirma ValidarCertificado(string rutaP12, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(rutaP12) || !File.Exists(rutaP12))
                throw new FileNotFoundException("No se encontró el archivo P12 seleccionado.", rutaP12);

            if (string.IsNullOrWhiteSpace(contrasena))
                throw new ArgumentException("Ingrese la contraseña del certificado.", nameof(contrasena));

            using (var certificado = new X509Certificate2(
                rutaP12,
                contrasena,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet))
            {
                if (!certificado.HasPrivateKey)
                    throw new InvalidOperationException("El archivo P12 no contiene una clave privada para firmar.");

                DateTime ahora = DateTime.Now;
                if (ahora < certificado.NotBefore)
                    throw new InvalidOperationException("El certificado todavía no está vigente.");

                if (ahora > certificado.NotAfter)
                    throw new InvalidOperationException("El certificado está vencido desde " +
                        certificado.NotAfter.ToString("dd/MM/yyyy") + ".");

                return new InformacionCertificadoFirma
                {
                    Titular = certificado.Subject,
                    VigenteDesde = certificado.NotBefore,
                    VigenteHasta = certificado.NotAfter
                };
            }
        }

        /// <summary>
        /// Firma un XML con XAdES-BES enveloped usando RSA-SHA1, que es el perfil
        /// requerido por el SRI Ecuador. Reemplaza a PassStoreKS.Signer (IKVM + MITyCLib).
        /// El nodo firmado se detecta automáticamente desde el atributo id del elemento raíz
        /// (id="comprobante" en facturas, notas de crédito y retenciones).
        /// </summary>
        public void Sign(string rutaXmlSinFirmar, string rutaXmlFirmado, string rutaP12, string contrasena)
        {
            if (!File.Exists(rutaXmlSinFirmar))
                throw new Exception("No existe el XML a firmar: " + rutaXmlSinFirmar);

            if (!File.Exists(rutaP12))
                throw new Exception("No existe el archivo P12: " + rutaP12);

            Directory.CreateDirectory(Path.GetDirectoryName(rutaXmlFirmado));

            var cert = new X509Certificate2(
                rutaP12,
                contrasena,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet
            );

            var service = new XadesService();

            var parametros = new SignatureParameters
            {
                SignaturePackaging = SignaturePackaging.ENVELOPED,
                InputMimeType      = "text/xml",
                SignatureMethod    = SignatureMethod.RSAwithSHA1,
                DigestMethod       = DigestMethod.SHA1
                // SignaturePolicyInfo = null → XAdES-BES sin política (correcto para SRI)
            };

            using (parametros.Signer = new Signer(cert))
            using (var fs = new FileStream(rutaXmlSinFirmar, FileMode.Open, FileAccess.Read))
            {
                var docFirmado = service.Sign(fs, parametros);
                docFirmado.Save(rutaXmlFirmado);
            }
        }
    }
}
