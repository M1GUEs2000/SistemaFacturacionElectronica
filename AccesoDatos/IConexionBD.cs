using System.Data;

namespace AccesoDatos.Abstractions
{
    public interface IConexionBD
    {
        bool EsSqlServer { get; }
        DataSet Seleccionar(string sql);
        DataSet Seleccionar(string sql, params (string nombre, object valor)[] parametros);
        int Ejecutar(string sql);
        int Ejecutar(string sql, params (string nombre, object valor)[] parametros);
    }
}