using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SonosStream.Service
{
    // we really don't care much about the intricacies
    // of the SMAPI SOAP requests; we only care about
    // a select few endpoints and we can return canned XML responses
    // for all of them. We can get away with this
    // because we just support one live stream!
    interface ISoapRequestHandler
    {
        Task<XDocument> HandleRequest(HttpListenerContext ctx, XDocument requestXml);
    }
}
