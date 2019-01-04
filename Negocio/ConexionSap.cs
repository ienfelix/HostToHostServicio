using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;

namespace Negocio
{
    public class ConexionSap : IDestinationConfiguration
    {
        private IDictionary<String, RfcConfigParameters> _configurationList;

        public ConexionSap()
        {
            _configurationList = _configurationList ?? new Dictionary<String, RfcConfigParameters>();
        }

        public RfcConfigParameters GetParameters(string destinationName)
        {
            RfcConfigParameters rfcConfigParameters = null;
            try
            {
                _configurationList.TryGetValue(destinationName, out rfcConfigParameters);
            }
            catch (Exception e)
            {
                throw e;
            }
            return rfcConfigParameters;
        }

        public bool ChangeEventsSupported()
        {
            return true;
        }

        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

        public Boolean AddOrEditDestination(RfcConfigParameters rfcConfigParameters)
        {
            Boolean isRegistered = false;
            try
            {
                String name = rfcConfigParameters[RfcConfigParameters.Name];
                if (_configurationList.ContainsKey(name))
                {
                    if (ConfigurationChanged != null)
                    {
                        RfcConfigurationEventArgs eventArgs = new RfcConfigurationEventArgs(RfcConfigParameters.EventType.CHANGED, rfcConfigParameters);
                        ConfigurationChanged(name, eventArgs);
                    }
                }

                _configurationList[name] = rfcConfigParameters;
                isRegistered = true;
            }
            catch (Exception e)
            {
                throw e;
            }
            return isRegistered;
        }

        public void RemoveDestination(String name)
        {
            try
            {
                if (_configurationList.Remove(name))
                {
                    if (ConfigurationChanged != null)
                    {
                        ConfigurationChanged(name, new RfcConfigurationEventArgs(RfcConfigParameters.EventType.DELETED));
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
