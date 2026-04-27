using System;
using System.Globalization;
using System.Text;

namespace LogicaNegocios.Procesos
{
    // DTO que retorna DetectarICE con todos los campos ICE pre-llenados
    public class ResultadoICE
    {
        public bool Detectado { get; set; }
        public string Categoria { get; set; }
        public string AplicaIce { get; set; }
        public string IceCodigo { get; set; }

        // AD_VALOREM | ESPECIFICO | AZUCAR | MIXTO
        public string IceTipo { get; set; }

        // Porcentaje (ej: 75 para 75%)
        public string IcePorcentaje { get; set; }

        // Valor específico (ej: 0.16 por unidad, 1.50 por litro)
        public string IceValor { get; set; }

        // Unidad de medida: UNIDAD | LITRO | 100G | (vacío si ad valorem puro)
        public string IceUnidad { get; set; }

        // NINGUNO | CONVENCIONAL | HIBRIDO | SENAE
        public string TipoVehiculo { get; set; }

        // Mensaje legible para mostrar al usuario
        public string Mensaje { get; set; }
    }

    /// <summary>
    /// Detecta el tipo de ICE de un producto según su nombre y calcula
    /// el código/tipo/tarifa correspondiente al SRI Ecuador.
    /// Reutilizable desde Windows Forms y desde la API REST.
    /// </summary>
    public static class ProcesosICE
    {
        // ====================================================================
        // DETECCIÓN POR NOMBRE
        // ====================================================================

