using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Owin;

namespace InventoryRepository.ServiceFabric
{
    internal class OwinCommunicationListener : ICommunicationListener
    {
        private readonly ServiceEventSource _eventSource;
        private readonly Action<IAppBuilder> _startup;
        private readonly ServiceContext _serviceContext;
        private readonly String _endpointName;
        private readonly String _appRoot;

        private IDisposable _webApp;
        private String _publishAddress;
        private String _listeningAddress;

        public OwinCommunicationListener(Action<IAppBuilder> startup, ServiceContext serviceContext, ServiceEventSource eventSource, string endpointName)
            : this(startup, serviceContext, eventSource, endpointName, null)
        {
        }

        public OwinCommunicationListener(Action<IAppBuilder> startup, ServiceContext serviceContext, ServiceEventSource eventSource, string endpointName, string appRoot)
        {
            if (startup == null) throw new ArgumentNullException(nameof(startup));
            if (serviceContext == null) throw new ArgumentNullException(nameof(serviceContext));
            if (endpointName == null) throw new ArgumentNullException(nameof(endpointName));
            if (eventSource == null) throw new ArgumentNullException(nameof(eventSource));

            _startup = startup;
            _serviceContext = serviceContext;
            _endpointName = endpointName;
            _eventSource = eventSource;
            _appRoot = appRoot;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serviceEndpoint = _serviceContext.CodePackageActivationContext.GetEndpoint(_endpointName);
            var protocol = serviceEndpoint.Protocol;
            int port = serviceEndpoint.Port;

            if (_serviceContext is StatefulServiceContext)
            {
                var statefulServiceContext = _serviceContext as StatefulServiceContext;

                var appRootValue = String.IsNullOrWhiteSpace(_appRoot)
                        ? String.Empty
                        : _appRoot.TrimEnd('/') + '/';
                _listeningAddress = $"{protocol}://+:{port}/{appRootValue}{statefulServiceContext.PartitionId}/{statefulServiceContext.ReplicaId}/{Guid.NewGuid()}";
            }
            else if (_serviceContext is StatelessServiceContext)
            {
                var appRootValue = String.IsNullOrWhiteSpace(_appRoot)
                        ? String.Empty
                        : _appRoot.TrimEnd('/') + '/';
                _listeningAddress = $"{protocol}://+:{port}/{appRootValue}";
            }
            else
            {
                throw new InvalidOperationException();
            }

            _publishAddress = _listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            try
            {
                _eventSource.Message($"Starting web server on {_listeningAddress}");
                _webApp = WebApp.Start(_listeningAddress, appBuilder => _startup.Invoke(appBuilder));
                _eventSource.Message($"Listening on {_publishAddress}");
                return Task.FromResult(_publishAddress);
            }
            catch (Exception ex)
            {
                _eventSource.Message($"Web server failed to open endpoint {_endpointName}. {ex}");
                StopWebServer();
                throw;
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            _eventSource.Message($"Closing web server on endpoint {_endpointName}");
            StopWebServer();
            return Task.FromResult(true);
        }

        public void Abort()
        {
            _eventSource.Message($"Aborting web server on endpoint {_endpointName}");
            StopWebServer();
        }

        private void StopWebServer()
        {
            if (_webApp != null)
            {
                try
                {
                    _webApp.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // no-op
                }
            }
        }
    }

    public static class WebHostStartup
    {
        public static void ConfigureApp(IAppBuilder appBuilder, IReliableStateManager stateManager)
        {
            StateManager = stateManager;

            // Configure Web API for self-host. 
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            appBuilder.UseWebApi(config);
        }

        public static IReliableStateManager StateManager { get; private set; }
    }
}
