using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSCore;
using CSCore.SoundIn;
using CSCore.SoundOut;

namespace SonosStream.Service
{
    // simple wrapper that handles starting/stopping loopback capture
    // based upon event subscriptions
    class AudioListener : IDisposable
    {
        private readonly object _syncLock;
        private readonly WasapiLoopbackCapture _loopbackCapture;
        private readonly CancellationTokenSource _cancellationSource;

        public AudioListener()
        {
            _cancellationSource = new CancellationTokenSource();
            _syncLock = new object();
            _loopbackCapture = new WasapiLoopbackCapture();
            _loopbackCapture.Initialize();
        }

        private int _subscribers;
        private Task _silenceTask;

        public event EventHandler<DataAvailableEventArgs> AudioAvailable
        {
            add
            {
                lock (_syncLock)
                {
                    _loopbackCapture.DataAvailable += value;
                    if (Interlocked.Increment(ref _subscribers) == 1)
                    {
                        _loopbackCapture.Start();
                        _silenceTask = GenerateSilenceAsync(_cancellationSource.Token);
                    }
                }
            }
            remove
            {
                lock (_syncLock)
                {
                    _loopbackCapture.DataAvailable -= value;
                    if (Interlocked.Decrement(ref _subscribers) == 0)
                    {
                        _loopbackCapture.Stop();
                        _cancellationSource.Cancel();
                    }
                }
            }
        }

        private class SilenceSource : IWaveSource
        {
            private readonly MemoryStream _memoryStream;

            public SilenceSource(WaveFormat format)
            {
                WaveFormat = format;

                _memoryStream = new MemoryStream(new byte[format.MillisecondsToBytes(10)], false);
            }

            public bool CanSeek => _memoryStream.CanSeek;

            public WaveFormat WaveFormat { get; }

            public long Position
            {
                get { return _memoryStream.Position; }
                set { _memoryStream.Position = value; }
            }

            public long Length => _memoryStream.Length;

            public void Dispose()
            {
                _memoryStream.Dispose();
            }

            public int Read(byte[] buffer, int offset, int count)
            {
                return _memoryStream.Read(buffer, offset, count);
            }
        }

        private Task GenerateSilenceAsync(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                using (var waveSource = new SilenceSource(_loopbackCapture.WaveFormat))
                {
                    using (var soundOut = new WasapiOut { Latency = 10, Device = _loopbackCapture.Device })
                    {
                        void Play(object sender, PlaybackStoppedEventArgs e)
                        {
                            // has to be run on a different thread to the one that raised the callback
                            Task.Run(() =>
                            {

                                try
                                {
                                    soundOut.Play();
                                }
                                catch
                                {
                                    // nothing we can do here, ignore it
                                }
                            });
                        }

                        soundOut.Initialize(waveSource);
                        soundOut.Stopped += Play;
                        soundOut.Play();
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(1000);
                        }

                        soundOut.Stopped -= Play;
                    }
                }
            });
        }

        public WaveFormat WaveFormat => _loopbackCapture.WaveFormat;

        public void Dispose()
        {
            _loopbackCapture.Dispose();
        }
    }
}
