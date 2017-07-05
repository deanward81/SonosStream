using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CSCore.MediaFoundation;
using CSCore.SoundIn;

namespace SonosStream.Service
{
    // streams audio from loopback capture out over the response
    // as an MP3 encoded stream
    class AudioStreamRequestHandler : ISonosRequestHandler
    {
        private readonly AudioListener _audioListener;

        public AudioStreamRequestHandler(AudioListener audioListener)
        {
            _audioListener = audioListener;
        }

        async Task ISonosRequestHandler.HandleRequest(HttpListenerContext ctx)
        {
            // stream out the audio content to Sonos
            ctx.Response.ContentType = "audio/mp3";
            await ctx.Response.OutputStream.WriteAsync(new byte[1] { 0 }, 0, 1);
            var cancellationToken = new CancellationTokenSource();
            var memoryOffset = 0;
            using (var memoryStream = new MemoryStream())
            {
                using (var audioEncoder = MediaFoundationEncoder.CreateMP3Encoder(_audioListener.WaveFormat, memoryStream))
                {
                    async void OnAudioAvailable(object sender, DataAvailableEventArgs e)
                    {
                        audioEncoder.Write(e.Data, e.Offset, e.ByteCount);
                        try
                        {
                            if (memoryStream.TryGetBuffer(out ArraySegment<byte> buffer))
                            {
                                var newOffset = (int)memoryStream.Position;
                                var length = (int)newOffset - memoryOffset;

                                await ctx.Response.OutputStream.WriteAsync(buffer.Array, (int)memoryOffset, length);

                                memoryOffset = newOffset;
                            }

                            await ctx.Response.OutputStream.FlushAsync();
                        }
                        catch
                        {
                            cancellationToken.Cancel();
                        }
                    }

                    try
                    {
                        _audioListener.AudioAvailable += OnAudioAvailable;
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(1000);
                        }
                    }
                    finally
                    {
                        _audioListener.AudioAvailable -= OnAudioAvailable;
                    }
                }
            }
        }
    }
}
