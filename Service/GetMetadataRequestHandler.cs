using System;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SonosStream.Service
{
    // handles SMAPI getMetadata SOAP requests
    class GetMetadataRequestHandler : ISoapRequestHandler
    {
        public const string SoapAction = "\"http://www.sonos.com/Services/1.1#getMetadata\"";

        private static readonly string _responseXml = @"
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <getMetadataResponse xmlns=""http://www.sonos.com/Services/1.1"">
      <getMetadataResult>
        <index>0</index>
        <count>1</count>
        <total>1</total>
        <mediaMetadata>
          <id>1</id>
          <title>Streaming from {machine}</title>
          <mimeType>audio/mp3</mimeType>
          <itemType>stream</itemType>
          <streamMetadata>
            <logo>http://{host}{path}</logo>
            <currentShow>{machine}</currentShow>
          </streamMetadata>
        </mediaMetadata>
      </getMetadataResult>
    </getMetadataResponse>
  </soap:Body>
</soap:Envelope>";

        private readonly string _logoPath;

        public GetMetadataRequestHandler(string logoPath)
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
