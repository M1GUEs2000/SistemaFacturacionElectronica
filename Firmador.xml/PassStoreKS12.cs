using es.mityc.firmaJava.libreria.utilidades;
using es.mityc.firmaJava.libreria.xades;
using es.mityc.javasign.pkstore;
using es.mityc.javasign.pkstore.keystore;
using es.mityc.javasign.trust;
using es.mityc.javasign.xml.refs;
using es.mityc.javasign.xml.xades.policy;
using java.io;
using java.security;
using java.security.cert;
using java.util;
using javax.xml.parsers;
using org.w3c.dom;
using System;
using System.Diagnostics;
using System.IO;

namespace Firmador

{
    public class PassStoreKS : IPassStoreKS
    {
        private string _password;

        public PassStoreKS(string password)
        {
            _password = password;
        }

        public char[] getPassword(X509Certificate certificate, string alias)
        {
            return _password.ToCharArray();
        }

        public class Signer
        {
            // ============================================
            // LOGGER SIMPLE A ARCHIVO
            // ============================================

            private X509Certificate _LoadCertificate(string path, string password, out PrivateKey privateKey, out Provider provider)
            {
                X509Certificate certificate = null;
                provider = null;
                privateKey = null;

                try
                {
                    LogFirma("======================================");
                    LogFirma("INICIO CARGA CERTIFICADO");
                    LogFirma("======================================");
                    LogFirma("PATH: " + path);

                    try
                    {
                        LogFirma("BASE_DIR: " + AppDomain.CurrentDomain.BaseDirectory);
                    }
                    catch { }

                    try
                    {
                        LogFirma("USER: " + Environment.UserName);
                    }
                    catch { }

                    try
                    {
                        LogFirma("64BIT_PROCESS: " + Environment.Is64BitProcess);
                        LogFirma("64BIT_OS: " + Environment.Is64BitOperatingSystem);
                    }
                    catch { }

                    LogFirma("PASSWORD_NULL: " + (password == null));
                    LogFirma("PASSWORD_LEN: " + (password == null ? 0 : password.Length));

                    bool existe = System.IO.File.Exists(path);
                    LogFirma("EXISTE_P12: " + existe);

                    if (!existe)
                        throw new Exception("El archivo P12 no existe.");

                    FileInfo fi = new FileInfo(path);
                    LogFirma("P12_SIZE: " + fi.Length);

                    LogFirma("ANTES KeyStore.getInstance(PKCS12)");
                    KeyStore ks = KeyStore.getInstance("PKCS12");
                    LogFirma("DESPUES KeyStore.getInstance(PKCS12)");

                    LogFirma("ANTES FileInputStream");
                    FileInputStream fis = new FileInputStream(path);
                    BufferedInputStream bis = new BufferedInputStream(fis);
                    LogFirma("DESPUES FileInputStream");

                    LogFirma("ANTES ks.load");
                    ks.load(bis, password.ToCharArray());
                    LogFirma("DESPUES ks.load");

                    LogFirma("ANTES KSStore");
                    IPKStoreManager storeManager = new KSStore(ks, new PassStoreKS(password));
                    LogFirma("DESPUES KSStore");

                    LogFirma("ANTES getSignCertificates");
                    List certificates = storeManager.getSignCertificates();
                    LogFirma("DESPUES getSignCertificates");

                    LogFirma("CERTIFICADOS_ENCONTRADOS: " + (certificates == null ? 0 : certificates.size()));

                    if (certificates != null && certificates.size() > 0)
                    {
                        certificate = (X509Certificate)certificates.get(0);

                        LogFirma("CERT_SUBJECT: " + certificate.getSubjectDN().getName());
                        LogFirma("CERT_ISSUER: " + certificate.getIssuerDN().getName());

                        LogFirma("ANTES getPrivateKey");
                        privateKey = storeManager.getPrivateKey(certificate);
                        LogFirma("DESPUES getPrivateKey");

                        LogFirma("ANTES getProvider");
                        provider = storeManager.getProvider(certificate);
                        LogFirma("DESPUES getProvider");

                        LogFirma("PROVIDER: " + (provider == null ? "NULL" : provider.getName()));

                        return certificate;
                    }

                    throw new Exception("No se encontraron certificados válidos en el P12.");
                }
                catch (java.lang.Throwable jex)
                {
                    string detalle = "JAVA ERROR: " + jex.toString();

                    try
                    {
                        java.lang.Throwable causa = (java.lang.Throwable)jex.getCause();
                        int nivel = 0;

                        while (causa != null && nivel < 10)
                        {
                            detalle += Environment.NewLine + "CAUSE[" + nivel + "]: " + causa.toString();
                            causa = (java.lang.Throwable)causa.getCause();
                            nivel++;
                        }
                    }
                    catch { }

                    LogFirma(detalle);
                    throw new Exception(detalle);
                }
                catch (Exception ex)
                {
                    LogFirma("ERROR FINAL EN CARGA CERTIFICADO: " + ex);
                    throw;
                }
            }
            private static readonly object _lockLog = new object();

