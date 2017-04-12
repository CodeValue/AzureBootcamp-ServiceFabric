using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace InventoryService
{
    public class HttpCommunicationClient : ICommunicationClient
    {
        public HttpCommunicationClient(HttpClient client, String address)
        {
            HttpClient = client;
            BaseUri = new Uri(address);
        }

        public HttpClient HttpClient { get; }
        public Uri BaseUri { get; }

        public ResolvedServicePartition ResolvedServicePartition { get; set; }
        public String ListenerName { get; set; }
        public ResolvedServiceEndpoint Endpoint { get; set; }
    }

    public class HttpCommunicationClientFactory : CommunicationClientFactoryBase<HttpCommunicationClient>
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public HttpCommunicationClientFactory(IServicePartitionResolver resolver = null)
            : base(resolver)
        {
        }

        protected override void AbortClient(HttpCommunicationClient client)
        {
            throw new NotImplementedException();
        }

        protected override Task<HttpCommunicationClient> CreateClientAsync(String endpoint, CancellationToken cancellationToken)
        {
            var result = new HttpCommunicationClient(_httpClient, endpoint);
            return Task.FromResult(result);
        }

        protected override bool ValidateClient(HttpCommunicationClient client)
        {
            return true;
        }

        protected override bool ValidateClient(String endpoint, HttpCommunicationClient client)
        {
            return true;
        }
    }
}

