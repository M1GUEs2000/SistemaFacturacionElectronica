using System;
using System.Drawing;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SistemaFacturacion.Controls
{
    internal static class IconosCasilleros
    {
        public const int Candado = 0xf023;
        public const int CandadoAbierto = 0xf09c;
        public const int Casilleros = 0xf009;
        public const int Campana = 0xf0f3;
        public const int Reloj = 0xf017;
        public const int Disponible = 0xf058;
        public const int Advertencia = 0xf071;
        public const int Configuracion = 0xf013;
        public const int Agregar = 0xf067;
        public const int Actualizar = 0xf021;
        public const int Cerrar = 0xf00d;
        public const int Ver = 0xf06e;
        public const int Llave = 0xf084;
        public const int Guardar = 0xf0c7;
        public const int Minimizar = 0xf068;
        public const int Usuario = 0xf007;
        public const int Duracion = 0xf252;
        public const int Buscar = 0xf002;
        public const int Pregunta = 0xf059;

        private static readonly PrivateFontCollection _coleccion = new PrivateFontCollection();
        private static FontFamily _familia;
        private static bool _intentoCarga;
        private const string RecursoEmbebido = "SistemaFacturacion.Recursos.Fonts.fontawesome-webfont.ttf";

        private static FontFamily Familia
        {
            get
            {
                if (!_intentoCarga) Cargar();
                return _familia;
            }
        }

        public static string Glifo(int codigoPunto) => char.ConvertFromUtf32(codigoPunto);

        public static Font CrearFuente(float tamano)
        {
            return Familia != null
                ? new Font(Familia, tamano, FontStyle.Regular, GraphicsUnit.Point)
                : new Font("Segoe UI", tamano, FontStyle.Regular, GraphicsUnit.Point);
        }

        private static void Cargar()
        {
            _intentoCarga = true;
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                using (var stream = asm.GetManifestResourceStream(RecursoEmbebido))
                {
                    if (stream == null) return;

                    byte[] datos = new byte[stream.Length];
                    stream.Read(datos, 0, datos.Length);

                    GCHandle handle = GCHandle.Alloc(datos, GCHandleType.Pinned);
                    try
                    {
                        _coleccion.AddMemoryFont(handle.AddrOfPinnedObject(), datos.Length);
                    }
                    finally
                    {
                        handle.Free();
                    }

                    if (_coleccion.Families.Length > 0)
                        _familia = _coleccion.Families[0];
                }
            }
            catch
            {
            }
        }
    }
}