            private void LogFirma(string texto)
            {
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string logDir = Path.Combine(baseDir, "logs_firma");
                    Directory.CreateDirectory(logDir);

                    string logPath = Path.Combine(logDir, "firma_" + DateTime.Now.ToString("yyyyMMdd") + ".log");

                    lock (_lockLog)
                    {
                        System.IO.File.AppendAllText(
                            logPath,
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " | " + texto + Environment.NewLine
                        );
                    }
                }
                catch
                {
                }
            }
            //private X509Certificate _LoadCertificate(string path, string password, out PrivateKey privateKey, out Provider provider)
            //{
            //    X509Certificate certificate = null;
            //    provider = null;
            //    privateKey = null;

            //    try
            //    {
            //        Debug.WriteLine("======================================");
            //        Debug.WriteLine("INICIO CARGA CERTIFICADO");
            //        Debug.WriteLine("======================================");

            //        // =========================================================
            //        // ENTORNO .NET
            //        // =========================================================

            //        Debug.WriteLine("PROCESS: " + Process.GetCurrentProcess().ProcessName);
            //        Debug.WriteLine("USER: " + Environment.UserName);
            //        Debug.WriteLine("DOTNET VERSION: " + Environment.Version);
            //        Debug.WriteLine("64BIT_PROCESS: " + Environment.Is64BitProcess);
            //        Debug.WriteLine("64BIT_OS: " + Environment.Is64BitOperatingSystem);

            //        Debug.WriteLine("BASE_DIR: " + AppDomain.CurrentDomain.BaseDirectory);
            //        Debug.WriteLine("CURRENT_DIR: " + Environment.CurrentDirectory);

            //        // =========================================================
            //        // VALIDACION ARCHIVO
            //        // =========================================================

            //        Debug.WriteLine("PATH: " + path);

            //        bool existe = System.IO.File.Exists(path);
            //        Debug.WriteLine("EXISTE: " + existe);

            //        if (!existe)
            //            throw new Exception("El archivo P12 no existe.");

            //        FileInfo fi = new FileInfo(path);

            //        Debug.WriteLine("TAMANIO_BYTES: " + fi.Length);

            //        byte[] header = new byte[16];

            //        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            //        {
            //            fs.Read(header, 0, header.Length);
            //        }

            //        Debug.WriteLine("HEADER_HEX: " + BitConverter.ToString(header));

            //        // =========================================================
            //        // PASSWORD
            //        // =========================================================

            //        Debug.WriteLine("PASSWORD_NULL: " + (password == null));
            //        Debug.WriteLine("PASSWORD_LEN: " + (password == null ? 0 : password.Length));

            //        if (password != null)
            //        {
            //            Debug.WriteLine("PASSWORD_HEX: " + BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(password)));
            //        }

