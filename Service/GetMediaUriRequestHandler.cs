using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SonosStream.Service
{
    // handles SMAPI getMediaURI SOAP requests
    class GetMediaUriRequestHandler : ISoapRequestHandler
    {
        public const string SoapAction = "\"http://www.sonos.com/Services/1.1#getMediaURI\"";

        private static readonly string _responseXml = @"
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soap:Body>
      <getMediaURIResponse xmlns=""http://www.sonos.com/Services/1.1"">
         <getMediaURIResult>http://{host}{path}</getMediaURIResult>
      </getMediaURIResponse>
   </soap:Body>
</soap:Envelope>";

        private readonly string _path;

        public GetMediaUriRequestHandler(string path)
        {
            _path = path;
        }

        public Task<XDocument> HandleRequest(HttpListenerContext ctx, XDocument requestXml)
        {
            var responseXml = _responseXml
                .Replace("{host}", ctx.Request.UserHostAddress)
                .Replace("{path}", _path);

            return Task.FromResult(XDocument.Parse(responseXml));
        }
    }
}
