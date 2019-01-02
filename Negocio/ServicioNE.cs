using Comun;
using Modelo;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Negocio
{
    public class ServicioNE
    {
        private Bitacora _bitacora = null;
        private ConexionSapNE _conexionSapNE = null;

        public ServicioNE()
        {
            _bitacora = _bitacora ?? new Bitacora();
            _conexionSapNE = _conexionSapNE ?? new ConexionSapNE();
        }

        public async Task<RespuestaMO> EnviarEstadoProcesoHostToHostAsync(CancellationToken cancelToken, String idSociedad, String anio, String momentoOrden, String idEstadoOrden, String idSap, String usuario)
        {
            RespuestaMO respuestaMO = null;
            try
            {
                respuestaMO = await _conexionSapNE.EnviarEstadoProcesoHostToHostAsync(cancelToken, idSociedad, anio, momentoOrden, idEstadoOrden, idSap, usuario);
                String mensaje = respuestaMO.IdRespuesta == Constante.TYPE_SUCCESS ? Constante.MENSAJE_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC_OK : Constante.MENSAJE_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC_NO_OK;
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_SERVICIO_NE, Constante.METODO_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC, mensaje);
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_NEGOCIO, Constante.CLASE_SERVICIO_NE, Constante.METODO_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC, Constante.MENSAJE_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC_NO_OK, e.Message);
                throw e;
            }
            return respuestaMO;
        }
    }
}