            //        // =========================================================
            //        // JAVA PROVIDERS
            //        // =========================================================

            //        Debug.WriteLine("LISTANDO JAVA PROVIDERS...");

            //        var providers = Security.getProviders();

            //        Debug.WriteLine("PROVIDERS COUNT: " + providers.Length);

            //        for (int i = 0; i < providers.Length; i++)
            //        {
            //            Debug.WriteLine("JAVA_PROVIDER[" + i + "]: " + providers[i].getName());
            //        }

            //        // =========================================================
            //        // INICIALIZAR PROVIDERS SI FALTAN
            //        // =========================================================

            //        if (providers.Length <= 1)
            //        {
            //            Debug.WriteLine("AGREGANDO JAVA SECURITY PROVIDERS...");

            //            Security.addProvider(new sun.security.provider.Sun());
            //            Security.addProvider(new sun.security.rsa.SunRsaSign());
            //            Security.addProvider(new com.sun.net.ssl.@internal.ssl.Provider());

            //            Debug.WriteLine("PROVIDERS AGREGADOS.");
            //        }

            //        // =========================================================
            //        // CREAR KEYSTORE
            //        // =========================================================

            //        Debug.WriteLine("CREANDO KEYSTORE PKCS12...");

            //        KeyStore ks = KeyStore.getInstance("PKCS12");

            //        Debug.WriteLine("KEYSTORE TYPE: " + ks.getType());

            //        // =========================================================
            //        // CARGAR PKCS12
            //        // =========================================================

            //        Debug.WriteLine("CARGANDO PKCS12...");

            //        try
            //        {
            //            FileInputStream fis = new FileInputStream(path);
            //            BufferedInputStream bis = new BufferedInputStream(fis);

            //            ks.load(bis, password.ToCharArray());

            //            Debug.WriteLine("KEYSTORE LOAD: OK");
            //        }
            //        catch (Exception ex)
            //        {
            //            Debug.WriteLine("ERROR EN KEYSTORE.LOAD()");
            //            Debug.WriteLine("TIPO: " + ex.GetType().FullName);
            //            Debug.WriteLine("MENSAJE: " + ex.Message);

            //            Exception inner = ex.InnerException;
            //            int nivel = 0;

            //            while (inner != null && nivel < 10)
            //            {
            //                Debug.WriteLine("INNER[" + nivel + "]: " + inner.GetType().FullName + " - " + inner.Message);
            //                inner = inner.InnerException;
            //                nivel++;
            //            }

            //            throw;
            //        }

            //        // =========================================================
            //        // LEER CERTIFICADOS
            //        // =========================================================

            //        Debug.WriteLine("LEYENDO CERTIFICADOS...");

            //        IPKStoreManager storeManager = new KSStore(ks, new PassStoreKS(password));

            //        List certificates = storeManager.getSignCertificates();

            //        Debug.WriteLine("CERTIFICADOS ENCONTRADOS: " + (certificates == null ? 0 : certificates.size()));

            //        if (certificates != null && certificates.size() > 0)
            //        {
            //            certificate = (X509Certificate)certificates.get(0);

            //            Debug.WriteLine("CERT SUBJECT: " + certificate.getSubjectDN().getName());
            //            Debug.WriteLine("CERT ISSUER: " + certificate.getIssuerDN().getName());

            //            privateKey = storeManager.getPrivateKey(certificate);
            //            provider = storeManager.getProvider(certificate);

            //            Debug.WriteLine("PRIVATE KEY: OK");
            //            Debug.WriteLine("PROVIDER: " + (provider == null ? "NULL" : provider.getName()));

            //            return certificate;
            //        }

            //        throw new Exception("No se encontraron certificados válidos en el P12.");
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine("ERROR FINAL EN CARGA CERTIFICADO");
            //        Debug.WriteLine(ex.ToString());
            //        throw;
            //    }
            //}



