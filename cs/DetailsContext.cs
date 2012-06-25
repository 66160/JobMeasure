using System;
using System.ServiceModel.DomainServices.Client;

namespace DetailsServiceLib
{
    public partial class DetailsContext
    {
        partial void OnCreated()
        {
            ((WebDomainClient<DetailsContext.IDetailsServiceContract>)this.DomainClient).ChannelFactory.Endpoint.Binding.SendTimeout = TimeSpan.MaxValue;
        }
    }
}
