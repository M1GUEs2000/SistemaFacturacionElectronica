using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Media;

namespace SistemaFacturacion.Controls
{
    internal static class SonidosCasilleros
    {
        public const string ClaveDefecto = "campana.wav";

        private static readonly string[] ExtensionesSoportadas = { "*.wav", "*.mp3" };
        private static MediaPlayer _reproductor;

        private static string CarpetaSonidos =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recursos", "SonidosCasilleros");

        public static List<(string Clave, string Etiqueta)> ObtenerDisponibles()
        {
            if (!Directory.Exists(CarpetaSonidos))
                return new List<(string, string)>();

            return ExtensionesSoportadas
                .SelectMany(patron => Directory.GetFiles(CarpetaSonidos, patron))
                .OrderBy(archivo => archivo)
                .Select(archivo => (Path.GetFileName(archivo), FormatearEtiqueta(Path.GetFileNameWithoutExtension(archivo))))
                .ToList();
        }

        private static string FormatearEtiqueta(string nombre)
        {
            string conEspacios = nombre.Replace('_', ' ').Replace('-', ' ');
            return conEspacios.Length == 0 ? nombre : char.ToUpper(conEspacios[0]) + conEspacios.Substring(1);
        }

        public static string AgregarPersonalizado(string rutaOrigen)
        {
            Directory.CreateDirectory(CarpetaSonidos);

            string nombreArchivo = Path.GetFileName(rutaOrigen);
            string destino = Path.Combine(CarpetaSonidos, nombreArchivo);

            int contador = 1;
            while (File.Exists(destino))
            {
                nombreArchivo = Path.GetFileNameWithoutExtension(rutaOrigen) + "_" + contador + Path.GetExtension(rutaOrigen);
                destino = Path.Combine(CarpetaSonidos, nombreArchivo);
                contador++;
            }

            File.Copy(rutaOrigen, destino);
            return nombreArchivo;
        }

        public static void Reproducir(string nombreArchivo)
        {
            string ruta = Path.Combine(CarpetaSonidos, string.IsNullOrWhiteSpace(nombreArchivo) ? ClaveDefecto : nombreArchivo);
            if (!File.Exists(ruta))
            {
                SystemSounds.Asterisk.Play();
                return;
            }

            if (_reproductor == null)
                _reproductor = new MediaPlayer();

            _reproductor.Open(new Uri(ruta, UriKind.Absolute));
            _reproductor.Position = TimeSpan.Zero;
            _reproductor.Play();
        }
    }
}