        /// <summary>
        /// Analiza el nombre del producto y retorna los campos ICE
        /// pre-llenados según las reglas del SRI Ecuador (REGLAS_ICE.md).
        /// Para vehículos el código es provisional (depende del PVP);
        /// llame a ObtenerICEVehiculoPorPrecio() con el PVP definitivo.
        /// </summary>
        public static ResultadoICE DetectarICE(string nombreProducto)
        {
            if (string.IsNullOrWhiteSpace(nombreProducto))
                return SinICE();

            string n = Normalizar(nombreProducto);

            // ----------------------------------------------------------------
            // TABACO
            // ----------------------------------------------------------------

            // Tabaco calentado / nicotina (más específico que "tabaco" solo)
            if (Contiene(n, "tabaco calentado") || Contiene(n, "iqos") ||
                Contiene(n, "liquido nicotina") || Contiene(n, "pod nicotina") ||
                Contiene(n, "vape") || Contiene(n, "vaporizador"))
                return AdValorem("3682", "TABACO CALENTADO / NICOTINA", 150);

            // Cigarrillo negro (más específico que cigarrillo solo)
            if ((Contiene(n, "cigarrillo") && Contiene(n, "negro")) ||
                Contiene(n, "cigarrillo negro"))
                return Especifico("3021", "CIGARRILLO NEGRO", 0.16, "UNIDAD");

            // Cigarrillo (incluye rubios por defecto)
            if (Contiene(n, "cigarrillo") || Contiene(n, "cigarrillos"))
                return Especifico("3011", "CIGARRILLO RUBIO", 0.16, "UNIDAD");

            // Tabaco / cigarro puro
            if (Contiene(n, "tabaco") || Contiene(n, "cigarro") || Contiene(n, "puro"))
                return AdValorem("3023", "TABACO / CIGARRO", 150);

            // ----------------------------------------------------------------
            // BEBIDAS AZUCARADAS (antes que "bebida" genérico)
            // ----------------------------------------------------------------

            // Bebida energizante
            if (Contiene(n, "energizante") || Contiene(n, "energy drink") ||
                Contiene(n, "bebida energetica") || Contiene(n, "energy"))
                return AdValorem("3101", "BEBIDA ENERGIZANTE", 10);

            // Gaseosa bajo azúcar / diet / light
            if ((Contiene(n, "gaseosa") || Contiene(n, "cola") || Contiene(n, "soda")) &&
                (Contiene(n, "light") || Contiene(n, "diet") || Contiene(n, "zero") ||
                 Contiene(n, "sin azucar") || Contiene(n, "bajo azucar")))
                return AdValorem("3054", "GASEOSA BAJO AZÚCAR", 10);

            // Bebida no alcohólica con azúcar (jugo, néctar, gaseosa regular, agua saborizada)
            if (Contiene(n, "gaseosa") || Contiene(n, "cola") || Contiene(n, "soda") ||
                Contiene(n, "refresco") || Contiene(n, "bebida carbonatada"))
                return Azucar("3053", "GASEOSA / BEBIDA AZUCARADA", 0.18);

            if (Contiene(n, "jugo") || Contiene(n, "nectar") || Contiene(n, "agua saborizada") ||
                Contiene(n, "bebida fruta") || Contiene(n, "bebida no alcoholica") ||
                Contiene(n, "bebida"))
                return Azucar("3111", "BEBIDA NO ALCOHÓLICA", 0.18);

            // ----------------------------------------------------------------
            // ALCOHOL Y BEBIDAS ALCOHÓLICAS
            // ----------------------------------------------------------------

            // Cerveza artesanal (más específico que cerveza sola)
            if (Contiene(n, "cerveza artesanal") || Contiene(n, "craft beer") ||
                (Contiene(n, "cerveza") && Contiene(n, "artesanal")))
                return Especifico("3043", "CERVEZA ARTESANAL", 1.50, "LITRO");

            // Cerveza industrial
            if (Contiene(n, "cerveza"))
                return AdValorem("3041", "CERVEZA INDUSTRIAL", 75);

            // Bebidas alcohólicas específicas (ron, whisky, vodka, vino, etc.)
            if (Contiene(n, "ron") || Contiene(n, "whisky") || Contiene(n, "whiskey") ||
                Contiene(n, "vodka") || Contiene(n, "vino") || Contiene(n, "licor") ||
                Contiene(n, "aguardiente") || Contiene(n, "tequila") || Contiene(n, "gin") ||
                Contiene(n, "brandy") || Contiene(n, "champagne") || Contiene(n, "sangria") ||
                Contiene(n, "mezcal") || Contiene(n, "pisco") || Contiene(n, "anisado") ||
                Contiene(n, "coctail") || Contiene(n, "cocktail") || Contiene(n, "bebida alcoholica") ||
                (Contiene(n, "bebida") && Contiene(n, "alcohol")))
                return Mixto("3031", "BEBIDA ALCOHÓLICA", 75, 10.00, "LITRO");

            // Alcohol puro / industrial
            if (Contiene(n, "alcohol"))
                return Mixto("3033", "ALCOHOL", 75, 10.00, "LITRO");

            // ----------------------------------------------------------------
            // VEHÍCULOS
            // ----------------------------------------------------------------

            // Híbrido / eléctrico (más específico que vehículo solo)
            if (Contiene(n, "hibrido") || Contiene(n, "electrico") || Contiene(n, "hybrid") ||
                Contiene(n, "electrico") || Contiene(n, "plugin") || Contiene(n, "plug-in"))
                return CrearVehiculo("3688", "VEHÍCULO HÍBRIDO / ELÉCTRICO ≤$35k (provisional)", "HIBRIDO", 0,
                    "VEHÍCULO HÍBRIDO detectado. Código provisional 3688 (ICE=0%). Ingrese el PVP para auto-actualizar el código: ≤$35k=3688(0%), 35-40k=3691(8%), 40-50k=3692(14%), 50-60k=3695(20%), 60-70k=3696(26%), >70k=3698(32%)");

            // SENAE (importación)
            if ((Contiene(n, "vehiculo") || Contiene(n, "auto") || Contiene(n, "carro") || Contiene(n, "camioneta")) &&
                Contiene(n, "senae"))
                return CrearVehiculo("3871", "VEHÍCULO SENAE ≤$20k (provisional)", "SENAE", 5,
                    "VEHÍCULO SENAE detectado. Código provisional 3871(5%). Ingrese el PVP para auto-actualizar el código.");

            // Vehículo convencional
            if (Contiene(n, "vehiculo") || Contiene(n, "automovil") || Contiene(n, "carro") ||
                Contiene(n, "camioneta") || Contiene(n, "suv") || Contiene(n, "sedan") ||
                Contiene(n, "pickup") || Contiene(n, "pick up") || Contiene(n, "pick-up") ||
                Contiene(n, "jeep") || Contiene(n, "furgon") || Contiene(n, "van") ||
                Contiene(n, "moto") || Contiene(n, "motocicleta") || Contiene(n, "camion") ||
                Contiene(n, "trailer") || Contiene(n, "bus"))
                return CrearVehiculo("3073", "VEHÍCULO ≤$20k (provisional)", "CONVENCIONAL", 5,
                    "VEHÍCULO detectado. Código provisional 3073 (ICE=5%). Ingrese el PVP para auto-actualizar el código: ≤$20k=3073(5%), $30-40k=3075(15%), $40-50k=3077(20%), $50-60k=3078(25%), $60-70k=3079(30%), >$70k=3080(35%)");

            // ----------------------------------------------------------------
            // LUJO / TRANSPORTE MAYOR
            // ----------------------------------------------------------------

            if (Contiene(n, "avion") || Contiene(n, "yate") || Contiene(n, "barco") ||
                Contiene(n, "lancha") || Contiene(n, "helicoptero") || Contiene(n, "nave") ||
                Contiene(n, "jet"))
                return AdValorem("3081", "AVIÓN / YATE / BARCO", 10);

            // ----------------------------------------------------------------
            // CONSUMO
            // ----------------------------------------------------------------

            if (Contiene(n, "perfume") || Contiene(n, "colonia") || Contiene(n, "fragancia") ||
                Contiene(n, "eau de") || Contiene(n, "toilette") || Contiene(n, "eau de parfum") ||
                Contiene(n, "aftershave"))
                return AdValorem("3610", "PERFUME / FRAGANCIA", 20);

            if (Contiene(n, "videojuego") || Contiene(n, "video juego") || Contiene(n, "consola juego") ||
                Contiene(n, "playstation") || Contiene(n, "xbox") || Contiene(n, "nintendo switch"))
                return AdValorem("3620", "VIDEOJUEGO / CONSOLA", 0);

            if (Contiene(n, "arma de fuego") || Contiene(n, "pistola") || Contiene(n, "rifle") ||
                Contiene(n, "municion") || Contiene(n, "revolver") || Contiene(n, "escopeta") ||
                Contiene(n, "arma"))
                return AdValorem("3630", "ARMA / MUNICIÓN", 30);

            if (Contiene(n, "foco incandescente") || Contiene(n, "bombilla incandescente") ||
                (Contiene(n, "foco") && Contiene(n, "incandescente")))
                return AdValorem("3640", "FOCO INCANDESCENTE", 100);

            // ----------------------------------------------------------------
            // SERVICIOS
            // ----------------------------------------------------------------

            if (Contiene(n, "membresia") || Contiene(n, "afiliacion") ||
                Contiene(n, "cuota club") || Contiene(n, "suscripcion"))
                return AdValorem("3660", "MEMBRESÍA / AFILIACIÓN", 35);

            if ((Contiene(n, "telefonia") && (Contiene(n, "sociedad") || Contiene(n, "empresa") ||
                 Contiene(n, "corporativa") || Contiene(n, "plan empresa"))) ||
                Contiene(n, "plan corporativo"))
                return AdValorem("3093", "TELEFONÍA SOCIEDADES", 15);

            if (Contiene(n, "tv prepagada") || Contiene(n, "television prepagada") ||
                Contiene(n, "cable tv") || Contiene(n, "cable prepagado") || Contiene(n, "satelital"))
                return AdValorem("3092", "TV PREPAGADA", 0);

            if (Contiene(n, "celular") || Contiene(n, "telefono movil") || Contiene(n, "smartphone") ||
                Contiene(n, "iphone") || Contiene(n, "samsung") || Contiene(n, "plan movil"))
                return AdValorem("3681", "TELEFONÍA MÓVIL PERSONAS NATURALES", 0);

            // ----------------------------------------------------------------
            // OTROS
            // ----------------------------------------------------------------

            if (Contiene(n, "calefon") || Contiene(n, "calentador de agua"))
                return AdValorem("3671", "CALEFÓN", 100);

            if (Contiene(n, "funda plastica") || Contiene(n, "bolsa plastica") ||
                Contiene(n, "funda de plastico") || Contiene(n, "bolsa de plastico") ||
                (Contiene(n, "funda") && (Contiene(n, "plastico") || Contiene(n, "plastica"))))
                return Especifico("3680", "FUNDA PLÁSTICA", 0.08, "UNIDAD");

            // ----------------------------------------------------------------
            // IMPORTACIONES (SENAE / CAE — palabras clave de importación)
            // ----------------------------------------------------------------

            if ((Contiene(n, "bebida") || Contiene(n, "alcohol")) && Contiene(n, "importad"))
                return AdValorem("3533", "BEBIDA ALCOHÓLICA IMPORTADA (SENAE)", 75);

            if (Contiene(n, "tabaco") && Contiene(n, "importad"))
                return AdValorem("3544", "TABACO IMPORTADO (SENAE)", 150);

            if (Contiene(n, "videojuego") && Contiene(n, "importad"))
                return AdValorem("3720", "VIDEOJUEGO IMPORTADO (CAE)", 35);

            if (Contiene(n, "arma") && Contiene(n, "importad"))
                return AdValorem("3730", "ARMA IMPORTADA (CAE)", 30);

            if (Contiene(n, "foco") && Contiene(n, "importad"))
                return AdValorem("3740", "FOCO IMPORTADO (CAE)", 100);

            // Producto sin ICE conocido
            return SinICE();
        }

