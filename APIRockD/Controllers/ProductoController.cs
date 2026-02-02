using BDRockDeveloper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Xml.Linq;

namespace APIRockD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductoController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ProductoController(IConfiguration config)
        {
            _config = config;
        }
        /*El método hace esto en el swagger:
         1. CONSULTAR TODOS LOS PRODUCTOS
        
        {
          "transaccion": "CONSULTAR_PRODUCTO"
        }
        2. BUSCAR PRODUCTO POR CÓDIGO
        
        {
          "codigo": "SA100",
          "transaccion": "BUSCAR_PRODUCTO"
        }
         */
        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> GetProducto([FromBody] Producto producto)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("Conexion");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest("Cadena de conexión no configurada");

                XDocument xmlParam = API.Shared.DBXmlMethods.GetXml(producto);
                DataSet dsResultado = await API.Shared.DBXmlMethods.EjecutaBase(
                    "GetProducto",
                    cadenaConexion,
                    producto.Transaccion ?? "CONSULTAR_PRODUCTO",
                    xmlParam?.ToString() ?? "");

                if (dsResultado.Tables.Count > 0)
                {
                    List<Producto> listProducto = new List<Producto>();
                    foreach (DataRow row in dsResultado.Tables[0].Rows)
                    {
                        Producto objResponse = new Producto
                        {
                            Id = Convert.ToInt32(row["id"]),
                            Codigo = row["codigo"].ToString(),
                            Nombre = row["nombre"].ToString(),
                            Precio = Convert.ToDouble(row["precio"]),
                            Proveedor = new Proveedor
                            {
                                Id = Convert.ToInt32(row["proveedor"]),
                                Nombre = row["nombreProveedor"].ToString()
                            }
                        };
                        listProducto.Add(objResponse);
                    }
                    return Ok(listProducto);
                }
                return Ok(new List<Producto>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
        /* El método hace esto en el swagger:
         1. INSERTAR NUEVO PRODUCTO

        {
            "codigo": "SA200",
            "nombre": "Aceite 2lt",
            "precio": 6.50,
            "proveedor": {
            "id": 1
            },
            "transaccion": "INSERTAR_PRODUCTO"
        } 
        2. ACTUALIZAR PRODUCTO
        
        {
            "codigo": "SA100",
            "nombre": "Aceite Premium 1lt",
            "precio": 4.25,
            "proveedor": {
            "id": 1
            },
            "transaccion": "ACTUALIZAR_PRODUCTO"
        }
        3. ELIMINAR PRODUCTO
        
        {
            "codigo": "SA100",
            "transaccion": "ELIMINAR_PRODUCTO"
        }

         */

        [HttpPost]
        public async Task<ActionResult> PostProducto([FromBody] Producto producto)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("Conexion");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest("Cadena de conexión no configurada");

                XDocument xmlParam = API.Shared.DBXmlMethods.GetXml(producto);
                DataSet dsResultado = await API.Shared.DBXmlMethods.EjecutaBase(
                    "SetProducto",
                    cadenaConexion,
                    producto.Transaccion,
                    xmlParam?.ToString() ?? "");

                Resultado resultadoTransaccion = new Resultado();
                if (dsResultado.Tables.Count > 0 && dsResultado.Tables[0].Rows.Count > 0)
                {
                    resultadoTransaccion.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"].ToString();
                    resultadoTransaccion.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"].ToString();
                }

                return Ok(resultadoTransaccion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
        /* El método hace esto en el swagger:
         1. INSERTAR NUEVO PRODUCTO

        {
            "codigo": "SA200",
            "nombre": "Aceite 2lt",
            "precio": 6.50,
            "proveedor": {
            "id": 1
            },
            "transaccion": "INSERTAR_PRODUCTO"
        } 
        2. ACTUALIZAR PRODUCTO
        
        {
            "codigo": "SA100",
            "nombre": "Aceite Premium 1lt",
            "precio": 4.25,
            "proveedor": {
            "id": 1
            },
            "transaccion": "ACTUALIZAR_PRODUCTO"
        }
        3. ELIMINAR PRODUCTO
        
        {
            "codigo": "SA200",
            "transaccion": "ELIMINAR_PRODUCTO"
        }
         
         */
        [Route("[action]")]

        [HttpPost]
        public async Task<ActionResult> SetProducto([FromBody] Producto producto)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("Conexion");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest("Cadena de conexión no configurada");

                XDocument xmlParam = API.Shared.DBXmlMethods.GetXml(producto);
                DataSet dsResultado = await API.Shared.DBXmlMethods.EjecutaBase(
                    "SetProducto",
                    cadenaConexion,
                    producto.Transaccion,
                    xmlParam?.ToString() ?? "");

                Resultado resultadoTransaccion = new Resultado();
                if (dsResultado.Tables.Count > 0 && dsResultado.Tables[0].Rows.Count > 0)
                {
                    resultadoTransaccion.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"].ToString();
                    resultadoTransaccion.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"].ToString();
                }

                return Ok(resultadoTransaccion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}