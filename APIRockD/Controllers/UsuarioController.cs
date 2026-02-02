using BDRockDeveloper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

using System.Data;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace APIRockD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _config;
        public UsuarioController(IConfiguration config)
        {
            _config = config;
        }

        [Route("[action]")]
        [HttpPost]
        /*El método hace esto en el swagger:
         1. CONSULTAR TODOS LOS USUARIOS

        {
            "transaccion": "CONSULTAR_USUARIO"
        }
        2. VALIDAR USUARIO (Login)

        {
            "cedula": "0911222334",
            "clave": "123456",
            "transaccion": "VALIDAR_USUARIO"
        }
         */
         
        public async Task<ActionResult> GetUsuario([FromBody] Usuario usuario)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("Conexion");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest("Cadena de conexión no configurada");

                XDocument xmlParam = API.Shared.DBXmlMethods.GetXml(usuario);
                DataSet dsResultado = await API.Shared.DBXmlMethods.EjecutaBase(
                    "GetUsuario",
                    cadenaConexion,
                    usuario.Transaccion,
                    xmlParam?.ToString() ?? "");

                if (dsResultado.Tables.Count > 0)
                {
                    List<Usuario> listUsuario = new List<Usuario>();
                    foreach (DataRow row in dsResultado.Tables[0].Rows)
                    {
                        Usuario objResponse = new Usuario
                        {
                            Id = Convert.ToInt32(row["id"]),
                            Cedula = row["cedula"].ToString(),
                            Nombres = row["nombres"].ToString(),
                            Apellidos = row["apellidos"].ToString(),
                            Transaccion = ""
                        };
                        listUsuario.Add(objResponse);
                    }
                    return Ok(listUsuario);
                }
                return Ok(new List<Usuario>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
        /*El método hace esto en el swagger:
         1. INSERTAR NUEVO USUARIO
        {
          "cedula": "0911222335",
          "nombres": "María",
          "apellidos": "González",
          "clave": "password123",
          "transaccion": "INSERTAR_USUARIO"
        }
        2. ACTUALIZAR CLAVE DE USUARIO

        {
          "cedula": "0911222334",
          "clave": "nuevaClave456",
          "transaccion": "ACTUALIZAR_CLAVE"
        }
        3. ELIMINAR USUARIO

        {
          "cedula": "0911222335",
          "transaccion": "ELIMINAR_USUARIO"
        }
         */
         [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult> PostUsuario([FromBody] Usuario usuario)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("Conexion");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest("Cadena de conexión no configurada");

                XDocument xmlParam = API.Shared.DBXmlMethods.GetXml(usuario);
                DataSet dsResultado = await API.Shared.DBXmlMethods.EjecutaBase(
                    "SetUsuario",
                    cadenaConexion,
                    usuario.Transaccion,
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
         1. VALIDAR USUARIO (Login)

        {
            "cedula": "0911222334",
            "clave": "123456",
            "transaccion": "VALIDAR_USUARIO"
        }
         */
        [Route("[action]")]
        [HttpPost]
       
        public async Task<ActionResult> ValidarAcceso([FromBody] Usuario usuario)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("Conexion");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest("Cadena de conexión no configurada");

                XDocument xmlParam = API.Shared.DBXmlMethods.GetXml(usuario);
                DataSet dsResultado = await API.Shared.DBXmlMethods.EjecutaBase(
                    "GetUsuario",
                    cadenaConexion,
                    "VALIDAR_USUARIO",
                    xmlParam?.ToString() ?? "");

                if (dsResultado.Tables.Count > 0 && dsResultado.Tables[0].Rows.Count > 0)
                {
                    Usuario usuario1 = new Usuario
                    {
                        Id = Convert.ToInt32(dsResultado.Tables[0].Rows[0]["id"]),
                        Cedula = dsResultado.Tables[0].Rows[0]["cedula"].ToString(),
                        Nombres = dsResultado.Tables[0].Rows[0]["nombres"].ToString(),
                        Apellidos = dsResultado.Tables[0].Rows[0]["apellidos"].ToString()
                    };


                    return Ok(usuario1);
                }
                else
                {
                    return Unauthorized("Credenciales incorrectas");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
        /*El método hace esto en el swagger:
         1. INSERTAR NUEVO USUARIO
        {
          "cedula": "0911222335",
          "nombres": "María",
          "apellidos": "González",
          "clave": "password123",
          "transaccion": "INSERTAR_USUARIO"
        }
        2. ACTUALIZAR CLAVE DE USUARIO

        {
          "cedula": "0911222334",
          "clave": "nuevaClave456",
          "transaccion": "ACTUALIZAR_CLAVE"
        }
        3. ELIMINAR USUARIO

        {
          "cedula": "0911222335",
          "transaccion": "ELIMINAR_USUARIO"
        }
         */
         [Route("[action]")]
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> SetUsuario([FromBody] Usuario usuario)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("Conexion");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest("Cadena de conexión no configurada");

                XDocument xmlParam = API.Shared.DBXmlMethods.GetXml(usuario);
                DataSet dsResultado = await API.Shared.DBXmlMethods.EjecutaBase(
                    "SetUsuario",
                    cadenaConexion,
                    usuario.Transaccion,
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
 1. VALIDAR USUARIO (Login)

{
    "cedula": "0911222311",
    "clave": "123456",
    "transaccion": "VALIDAR_USUARIO"
}
 */
        [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult<Usuario>> GetUsuarioAccess([FromBody] Usuario usuario)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("Conexion");


                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest("Cadena de conexión no configurada");

                XDocument xmlParam = API.Shared.DBXmlMethods.GetXml(usuario);
                DataSet dsResultado = await API.Shared.DBXmlMethods.EjecutaBase(
                    "GetUsuario",
                    cadenaConexion,
                    "VALIDAR_USUARIO",
                    xmlParam?.ToString() ?? "");
                List<Usuario> listUsuario= new List<Usuario>();
                if (dsResultado.Tables.Count > 0 && dsResultado.Tables[0].Rows.Count > 0)

                {
                    Usuario usuario1 = new Usuario();
                    usuario1.Id=Convert.ToInt32(dsResultado.Tables[0].Rows[0]["id"]);
                    usuario1.Cedula = dsResultado.Tables[0].Rows[0]["cedula"].ToString();
                    


                    return Ok(JsonConvert.SerializeObject(CrearToken(usuario1)));
                }
                else
                {
                    return Ok(JsonConvert.SerializeObject("Error credenciales"));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
        
        //Sirve para la creación de un acceso según los datos de usuario que ingresa al sistema
        private string CrearToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Cedula)
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = creds,
                Expires = DateTime.Now.AddDays(1),
                Subject = new ClaimsIdentity(claims)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

  [HttpGet("test-db")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("Conexion");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest("Cadena de conexión no configurada");

                // Ejecutamos un procedimiento simple que siempre funciona, como "CONSULTAR_USUARIO"
                DataSet ds = await API.Shared.DBXmlMethods.EjecutaBase(
                    "GetUsuario",
                    cadenaConexion,
                    "CONSULTAR_USUARIO",
                    null
                );

                if (ds.Tables.Count > 0)
                {
                    return Ok("Conexión a SQL Server OK!");
                }
                else
                {
                    return StatusCode(500, "Conexión establecida pero no se pudo consultar la tabla.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error de conexión: " + ex.Message);
            }
        }


    }
    
}