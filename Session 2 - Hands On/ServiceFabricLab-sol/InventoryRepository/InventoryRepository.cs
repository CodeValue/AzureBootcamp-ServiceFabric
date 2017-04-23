using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric.Description;
using InventoryRepository.ServiceFabric;

namespace InventoryRepository
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class InventoryRepository : StatefulService
    {
        public InventoryRepository(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            var endpoints = Context.CodePackageActivationContext.GetEndpoints()
           .Where(endpoint => endpoint.Protocol == EndpointProtocol.Http || endpoint.Protocol == EndpointProtocol.Https)
           .Select(endpoint => endpoint.Name);

            return endpoints.Select(
                endpoint => new ServiceReplicaListener(
                    serviceContext => new OwinCommunicationListener(
                        appBuilder => { WebHostStartup.ConfigureApp(appBuilder, StateManager); },
                        serviceContext,
                        ServiceEventSource.Current,
                        endpoint),
                    endpoint));
        }

       
    }
}
