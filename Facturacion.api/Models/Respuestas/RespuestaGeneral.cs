namespace Facturacion.api.Models.Respuestas
{
    public class RespuestaGeneral<T>
    {
        public bool exito { get; set; }
        public string mensaje { get; set; }

        public T data { get; set; }

        public static RespuestaGeneral<T> Ok(T data, string mensaje = null)
        {
            return new RespuestaGeneral<T>
            {
                exito = true,
                mensaje = mensaje,
                data = data
            };
        }

        public static RespuestaGeneral<T> Fail(string mensaje)
        {
            return new RespuestaGeneral<T>
            {
                exito = false,
                mensaje = mensaje,
                data = default
            };
        }
    }
}