        // ====================================================================
        // DETERMINACIÓN DE CÓDIGO POR PVP (VEHÍCULOS)
        // ====================================================================

        /// <summary>
        /// Dado el PVP de un vehículo y su tipo, retorna el ResultadoICE
        /// con el código SRI correcto y el porcentaje exacto.
        /// Llamar después de DetectarICE cuando se conoce el precio.
        /// </summary>
        public static ResultadoICE ObtenerICEVehiculoPorPrecio(double pvp, string tipoVehiculo)
        {
            string tipo = (tipoVehiculo ?? "CONVENCIONAL").Trim().ToUpperInvariant();

            if (tipo == "HIBRIDO")
            {
                if (pvp <= 35000) return CrearVehiculo("3688", "VEHÍCULO HÍBRIDO ≤$35k", "HIBRIDO", 0, "");
                if (pvp <= 40000) return CrearVehiculo("3691", "VEHÍCULO HÍBRIDO $35k-$40k", "HIBRIDO", 8, "");
                if (pvp <= 50000) return CrearVehiculo("3692", "VEHÍCULO HÍBRIDO $40k-$50k", "HIBRIDO", 14, "");
                if (pvp <= 60000) return CrearVehiculo("3695", "VEHÍCULO HÍBRIDO $50k-$60k", "HIBRIDO", 20, "");
                if (pvp <= 70000) return CrearVehiculo("3696", "VEHÍCULO HÍBRIDO $60k-$70k", "HIBRIDO", 26, "");
                return CrearVehiculo("3698", "VEHÍCULO HÍBRIDO >$70k", "HIBRIDO", 32, "");
            }

            if (tipo == "SENAE")
            {
                if (pvp <= 20000) return CrearVehiculo("3871", "VEHÍCULO SENAE ≤$20k", "SENAE", 5, "");
                if (pvp <= 30000) return CrearVehiculo("3872", "VEHÍCULO SENAE $20k-$30k", "SENAE", 10, "");
                if (pvp <= 40000) return CrearVehiculo("3873", "VEHÍCULO SENAE $30k-$40k", "SENAE", 15, "");
                if (pvp <= 50000) return CrearVehiculo("3874", "VEHÍCULO SENAE $40k-$50k", "SENAE", 20, "");
                if (pvp <= 60000) return CrearVehiculo("3875", "VEHÍCULO SENAE $50k-$60k", "SENAE", 25, "");
                if (pvp <= 70000) return CrearVehiculo("3876", "VEHÍCULO SENAE $60k-$70k", "SENAE", 30, "");
                return CrearVehiculo("3877", "VEHÍCULO SENAE >$70k", "SENAE", 35, "");
            }

            // CONVENCIONAL (default)
            if (pvp <= 20000) return CrearVehiculo("3073", "VEHÍCULO ≤$20k", "CONVENCIONAL", 5, "");
            if (pvp <= 30000) return CrearVehiculo("3073", "VEHÍCULO ≤$30k", "CONVENCIONAL", 5, "");
            if (pvp <= 40000) return CrearVehiculo("3075", "VEHÍCULO $30k-$40k", "CONVENCIONAL", 15, "");
            if (pvp <= 50000) return CrearVehiculo("3077", "VEHÍCULO $40k-$50k", "CONVENCIONAL", 20, "");
            if (pvp <= 60000) return CrearVehiculo("3078", "VEHÍCULO $50k-$60k", "CONVENCIONAL", 25, "");
            if (pvp <= 70000) return CrearVehiculo("3079", "VEHÍCULO $60k-$70k", "CONVENCIONAL", 30, "");
            return CrearVehiculo("3080", "VEHÍCULO >$70k", "CONVENCIONAL", 35, "");
        }

