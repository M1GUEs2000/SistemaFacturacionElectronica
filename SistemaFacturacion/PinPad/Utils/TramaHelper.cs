using System;
using System.Text;

namespace DF_PinPad.Wrapper.Utils
{
    /// <summary>
    /// La DLL DF_PinPad ya trae Byte2Hex/Hex2Byte dentro de clsComponenteSeguridad,
    /// pero como son miembros de una clase de negocio (no una utilidad aislada),
    /// se replica aquí una versión propia para usar libremente en logging sin
    /// acoplarse a esa clase interna.
    /// </summary>
    public static class TramaHelper
    {
        public static string BytesToHex(byte[] data)
        {
            if (data == null) return null;
            var sb = new StringBuilder(data.Length * 2);
            foreach (var b in data)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        public static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Array.Empty<byte>();
            if (hex.Length % 2 != 0)
                throw new ArgumentException("La cadena hexadecimal debe tener longitud par.", nameof(hex));

            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }
    }
}