            //Nuevo sin xerces

            //public void Sign(string unsignedXmlPath, string signedXmlPath, string pfxPath, string pfxPassword)
            //{
            //    LogFirma("========== INICIO Sign ==========");
            //    LogFirma("unsignedXmlPath: " + unsignedXmlPath);
            //    LogFirma("signedXmlPath: " + signedXmlPath);
            //    LogFirma("pfxPath: " + pfxPath);

            //    PrivateKey privateKey;
            //    Provider provider;

            //    X509Certificate certificate = _LoadCertificate(pfxPath, pfxPassword, out privateKey, out provider);

            //    if (certificate != null)
            //    {
            //        try
            //        {
            //            LogFirma("ANTES TrustExtendFactory.newInstance()");
            //            TrustFactory.instance = es.mityc.javasign.trust.TrustExtendFactory.newInstance();
            //            LogFirma("DESPUES TrustExtendFactory.newInstance()");

            //            LogFirma("ANTES MyPropsTruster.getInstance()");
            //            TrustFactory.truster = es.mityc.javasign.trust.MyPropsTruster.getInstance();
            //            LogFirma("DESPUES MyPropsTruster.getInstance()");

            //            LogFirma("ANTES POLICY_SIGN");
            //            PoliciesManager.POLICY_SIGN = new es.mityc.javasign.xml.xades.policy.facturae.Facturae31Manager();
            //            LogFirma("DESPUES POLICY_SIGN");

            //            LogFirma("ANTES POLICY_VALIDATION");
            //            PoliciesManager.POLICY_VALIDATION = new es.mityc.javasign.xml.xades.policy.facturae.Facturae31Manager();
            //            LogFirma("DESPUES POLICY_VALIDATION");

            //            LogFirma("ANTES DocumentBuilderFactory.newInstance()");
            //            DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
            //            LogFirma("DESPUES DocumentBuilderFactory.newInstance()");

            //            dbf.setNamespaceAware(true);
            //            LogFirma("DESPUES dbf.setNamespaceAware(true)");

            //            LogFirma("ANTES parse unsigned XML");
            //            Document unsignedDocument = dbf
            //                .newDocumentBuilder()
            //                .parse(new BufferedInputStream(new FileInputStream(unsignedXmlPath)));
            //            LogFirma("DESPUES parse unsigned XML");

            //            LogFirma("ANTES construir DataToSign");
            //            DataToSign dataToSign = new DataToSign();
            //            dataToSign.setXadesFormat(EnumFormatoFirma.XAdES_BES);
            //            dataToSign.setEsquema(XAdESSchemas.XAdES_132);
            //            dataToSign.setXMLEncoding("UTF-8");
            //            dataToSign.setEnveloped(true);
            //            dataToSign.addObject(new ObjectToSign(
            //                new InternObjectToSign("comprobante"),
            //                "contenido comprobante",
            //                null,
            //                "text/xml",
            //                null
            //            ));
            //            dataToSign.setParentSignNode("comprobante");
            //            dataToSign.setDocument(unsignedDocument);
            //            LogFirma("DESPUES construir DataToSign");

            //            LogFirma("ANTES FirmaXML.signFile()");
            //            Object[] res = new FirmaXML().signFile(certificate, dataToSign, privateKey, provider);
            //            LogFirma("DESPUES FirmaXML.signFile()");

            //            LogFirma("ANTES guardar XML firmado");
            //            FileOutputStream fs = new FileOutputStream(signedXmlPath);
            //            UtilidadTratarNodo.saveDocumentToOutputStream((Document)res[0], fs, true);
            //            fs.close();
            //            LogFirma("DESPUES guardar XML firmado");
            //        }
            //        catch (java.lang.Throwable jex)
            //        {
            //            string detalle = "";
            //            try { detalle += "JAVA TYPE: " + jex.getClass().getName() + Environment.NewLine; } catch { }
            //            try { detalle += "JAVA TO_STRING: " + jex.toString() + Environment.NewLine; } catch { }
            //            try { detalle += "JAVA MESSAGE: " + jex.getMessage() + Environment.NewLine; } catch { }

