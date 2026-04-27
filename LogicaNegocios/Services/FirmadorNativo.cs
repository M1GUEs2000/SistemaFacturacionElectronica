using FirmaXadesNet;
using FirmaXadesNet.Crypto;
using FirmaXadesNet.Signature.Parameters;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace LogicaNegocios.Services
{
    public class FirmadorNativo
    {
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
