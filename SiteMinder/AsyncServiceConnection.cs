using System;
using System.ServiceModel;
using pmsXchange.pmsXchangeService;

namespace pmsXchange
{
    //
    // The async service connection implemeted as a class, creates a new connection very time it's instantiated. Use only for asynchronous calls.
    //

    public sealed class AsyncServiceConnection
    {
        private const string endpointURI = "https://cmtpi.siteminder.com/pmsxchangev2/services/SPIORANGE";  // Provided by SiteMinder.
        public PmsXchangeServiceClient service { get; private set; }

        public AsyncServiceConnection()
        {
            InitializeService();
        }

        public void InitializeService()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.Transport;

            EndpointAddress address = new EndpointAddress(endpointURI);
            service = new PmsXchangeServiceClient(binding, address);
        }
    }
}
