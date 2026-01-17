using System.Data;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml;
using System.Data.SqlClient;

namespace API.Shared
{
    public class DBXmlMethods
    {
        public static XDocument GetXml<T>(T criterio)
        {
            XDocument resultado = new XDocument(new XDeclaration("1.0", "utf-8", "true"));
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                using XmlWriter xw = resultado.CreateWriter();
                xs.Serialize(xw, criterio);
                return resultado;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<DataSet> EjecutaBase(string nombreProcedimiento, string cadenaConexion, string proceso, string dataXML)
        {
            DataSet dsResultado = new DataSet();

            if (string.IsNullOrEmpty(cadenaConexion) || string.IsNullOrEmpty(proceso))
                return dsResultado;

            using (SqlConnection cnn = new SqlConnection(cadenaConexion))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = nombreProcedimiento;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = cnn;
                        cmd.CommandTimeout = 120;
                        cmd.Parameters.Add("@iTransaccion", SqlDbType.VarChar).Value = proceso;
                        cmd.Parameters.Add("@iXml", SqlDbType.Xml).Value = string.IsNullOrEmpty(dataXML) ? DBNull.Value : (object)dataXML;

                        await cnn.OpenAsync().ConfigureAwait(false);

                        using (SqlDataAdapter adt = new SqlDataAdapter(cmd))
                        {
                            adt.Fill(dsResultado);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en EjecutaBase: {ex.Message}");
                }
            }
            return dsResultado;
        }
    }
}
