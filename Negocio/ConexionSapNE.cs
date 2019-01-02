using Comun;
using Modelo;
using SAP.Middleware.Connector;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Negocio
{
    public class ConexionSapNE
    {
        private Bitacora _bitacora;
        private ConexionSap _conexionSap = null;

        public ConexionSapNE()
        {
            _bitacora = _bitacora ?? new Bitacora();
            _conexionSap = _conexionSap ?? new ConexionSap();
            String sapName = HttpContext.Current.Application[Constante.SAP_NAME] == null ? String.Empty : HttpContext.Current.Application[Constante.SAP_NAME].ToString();
            
            if (sapName == String.Empty)
            {
                var _sapName = _conexionSap.ConnectToServer(new CancellationToken(false), sapName);
                HttpContext.Current.Application[Constante.SAP_NAME] = _sapName;
            }
        }

        public async Task<RespuestaMO> EnviarEstadoProcesoHostToHostAsync(CancellationToken cancelToken, String idSociedad, String anio, String momentoOrden, String idEstadoOrden, String idSap, String usuario)
        {
            RespuestaMO respuestaMO = null;
            try
            {
                DateTime? fecha = ConvertirCadenaAFecha(momentoOrden);
                String sapName = ConfigurationManager.AppSettings[Constante.SAP_NAME] ?? String.Empty;
                RfcDestination rfcDestination = await _conexionSap.TestConnection(cancelToken, sapName);
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
                respuestaMO = await MapearEstructuraAModelo(cancelToken, rfcStructureReturn);
                String mensaje = respuestaMO.IdRespuesta == Constante.TYPE_SUCCESS ? Constante.MENSAJE_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC_OK : Constante.MENSAJE_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC_NO_OK;
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_CONEXION_SAP_NE, Constante.METODO_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC, mensaje);
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_NEGOCIO, Constante.CLASE_CONEXION_SAP_NE, Constante.METODO_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC, Constante.MENSAJE_ENVIAR_ESTADO_PROCESO_HOSTTOHOST_ASYNC_NO_OK, e.Message);
                throw e;
            }
            return respuestaMO;
        }

        private async Task<RespuestaMO> MapearEstructuraAModelo(CancellationToken cancelToken, IRfcStructure rfcStructureReturn)
        {
            RespuestaMO respuestaMO = null;
            try
            {
                String tipo = rfcStructureReturn.GetString(Constante.TIPO);
                String mensaje = rfcStructureReturn.GetString(Constante.MENSAJE);
                if (tipo != String.Empty)
                {
                    respuestaMO = new RespuestaMO();
                    respuestaMO.IdRespuesta = tipo;
                    respuestaMO.Respuesta = mensaje;
                }
                
                mensaje = tipo != String.Empty ? Constante.MENSAJE_MAPEAR_ESTRUCTURA_A_MODELO_ASYNC_OK : Constante.MENSAJE_MAPEAR_ESTRUCTURA_A_MODELO_ASYNC_NO_OK;
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_CONEXION_SAP_NE, Constante.METODO_MAPEAR_ESTRUCTURA_A_MODELO, mensaje);
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_NEGOCIO, Constante.CLASE_CONEXION_SAP_NE, Constante.METODO_MAPEAR_ESTRUCTURA_A_MODELO, Constante.MENSAJE_MAPEAR_ESTRUCTURA_A_MODELO_ASYNC_NO_OK, e.Message);
                throw e;
            }
            return respuestaMO;
        }

        private DateTime? ConvertirCadenaAFecha(String momentoOrden)
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
            }
            catch (Exception e)
            {
                throw e;
            }
            return fecha;
        }
    }

    public class ConexionSap : IDestinationConfiguration
    {
        private Bitacora _bitacora;
        private String _sapName = String.Empty, _sapAppServerHost = String.Empty, _sapSystemNum = String.Empty, _sapSystemId = String.Empty, _sapUserName = String.Empty, _sapPassword = String.Empty, _sapClient = String.Empty, _sapLanguage = String.Empty, _sapPoolSize = String.Empty;
        
        public ConexionSap()
        {
            _sapName = ConfigurationManager.AppSettings[Constante.SAP_NAME] ?? String.Empty;
            _sapAppServerHost = ConfigurationManager.AppSettings[Constante.SAP_APP_SERVER_HOST] ?? String.Empty;
            _sapSystemNum = ConfigurationManager.AppSettings[Constante.SAP_SYSTEM_NUM] ?? String.Empty;
            _sapSystemId = ConfigurationManager.AppSettings[Constante.SAP_SYSTEM_ID] ?? String.Empty;
            _sapUserName = ConfigurationManager.AppSettings[Constante.SAP_USERNAME] ?? String.Empty;
            _sapPassword = ConfigurationManager.AppSettings[Constante.SAP_PASSWORD] ?? String.Empty;
            _sapClient = ConfigurationManager.AppSettings[Constante.SAP_CLIENT] ?? String.Empty;
            _sapLanguage = ConfigurationManager.AppSettings[Constante.SAP_LANGUAGE] ?? String.Empty;
            _sapPoolSize = ConfigurationManager.AppSettings[Constante.SAP_POOL_SIZE] ?? String.Empty;
            _bitacora = _bitacora ?? new Bitacora();
        }

        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

        public bool ChangeEventsSupported()
        {
            return false;
        }

        public RfcConfigParameters GetParameters(string destinationName)
        {
            RfcConfigParameters rfcConfigParameters = null;
            try
            {
                rfcConfigParameters = new RfcConfigParameters();
                rfcConfigParameters.Add(RfcConfigParameters.Name, _sapName);
                rfcConfigParameters.Add(RfcConfigParameters.AppServerHost, _sapAppServerHost);
                rfcConfigParameters.Add(RfcConfigParameters.SystemNumber, _sapSystemNum);
                rfcConfigParameters.Add(RfcConfigParameters.SystemID, _sapSystemId);
                rfcConfigParameters.Add(RfcConfigParameters.User, _sapUserName);
                rfcConfigParameters.Add(RfcConfigParameters.Password, _sapPassword);
                rfcConfigParameters.Add(RfcConfigParameters.Client, _sapClient);
                rfcConfigParameters.Add(RfcConfigParameters.Language, _sapLanguage);
                rfcConfigParameters.Add(RfcConfigParameters.PoolSize, _sapPoolSize);
            }
            catch (Exception e)
            {
                throw e;
            }
            return rfcConfigParameters;
        }

        public async Task<String> ConnectToServer(CancellationToken cancelToken, String destinationName)
        {
            String sapName = String.Empty;
            try
            {
                if (destinationName == String.Empty)
                {
                    IDestinationConfiguration _destinationConfiguration = new ConexionSap();
                    _destinationConfiguration.GetParameters(String.Empty);
                    RfcDestinationManager.RegisterDestinationConfiguration(_destinationConfiguration);
                    sapName = _sapName;
                }

                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_CONEXION_SAP, Constante.METODO_CONNECT_TO_SERVER, Constante.MENSAJE_CONNECT_TO_SERVER_OK);
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_NEGOCIO, Constante.CLASE_CONEXION_SAP, Constante.METODO_CONNECT_TO_SERVER, Constante.MENSAJE_CONNECT_TO_SERVER_NO_OK, e.Message);
                throw e;
            }
            return sapName;
        }

        public async Task<RfcDestination> TestConnection(CancellationToken cancelToken, String destinationName)
        {
            RfcDestination rfcDestination = null;
            try
            {
                rfcDestination = RfcDestinationManager.GetDestination(destinationName);

                if (rfcDestination == null)
                {
                    await ConnectToServer(cancelToken, String.Empty);
                    rfcDestination = RfcDestinationManager.GetDestination(destinationName);
                }

                rfcDestination.Ping();
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_CONEXION_SAP, Constante.METODO_TEST_CONNECTION, Constante.MENSAJE_TEST_CONEXION_SAP_OK);
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_NEGOCIO, Constante.CLASE_CONEXION_SAP, Constante.METODO_TEST_CONNECTION, Constante.MENSAJE_TEST_CONEXION_SAP_NO_OK, e.Message);
                throw e;
            }
            return rfcDestination;
        }
    }
}
