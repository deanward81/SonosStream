using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SonosStream.Service
{
    // handles SMAPI SOAP requests by dispatching them to child handlers
    class SoapRequestHandler : ISonosRequestHandler
    {
        private readonly Dictionary<string, ISoapRequestHandler> _handlers = new Dictionary<string, ISoapRequestHandler>(StringComparer.InvariantCultureIgnoreCase);

        async Task ISonosRequestHandler.HandleRequest(HttpListenerContext ctx)
        {
            var contentType = ctx.Request.Headers["Content-Type"];
            if (!contentType.StartsWith("text/xml"))
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                ctx.Response.StatusDescription = "Bad Request";
                return;
            }

            var soapAction = ctx.Request.Headers["SOAPAction"];
            Console.WriteLine($"SOAP Action = {soapAction}");
            if (!_handlers.TryGetValue(soapAction, out ISoapRequestHandler handler))
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                ctx.Response.StatusDescription = "Not Found";
                return;
            }

            var requestXml = XDocument.Load(ctx.Request.InputStream);
            Console.WriteLine($"Request Content = {requestXml}");
            var responseXml = await handler.HandleRequest(ctx, requestXml);
            Console.WriteLine($"Response Content = {responseXml}");
            ctx.Response.ContentEncoding = Encoding.UTF8;
            ctx.Response.ContentType = "text/xml";
            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            using (var xmlWriter = XmlWriter.Create(ctx.Response.OutputStream, new XmlWriterSettings { Encoding = Encoding.UTF8 }))
            {
                responseXml.WriteTo(xmlWriter);
            }
        }

        public SoapRequestHandler AddHandler(string action, ISoapRequestHandler handler)
        {
            if (_handlers.ContainsKey(action))
            {
                throw new ArgumentException($"SOAP action {action} already has a handler");
            }

            _handlers.Add(action, handler);
            return this;
        }
    }
}
