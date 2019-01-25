using Comun;
using Modelo;
using SAP.Middleware.Connector;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Negocio
{
    public class ConexionSapNE
    {
        private Bitacora _bitacora;
        private static ConexionSap _conexionSap = null;

        public ConexionSapNE()
        {
            _bitacora = _bitacora ?? new Bitacora();
            _conexionSap = _conexionSap ?? new ConexionSap();
        }

        public async Task<RespuestaMO> EnviarEstadoProcesoHostToHostAsync(CancellationToken cancelToken, String idSociedad, String anio, String momentoOrden, String idEstadoOrden, String idSap, String usuario)
        {
            RespuestaMO respuestaMO = null;
            try
            {
                DateTime? fecha = ConvertirCadenaHaciaFechaAsync(momentoOrden);
                RfcDestinationManager.RegisterDestinationConfiguration(_conexionSap);
                RfcConfigParameters rfcConfigParameters = GetParametersAsync();
                RfcDestination rfcDestination = RfcDestinationManager.GetDestination(rfcConfigParameters[RfcConfigParameters.Name]);
                RfcRepository rfcRepository = rfcDestination.Repository;
                IRfcFunction rfcFunction = rfcRepository.CreateFunction(Constante.FUNCTION_YFIRFC_ACTSTS_H2H);
                rfcFunction.SetValue(Constante.IP_BUKRS, idSociedad);
                rfcFunction.SetValue(Constante.IP_GJAHR, anio);
                rfcFunction.SetValue(Constante.IP_LAUFD, fecha);
                rfcFunction.SetValue(Constante.IP_BSTAT, idEstadoOrden);
                rfcFunction.SetValue(Constante.IP_REF1, idSap);
                rfcFunction.SetValue(Constante.IP_USNAM, usuario);
                rfcFunction.Invoke(rfcDestination);
                IRfcStructure rfcStructureReturn = rfcFunction.GetStructure(Constante.EW_MENSG);
                respuestaMO = MapearEstructuraHaciaModeloAsync(rfcStructureReturn);
                String mensaje = respuestaMO.IdRespuesta == Constante.TYPE_SUCCESS ? Constante.MENSAJE_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC_OK : Constante.MENSAJE_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC_NO_OK;
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_CONEXION_SAP_NE, Constante.METODO_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC, mensaje);
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_NEGOCIO, Constante.CLASE_CONEXION_SAP_NE, Constante.METODO_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC, Constante.MENSAJE_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC_NO_OK, e.Message);
                throw e;
            }
            finally
            {
                RfcDestinationManager.UnregisterDestinationConfiguration(_conexionSap);
            }
            return respuestaMO;
        }

        public RfcConfigParameters GetParametersAsync()
        {
            RfcConfigParameters rfcConfigParameters = new RfcConfigParameters();
            try
            {
                String _sapName = ConfigurationManager.AppSettings[Constante.SAP_NAME] ?? String.Empty;
                String _sapAppServerHost = ConfigurationManager.AppSettings[Constante.SAP_APP_SERVER_HOST] ?? String.Empty;
                String _sapSystemNum = ConfigurationManager.AppSettings[Constante.SAP_SYSTEM_NUM] ?? String.Empty;
                String _sapSystemId = ConfigurationManager.AppSettings[Constante.SAP_SYSTEM_ID] ?? String.Empty;
                String _sapUserName = ConfigurationManager.AppSettings[Constante.SAP_USERNAME] ?? String.Empty;
                String _sapPassword = ConfigurationManager.AppSettings[Constante.SAP_PASSWORD] ?? String.Empty;
                String _sapClient = ConfigurationManager.AppSettings[Constante.SAP_CLIENT] ?? String.Empty;
                String _sapLanguage = ConfigurationManager.AppSettings[Constante.SAP_LANGUAGE] ?? String.Empty;
                String _sapPoolSize = ConfigurationManager.AppSettings[Constante.SAP_POOL_SIZE] ?? String.Empty;
                rfcConfigParameters.Add(RfcConfigParameters.Name, _sapName);
                rfcConfigParameters.Add(RfcConfigParameters.AppServerHost, _sapAppServerHost);
                rfcConfigParameters.Add(RfcConfigParameters.SystemNumber, _sapSystemNum);
                rfcConfigParameters.Add(RfcConfigParameters.SystemID, _sapSystemId);
                rfcConfigParameters.Add(RfcConfigParameters.User, _sapUserName);
                rfcConfigParameters.Add(RfcConfigParameters.Password, _sapPassword);
                rfcConfigParameters.Add(RfcConfigParameters.Client, _sapClient);
                rfcConfigParameters.Add(RfcConfigParameters.Language, _sapLanguage);
                rfcConfigParameters.Add(RfcConfigParameters.PoolSize, _sapPoolSize);
                _conexionSap.AddOrEditDestination(rfcConfigParameters);
            }
            catch (Exception e)
            {
                throw e;
            }
            return rfcConfigParameters;
        }

        private RespuestaMO MapearEstructuraHaciaModeloAsync(IRfcStructure rfcStructureReturn)
        {
            RespuestaMO respuestaMO = respuestaMO = new RespuestaMO();
            try
            {
                String tipo = rfcStructureReturn.GetString(Constante.TIPO) ?? String.Empty;
                String mensaje = rfcStructureReturn.GetString(Constante.MENSAJE) ?? String.Empty;
                if (tipo != String.Empty)
                {
                    respuestaMO.IdRespuesta = tipo;
                    respuestaMO.Respuesta = mensaje;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return respuestaMO;
        }

        private DateTime? ConvertirCadenaHaciaFechaAsync(String momentoOrden)
        {
            DateTime? fecha = null;
            try
            {
                String anio = momentoOrden.Substring(0, 4);
                String mes = momentoOrden.Substring(4, 2);
                String dia = momentoOrden.Substring(6, 2);
                Int32 year = Convert.ToInt32(anio);
                Int32 month = Convert.ToInt32(mes);
                Int32 day = Convert.ToInt32(dia);
                if (month >= 1 && month <= 12 && day >= 1 && day <= 31)
                {
                    fecha = new DateTime(year, month, day);
                }
                else
                {
                    fecha = new DateTime();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return fecha;
        }
    }
}