            //            try
            //            {
            //                java.io.StringWriter sw = new java.io.StringWriter();
            //                java.io.PrintWriter pw = new java.io.PrintWriter(sw);
            //                jex.printStackTrace(pw);
            //                pw.flush();
            //                detalle += "JAVA STACKTRACE:" + Environment.NewLine + sw.toString() + Environment.NewLine;
            //            }
            //            catch { }

            //            try
            //            {
            //                java.lang.Throwable causa = (java.lang.Throwable)jex.getCause();
            //                int nivel = 0;

            //                while (causa != null && nivel < 10)
            //                {
            //                    detalle += "CAUSE[" + nivel + "] TYPE: " + causa.getClass().getName() + Environment.NewLine;
            //                    detalle += "CAUSE[" + nivel + "] TO_STRING: " + causa.toString() + Environment.NewLine;

            //                    try { detalle += "CAUSE[" + nivel + "] MESSAGE: " + causa.getMessage() + Environment.NewLine; } catch { }

            //                    causa = (java.lang.Throwable)causa.getCause();
            //                    nivel++;
            //                }
            //            }
            //            catch { }

            //            LogFirma("ERROR JAVA EN Sign(): " + detalle);
            //            throw new Exception(detalle);
            //        }
            //        catch (Exception ex)
            //        {
            //            LogFirma("ERROR .NET EN Sign(): " + ex);
            //            throw;
            //        }
            //    }
            //    else
            //    {
            //        LogFirma("certificate == null");
            //    }

            //    LogFirma("========== FIN Sign ==========");
            //}

            //ANTIGUO XERCES

            public void Sign(string unsignedXmlPath, string signedXmlPath, string pfxPath, string pfxPassword)
            {


                PrivateKey privateKey;
                Provider provider;
                X509Certificate certificate = _LoadCertificate(pfxPath, pfxPassword, out privateKey, out provider);

                if (certificate != null)
                {
                    TrustFactory.instance = es.mityc.javasign.trust.TrustExtendFactory.newInstance();
                    TrustFactory.truster = es.mityc.javasign.trust.MyPropsTruster.getInstance();
                    PoliciesManager.POLICY_SIGN = new es.mityc.javasign.xml.xades.policy.facturae.Facturae31Manager();
                    PoliciesManager.POLICY_VALIDATION = new es.mityc.javasign.xml.xades.policy.facturae.Facturae31Manager();
                    com.sun.org.apache.xerces.@internal.jaxp.SAXParserFactoryImpl s = new com.sun.org.apache.xerces.@internal.jaxp.SAXParserFactoryImpl();
                    DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
                    dbf.setNamespaceAware(true);
                    Document unsignedDocument = dbf.newDocumentBuilder().parse(new BufferedInputStream(new FileInputStream(unsignedXmlPath)));

                    DataToSign dataToSign = new DataToSign();
                    dataToSign.setXadesFormat(EnumFormatoFirma.XAdES_BES);
                    dataToSign.setEsquema(XAdESSchemas.XAdES_132);
                    dataToSign.setXMLEncoding("UTF-8");
                    dataToSign.setEnveloped(true);
                    dataToSign.addObject(new ObjectToSign(new InternObjectToSign("comprobante"), "contenido comprobante", null, "text/xml", null));
                    dataToSign.setParentSignNode("comprobante");
                    dataToSign.setDocument(unsignedDocument);

                    Object[] res = new FirmaXML().signFile(certificate, dataToSign, privateKey, provider);

                    FileOutputStream fs = new FileOutputStream(signedXmlPath);
                    UtilidadTratarNodo.saveDocumentToOutputStream((Document)res[0], fs, true);
                    fs.close();

                }

            }
        }
    }
}
