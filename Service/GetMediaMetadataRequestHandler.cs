using System;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SonosStream.Service
{
    // handles SMAPI getMediaMetadata SOAP requests
    class GetMediaMetadataRequestHandler : ISoapRequestHandler
    {
        public const string SoapAction = "\"http://www.sonos.com/Services/1.1#getMediaMetadata\"";

        private static readonly string _responseXml = @"
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soap:Body>
      <getMediaMetadataResponse xmlns=""http://www.sonos.com/Services/1.1"">
         <getMediaMetadataResult>
            <id>1</id>
            <title>Streaming from {machine}</title>
            <mimeType>audio/mp3</mimeType>
            <itemType>stream</itemType>
            <streamMetadata>
                <logo>http://{host}{path}</logo>
                <currentShow>{machine}</currentShow>
                <secondsRemaining>86400</secondsRemaining>
            </streamMetadata>
         </getMediaMetadataResult>
      </getMediaMetadataResponse>
   </soap:Body>
</soap:Envelope>";

        private readonly string _logoPath;

        public GetMediaMetadataRequestHandler(string logoPath)
        {
            _logoPath = logoPath;
        }

        public Task<XDocument> HandleRequest(HttpListenerContext ctx, XDocument requestXml)
        {
            var responseXml = _responseXml
                .Replace("{machine}", Environment.MachineName)
                .Replace("{host}", ctx.Request.UserHostAddress)
                .Replace("{path}", _logoPath);

            return Task.FromResult(XDocument.Parse(responseXml));
        }
    }
}
