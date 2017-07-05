using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SonosStream.Service
{
    // handles SMAPI getLastUpdate SOAP requests
    class GetLastUpdateRequestHandler : ISoapRequestHandler
    {
        public const string SoapAction = "\"http://www.sonos.com/Services/1.1#getLastUpdate\"";

        private static readonly XDocument _responseXml = XDocument.Parse(@"
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <getLastUpdateResponse xmlns=""http://www.sonos.com/Services/1.1"">
      <getLastUpdateResult>
        <pollInterval>86400</pollInterval>
      </getLastUpdateResult>
    </getLastUpdateResponse>
  </soap:Body>
</soap:Envelope>");

        public Task<XDocument> HandleRequest(HttpListenerContext ctx, XDocument requestXml)
        {
            return Task.FromResult(_responseXml);
        }
    }
}
