using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SonosStream.Service
{
    class LogoRequestHandler : ISonosRequestHandler
    {
        private readonly byte[] _imageData;

        public LogoRequestHandler()
        {
            var namespaceType = typeof(LogoRequestHandler);
            using (var resourceStream = namespaceType.Assembly.GetManifestResourceStream(namespaceType, "logo.png"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    resourceStream.CopyTo(memoryStream);
                    _imageData = memoryStream.ToArray();
                }
            }
        }

        public async Task HandleRequest(HttpListenerContext ctx)
        {
            ctx.Response.ContentType = "image/png";
            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            await ctx.Response.OutputStream.WriteAsync(_imageData, 0, _imageData.Length);
        }
    }
}
