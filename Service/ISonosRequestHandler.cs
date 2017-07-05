using System.Net;
using System.Threading.Tasks;

namespace SonosStream.Service
{
    interface ISonosRequestHandler
    {
        Task HandleRequest(HttpListenerContext ctx);
    }
}
