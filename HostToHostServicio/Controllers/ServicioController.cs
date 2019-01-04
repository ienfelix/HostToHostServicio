using Comun;
using Modelo;
using Negocio;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace HostToHostServicio.Controllers
{
    public class ServicioController : ApiController
    {
        private ServicioNE _servicioNE = null;
        public String sapName = String.Empty;

        public ServicioController()
        {
            _servicioNE = _servicioNE ?? new ServicioNE();
        }

        [HttpGet]
        public string Get()
        {
            return Constante.MENSAJE_SERVICIO_OK;
        }

        [HttpGet]
        public async Task<RespuestaMO> Get(String idSociedad, String anio, String momentoOrden, String idEstadoOrden, String idSap, String usuario)
        {
            RespuestaMO respuestaMO = null;
            try
            {
                respuestaMO = await _servicioNE.EnviarEstadoProcesoHostToHostAsync(new CancellationToken(false), idSociedad, anio, momentoOrden, idEstadoOrden, idSap, usuario);
            }
            catch (Exception e)
            {
                respuestaMO.Respuesta = e.Message;
            }
            return respuestaMO;
        }
    }
}