        /// <summary>
        /// Retorna true si el código ICE detectado corresponde a un vehículo
        /// (el código cambia según el PVP ingresado).
        /// </summary>
        public static bool EsVehiculo(ResultadoICE resultado)
        {
            if (resultado == null || !resultado.Detectado) return false;
            string tv = (resultado.TipoVehiculo ?? "").ToUpperInvariant();
            return tv == "CONVENCIONAL" || tv == "HIBRIDO" || tv == "SENAE";
        }

        // ====================================================================
        // HELPERS INTERNOS
        // ====================================================================

        private static string Normalizar(string s)
        {
            if (s == null) return "";
            s = s.ToLowerInvariant();
            var sb = new StringBuilder();
            string decomposed = s.Normalize(NormalizationForm.FormD);
            foreach (char c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private static bool Contiene(string texto, string buscar)
        {
            return texto.IndexOf(buscar, StringComparison.Ordinal) >= 0;
        }

        private static ResultadoICE SinICE()
        {
            return new ResultadoICE
            {
                Detectado = false,
                Categoria = "",
                AplicaIce = "NO",
                IceCodigo = "",
                IceTipo = "",
                IcePorcentaje = "",
                IceValor = "",
                IceUnidad = "",
                TipoVehiculo = "NINGUNO",
                Mensaje = "Producto sin ICE detectado automáticamente."
            };
        }

        private static ResultadoICE AdValorem(string codigo, string categoria, double porcentaje)
        {
            return new ResultadoICE
            {
                Detectado = true,
                Categoria = categoria,
                AplicaIce = "SI",
                IceCodigo = codigo,
                IceTipo = "AD_VALOREM",
                IcePorcentaje = porcentaje.ToString("G", CultureInfo.InvariantCulture),
                IceValor = "",
                IceUnidad = "",
                TipoVehiculo = "NINGUNO",
                Mensaje = "Detectado: " + categoria + " — Cód. ICE " + codigo + " — AD VALOREM " + porcentaje + "%"
            };
        }

        private static ResultadoICE Especifico(string codigo, string categoria, double valor, string unidad)
        {
            return new ResultadoICE
            {
                Detectado = true,
                Categoria = categoria,
                AplicaIce = "SI",
                IceCodigo = codigo,
                IceTipo = "ESPECIFICO",
                IcePorcentaje = "",
                IceValor = valor.ToString("G", CultureInfo.InvariantCulture),
                IceUnidad = unidad,
                TipoVehiculo = "NINGUNO",
                Mensaje = "Detectado: " + categoria + " — Cód. ICE " + codigo + " — ESPECÍFICO $" + valor + " por " + unidad
            };
        }

        private static ResultadoICE Azucar(string codigo, string categoria, double tarifaPor100g)
        {
            return new ResultadoICE
            {
                Detectado = true,
                Categoria = categoria,
                AplicaIce = "SI",
                IceCodigo = codigo,
                IceTipo = "AZUCAR",
                IcePorcentaje = "",
                IceValor = tarifaPor100g.ToString("G", CultureInfo.InvariantCulture),
                IceUnidad = "100G",
                TipoVehiculo = "NINGUNO",
                Mensaje = "Detectado: " + categoria + " — Cód. ICE " + codigo + " — POR AZÚCAR $" + tarifaPor100g + " por cada 100g. Ingrese los gramos de azúcar."
            };
        }

        private static ResultadoICE Mixto(string codigo, string categoria, double porcentaje, double valorEspecifico, string unidad)
        {
            return new ResultadoICE
            {
                Detectado = true,
                Categoria = categoria,
                AplicaIce = "SI",
                IceCodigo = codigo,
                IceTipo = "MIXTO",
                IcePorcentaje = porcentaje.ToString("G", CultureInfo.InvariantCulture),
                IceValor = valorEspecifico.ToString("G", CultureInfo.InvariantCulture),
                IceUnidad = unidad,
                TipoVehiculo = "NINGUNO",
                Mensaje = "Detectado: " + categoria + " — Cód. ICE " + codigo + " — MIXTO " + porcentaje + "% + $" + valorEspecifico + " por " + unidad
            };
        }

        private static ResultadoICE CrearVehiculo(string codigo, string categoria, string tipoVehiculo, double porcentaje, string mensajeExtra)
        {
            string msg = string.IsNullOrEmpty(mensajeExtra)
                ? "Detectado: " + categoria + " — Cód. ICE " + codigo + " — AD VALOREM " + porcentaje + "%"
                : mensajeExtra;

            return new ResultadoICE
            {
                Detectado = true,
                Categoria = categoria,
                AplicaIce = "SI",
                IceCodigo = codigo,
                IceTipo = "AD_VALOREM",
                IcePorcentaje = porcentaje.ToString("G", CultureInfo.InvariantCulture),
                IceValor = "",
                IceUnidad = "",
                TipoVehiculo = tipoVehiculo,
                Mensaje = msg
            };
        }
    }
}
