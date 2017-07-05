using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SonosStream.Service
{
    class Program
    {
        private const string StreamPath = "/stream.mp3";
        private const string LogoPath = "/stream.png";
        static void Main()
        {
            using (var audioListener = new AudioListener())
            {
                var sonosListener = CreateSonosListener(
                    new Dictionary<string, ISonosRequestHandler>
                    {
                        ["/"] = CreateSoapHandler(),
                        [LogoPath] = new LogoRequestHandler(),
                        [StreamPath] = new AudioStreamRequestHandler(audioListener)
                    });

                using (sonosListener)
                {
                    var httpTask = Task.Run(sonosListener.StartAsync);
                    while (!Console.KeyAvailable)
                    {
                        Thread.Sleep(100);
                    }

                    sonosListener.Stop();
                    httpTask.Wait();
                }
            }
        }

        private static SonosListener CreateSonosListener(Dictionary<string, ISonosRequestHandler> handlers)
        {
            var sonosListener = new SonosListener
            {
                OnException = Console.WriteLine
            };

            foreach (var handler in handlers)
            {
                sonosListener.AddHandler(handler.Key, handler.Value);
            }

            return sonosListener;
        }

        private static SoapRequestHandler CreateSoapHandler()
        {
            return new SoapRequestHandler()
                .AddHandler(GetLastUpdateRequestHandler.SoapAction, new GetLastUpdateRequestHandler())
                .AddHandler(GetMetadataRequestHandler.SoapAction, new GetMetadataRequestHandler(LogoPath))
                .AddHandler(GetMediaMetadataRequestHandler.SoapAction, new GetMediaMetadataRequestHandler(LogoPath))
                .AddHandler(GetMediaUriRequestHandler.SoapAction, new GetMediaUriRequestHandler(StreamPath));
        }
    }
}
