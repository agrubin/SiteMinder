using System;
using System.ServiceModel;
using pmsXchange.pmsXchangeService;

//
// The service connection implemeted as a singleton so it is only instantiated and initalized one time
// upon the first access, then the same connection is returned on each subsequent access.  Use only for
// synchronous calls.
//

namespace pmsXchange
{
    public sealed class ServiceConnection
    {
        public static ServiceConnection Instance { get { return lazyConnection.Value; } }
        private const string endpointURI = "https://cmtpi.siteminder.com/pmsxchangev2/services/SPIORANGE";  // Provided by SiteMinder.
        private static readonly Lazy<ServiceConnection> lazyConnection = new Lazy<ServiceConnection>(() => new ServiceConnection());
        public PmsXchangeServiceClient service { get; private set; }

        private ServiceConnection()
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
