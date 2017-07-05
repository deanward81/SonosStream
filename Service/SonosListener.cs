using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SonosStream.Service
{
    // simple wrapper around an HttpListener that dispatches incoming requests to
    // registered ISonosRequestHandler implementations
    class SonosListener : IDisposable
    {
        private readonly Dictionary<string, ISonosRequestHandler> _handlers = new Dictionary<string, ISonosRequestHandler>(StringComparer.InvariantCultureIgnoreCase);
        private readonly HttpListener _httpListener;
        private readonly SemaphoreSlim _throttle;

        public SonosListener()
        {
            _httpListener = CreateHttpListener();
            _throttle = new SemaphoreSlim(2);
        }

        public Action<Exception> OnException;

        public SonosListener AddHandler(string url, ISonosRequestHandler handler)
        {
            if (_handlers.ContainsKey(url))
            {
                throw new ArgumentException($"Url {url} already has a handler");
            }

            _handlers.Add(url, handler);
            return this;
        }

        public async Task StartAsync()
        {
            _httpListener.Start();
            while (true)
            {
                if (!_httpListener.IsListening)
                {
                    break;
                }

                await _throttle.WaitAsync();

                if (!_httpListener.IsListening)
                {
                    break;
                }

#pragma warning disable CS4014
                // delibrately suppressing warnings because this is intended to run in the background
                _httpListener.GetContextAsync().ContinueWith(OnDispatch);
#pragma warning restore CS4014
            }

            async Task HandleRequest(HttpListenerContext ctx)
            {
                var path = ctx.Request.Url.AbsolutePath;
                Console.WriteLine($"Request for {path}");
                if (!_handlers.TryGetValue(path, out ISonosRequestHandler handler))
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    ctx.Response.StatusDescription = "Not Found";
                    return;
                }

                try
                {
                    await handler.HandleRequest(ctx);
                }
                finally
                {
                    ctx.Response.Close();
                }
            }

            async Task OnDispatch(Task<HttpListenerContext> t)
            {
                _throttle.Release();

                try
                {
                    await HandleRequest(await t);
                }
                catch (HttpListenerException)
                {
                    // ignore these; they're just noise
                }
                catch (Exception ex)
                {
                    OnException?.Invoke(ex);
                }
            }
        }

        public void Stop()
        {
            try
            {
                _httpListener.Stop();
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }
        }

        public void Dispose()
        {
            try
            {
                _httpListener.Close();
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }
        }

        private static HttpListener CreateHttpListener()
        {
            return new HttpListener
            {
                Prefixes = { "http://*:1401/" },
                IgnoreWriteExceptions = true
            };
        }
    }
}
