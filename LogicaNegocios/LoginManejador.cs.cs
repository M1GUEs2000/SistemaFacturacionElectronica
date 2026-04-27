using AccesoDatos.Abstractions;
using System.Data;
using System.Net;
using System.Net.Sockets;

namespace LogicaNegocios
{
    public class LoginManejador
    {
        private readonly IConexionBD _conexion;

        public LoginManejador(IConexionBD conexion)
        {
            _conexion = conexion;
        }
        public DataSet ConsultarUsuario(string CLAVE)
        {
            return _conexion.Seleccionar(
                "SELECT * FROM EMPRESA WHERE CLAVEINGRESO = @clave",
                ("clave", CLAVE));
        }
        public DataSet ConsultarClaveEliminar(string CLAVE)
        {
            return _conexion.Seleccionar(
                "SELECT * FROM EMPRESA WHERE CLAVEELIMINACION = @clave",
                ("clave", CLAVE));
        }
        public DataSet ConsultarClaveTotal(string CLAVE)
        {
            return _conexion.Seleccionar(
                "SELECT * FROM EMPRESA WHERE CLAVETOTALES = @clave",
                ("clave", CLAVE));
        }
        public DataSet ConsultarClaveConsultaFecha(string CLAVE)
        {
            return _conexion.Seleccionar(
                "SELECT * FROM EMPRESA WHERE CLAVECONSULTA = @clave",
                ("clave", CLAVE));
        }
        public DataSet ConsultarClaveTabla(string CLAVE)
        {
            return _conexion.Seleccionar(
                "SELECT * FROM EMPRESA WHERE CLAVETABLAS = @clave",
                ("clave", CLAVE));
        }
        public string ObtenerIP()
        {
            string ip = "";

            foreach (var host in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (host.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = host.ToString();
                    break;
                }
            }

            // Si por alguna razón no encuentra IPv4
            if (string.IsNullOrWhiteSpace(ip))
                ip = "0.0.0.0";

            return ip;
        }

    }
}
