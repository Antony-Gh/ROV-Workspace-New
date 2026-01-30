using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;

namespace ROV_GUI_Control.ViewModels
{
    public class CAMStream(int port) : INotifyPropertyChanged, IDisposable
    {
        private UdpClient UDPReceiver;
        private CancellationTokenSource cancellation;

        private int _port = port;
        public int Port
        {
            get => _port;
            private set
            {
                _port = value;
                OnPropertyChanged(nameof(Port));
                _ = Restart();
            }
        }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get => _image;
            private set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }
        public void Start()
        {
            cancellation = new CancellationTokenSource();
            _ = ReceiveAsync(cancellation.Token);
        }
        private async Task Restart()
        {
            await Task.Run(() =>
            {
                cancellation?.Cancel();
                UDPReceiver?.Dispose();
                cancellation = new CancellationTokenSource();
                _ = ReceiveAsync(cancellation.Token);
            });
        }

        private async Task ReceiveAsync(CancellationToken token)
        {
            UDPReceiver = new UdpClient(Port);
            try
            {
                MemoryStream frameBuffer = new MemoryStream();
                while (!token.IsCancellationRequested)
                {
                    var result = await UDPReceiver.ReceiveAsync();
                    var data = result.Buffer;
                    if (data.Length < 1) continue;

                    byte flag = data[0];
                    // Protocol: 0=Middle, 1=Start, 2=End, 3=Single(RAM)
                    
                    if (flag == 1) // Start of Frame
                    {
                        frameBuffer.SetLength(0);
                        frameBuffer.Write(data, 1, data.Length - 1);
                    }
                    else if (flag == 0) // Middle Chunk
                    {
                        frameBuffer.Write(data, 1, data.Length - 1);
                    }
                    else if (flag == 2) // End of Frame
                    {
                        frameBuffer.Write(data, 1, data.Length - 1);
                        var frameData = frameBuffer.ToArray();
                        
                        await Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            try 
                            { 
                                using var ms = new MemoryStream(frameData);
                                var bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.StreamSource = ms;
                                bitmapImage.EndInit();
                                bitmapImage.Freeze();
                                Image = bitmapImage;
                            }
                            catch (Exception) { /* Handle invalid bitmaps slightly gracefully */ }
                        });
                    }
                    else if (flag == 3) // Single Packet Frame
                    {
                        await Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                using var ms = new MemoryStream(data, 1, data.Length - 1);
                                var bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.StreamSource = ms;
                                bitmapImage.EndInit();
                                bitmapImage.Freeze();
                                Image = bitmapImage;
                            }
                            catch (Exception) { }
                        });
                    }
                }
            }
            catch (ObjectDisposedException)// UDP has been disposed
            {
               
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, ignore
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReceiveAsync: {ex.Message}");
            }
        }
        public void SetPort(int port)
        {
            Port = port;
        }
        public int GetPort()
        {
            return Port;
        }
        public void Stop()
        {
            cancellation?.Cancel();
            UDPReceiver?.Close();
            UDPReceiver?.Dispose();
            UDPReceiver = null;
        }

        public void Dispose()
        {
            Stop();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
