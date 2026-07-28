using System;
using System.Configuration;

namespace DF_PinPad.Wrapper.Config
{
    /// <summary>
    /// Configuración necesaria para construir un DF_PinPad.LANConfig / DF_PinPad.LAN
    /// (constructor real, confirmado por reflection sobre DF_PinPad.dll):
    ///
    ///   LANConfig(string IP, int Puerto, int TimeOut, string MID, string TID,
    ///             string CajaID, int Version, int SHA)
    ///
    /// Version y SHA son enteros que corresponden a los enums internos de la DLL:
    ///   DF_PinPad.ELANVersion { V4_2 = 0, V4_4 = 1, VFastTrack = 2 }
    ///   DF_PinPad.ESHA        { NoSha = 0, SHA1 = 1, SHA2 = 2 }
    /// </summary>
    public class PinPadSettings
    {
        public string IP { get; set; }
        public int Puerto { get; set; }
        public int TimeOutMs { get; set; }
        public string MID { get; set; }
        public string TID { get; set; }
        public string CajaID { get; set; }

        /// <summary>0 = V4_2, 1 = V4_4, 2 = VFastTrack</summary>
        public int Version { get; set; }

        /// <summary>0 = NoSha, 1 = SHA1, 2 = SHA2</summary>
        public int SHA { get; set; }

        public string SqlConnectionString { get; set; }

        public static PinPadSettings LoadFromAppConfig()
        {
            return new PinPadSettings
            {
                IP = ConfigurationManager.AppSettings["PinPad.IP"],
                Puerto = int.Parse(ConfigurationManager.AppSettings["PinPad.Puerto"] ?? "0"),
                TimeOutMs = int.Parse(ConfigurationManager.AppSettings["PinPad.TimeOutMs"] ?? "30000"),
                MID = ConfigurationManager.AppSettings["PinPad.MID"],
                TID = ConfigurationManager.AppSettings["PinPad.TID"],
                CajaID = ConfigurationManager.AppSettings["PinPad.CajaID"],
                Version = int.Parse(ConfigurationManager.AppSettings["PinPad.Version"] ?? "1"), // 1 = V4_4
                SHA = int.Parse(ConfigurationManager.AppSettings["PinPad.SHA"] ?? "1"),         // 1 = SHA1
                SqlConnectionString = ConfigurationManager.ConnectionStrings["PinPadLogsDb"]?.ConnectionString
                    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'PinPadLogsDb' en App.config.")
            };
        }

        /// <summary>
        /// Persiste IP/Puerto/MID/TID/CajaID de vuelta al App.config físico del ejecutable
        /// (DF_PinPad.App.exe.config, junto al .exe), para que la próxima vez que se abra la
        /// aplicación arranque con los últimos valores usados desde la pestaña "0. Conexión",
        /// en vez de siempre volver a los valores originales del proyecto.
        /// No afecta la cadena de conexión SQL ni TimeOut/Version/SHA (esos se siguen editando
        /// directamente en el archivo si hace falta).
        /// </summary>
        public void SaveToAppConfig()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            SetOrAdd(config, "PinPad.IP", IP);
            SetOrAdd(config, "PinPad.Puerto", Puerto.ToString());
            SetOrAdd(config, "PinPad.MID", MID);
            SetOrAdd(config, "PinPad.TID", TID);
            SetOrAdd(config, "PinPad.CajaID", CajaID);

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static void SetOrAdd(Configuration config, string key, string value)
        {
            if (config.AppSettings.Settings[key] != null)
                config.AppSettings.Settings[key].Value = value ?? string.Empty;
            else
                config.AppSettings.Settings.Add(key, value ?? string.Empty);
        }
    }
}
