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
                while (!token.IsCancellationRequested)
                {
                    var result = await UDPReceiver.ReceiveAsync();
                    
                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        using var ms = new MemoryStream(result.Buffer);
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();

                        Image = bitmapImage;
                    });
                }
            }
            catch (ObjectDisposedException)// UDP has been disposed
            {
               
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
