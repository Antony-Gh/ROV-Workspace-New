using System;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using static MAVLink;
using System.Windows.Media.Imaging;
using Renci.SshNet;
using System.Windows.Media.Media3D;
using OxyPlot;
using System.Windows.Media;
using ROV_GUI_Control.View;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot.Axes;
using HelixToolkit.Wpf;
using System.Collections.Generic;
namespace ROV_GUI_Control.ViewModels
{
    public class MainViewModel : ObservableObject, INotifyPropertyChanged, IDisposable
    {
        #region connection        
        public string Host_IP { get; set; }
        public string UserName { get; }
        public string Password { get; }
        public int Cam1_Port { get; set; }
        public int Cam2_Port { get; set; }
        public int Cam3_Port { get; set; }
        public int Joystick_Port { get; set; }
        public int MAVLink_Port { get; set; }

        public ICommand ConnectCommand { get; }
        private bool _isConnect = false;
        public bool IsConnect
        {
            get => _isConnect;
            set
            {
                if (_isConnect != value)
                {
                    _isConnect = value;
                    OnPropertyChanged(nameof(IsConnect));
                }
            }
        }
        private async void ExecuteConnectCommand()
        {
            try
            {
                if (!IsConnect)
                {
                    IsConnect = await MavlinkHandler.ConnectionCheck();
                    if (!IsConnect)
                    {
                        MessageBox.Show($"Connect to {Host_IP} faild !");
                    }
                }
                else
                {
                    using var ssh = new SshClient(Host_IP, UserName, Password);
                    ssh.Connect();
                    // ssh.RunCommand("pkill -f bridge.py"); /* Old Version */
                    ssh.RunCommand("pkill -f main.py");
                    ssh.Disconnect();
                    IsConnect = false;
                    IsPower = false;
                    IsEnable = false;
                    IsStream = false;
                }
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"Connection error: {ex.Message}");
                 IsConnect = false;
            }
        }
        #endregion

        #region MavlinkUpdate
        public GaugeSpeedViewModel Speedgauge { get; set; }
        public GaugeDepthViewModel Depthgauge { get; set; }
        public GaugeCompassViewModel Compassgauge { get; set; }
        private readonly byte SystemID = 1;
        private readonly byte ComponentID = 1;
        private static readonly MavlinkParse mavlinkParser = new();
        private readonly MAVLinkHandler MavlinkHandler;
        private readonly DispatcherTimer _timer;
        private int _lineCount = 0;
        private const int MaxLines = 13;
        readonly SemaphoreSlim _semaphore = new(1, 1);

        private string _heartbeatText;
        public string HeartBeatText
        {
            get => _heartbeatText;
            set
            {
                _heartbeatText = value;
                OnPropertyChanged();
            }
        }
        private StringBuilder _statustext = new();
        public string StatusText
        {
            get => _statustext.ToString();
            set
            {
                _statustext = new StringBuilder(value);
                OnPropertyChanged(nameof(StatusText));
            }
        }
        private float _speed = 0;
        public float Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnPropertyChanged(nameof(Speed));
            }
        }
        private float _depth = 0;
        public float Depth
        {
            get => _depth;
            set
            {
                _depth = value;
                OnPropertyChanged(nameof(Depth));
            }
        }
        private float _comp_value = 0;
        public float CompValue
        {
            get => _comp_value;
            set
            {
                _comp_value = value;
                OnPropertyChanged(nameof(CompValue));
            }
        }
        private float _roll = 0;
        public float Roll
        {
            get { return _roll; }
            set
            {
                if (_roll != value)
                {
                    _roll = value;
                    OnPropertyChanged(nameof(Roll));
                }
            }
        }

        private float _pitch = 0;
        public float Pitch
        {
            get { return _pitch; }
            set
            {
                if (_pitch != value)
                {
                    _pitch = value;
                    OnPropertyChanged(nameof(Pitch));
                }
            }
        }

        private float _yaw = 0;
        public float Yaw
        {
            get { return _yaw; }
            set
            {
                if (_yaw != value)
                {
                    _yaw = value;
                    OnPropertyChanged(nameof(Yaw));
                }
            }
        }
        private float _water_temp = 0;
        public float Water_Temp
        {
            get { return _water_temp; }
            set
            {
                if (_water_temp != value)
                {
                    _water_temp = value;
                    OnPropertyChanged(nameof(Water_Temp));
                }
            }
        }
        private float _water_press = 0;
        public float Water_Press
        {
            get { return _water_press; }
            set
            {
                if (_water_press != value)
                {
                    _water_press = value;
                    OnPropertyChanged(nameof(Water_Press));
                }
            }
        }
        private float _tube_temp = 0;
        public float Tube_Temp
        {
            get { return _tube_temp; }
            set
            {
                if (_tube_temp != value)
                {
                    _tube_temp = value;
                    OnPropertyChanged(nameof(Tube_Temp));
                }
            }
        }
        private float _tube_press = 0;
        public float Tube_Press
        {
            get { return _tube_press; }
            set
            {
                if (_tube_press != value)
                {
                    _tube_press = value;
                    OnPropertyChanged(nameof(Tube_Press));
                }
            }
        }       
        private async void UpdateHeartbeat(mavlink_heartbeat_t heartbeat)
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                HeartBeatText = $"  Type: {heartbeat.type}\n" +
                                 $"  Autopilot: {heartbeat.autopilot}\n" +
                                 $"  Mode: {heartbeat.base_mode}\n" +
                                 $"  Status: {heartbeat.system_status}\n" +
                                 $"  MAVLink Version: {heartbeat.mavlink_version}";
                    OnPropertyChanged(nameof(HeartBeatText));
                    Flash();
             }, DispatcherPriority.Background);
        }
        private async void UpdateStatus(string message)
        {
            await _semaphore.WaitAsync();
            await App.Current.Dispatcher.InvokeAsync(() =>
            { 
            try
            {
                var timestamp = $"[{DateTime.Now:HH:mm:ss}] ";
                var newMessage = timestamp + message;

                if (_lineCount > 0)
                    _statustext.AppendLine();

                _statustext.Append(newMessage);
                _lineCount++;

                if (_lineCount > MaxLines)
                {
                    int firstNewLine = _statustext.ToString().IndexOf(Environment.NewLine);
                    if (firstNewLine >= 0)
                    {
                        _statustext.Remove(0, firstNewLine + Environment.NewLine.Length);
                        _lineCount--;
                    }
                }
                StatusText = _statustext.ToString();
            }
            finally
            {
                _semaphore.Release();
            }
        }, DispatcherPriority.Background);
        }
        private DateTime _lastGaugeUpdate = DateTime.MinValue;
        private DateTime _lastAttitudeUpdate = DateTime.MinValue;
        private DateTime _lastWaterUpdate = DateTime.MinValue;
        private const int UpdateIntervalMs = 33; // ~30Hz

        private async void UpdateGauges(mavlink_vfr_hud_t vfrHud)
        {
            if ((DateTime.Now - _lastGaugeUpdate).TotalMilliseconds < UpdateIntervalMs) return;
            _lastGaugeUpdate = DateTime.Now;

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Speed = vfrHud.groundspeed;
                Depth = vfrHud.alt;
                CompValue = vfrHud.heading;
            }, DispatcherPriority.Background);
        }
        private async void UpdateAttitude(mavlink_attitude_t attitude)
        {
            if ((DateTime.Now - _lastAttitudeUpdate).TotalMilliseconds < UpdateIntervalMs) return;
            _lastAttitudeUpdate = DateTime.Now;

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Roll = attitude.roll;
                Pitch = attitude.pitch;
                Yaw = attitude.yaw;
            }, DispatcherPriority.Background);
        }
        private async void UpdateWaterEnv(mavlink_scaled_pressure_t pres_temp_w)
        {
            if ((DateTime.Now - _lastWaterUpdate).TotalMilliseconds < UpdateIntervalMs) return;
            _lastWaterUpdate = DateTime.Now;

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Water_Temp = pres_temp_w.temperature;
                    Water_Press = pres_temp_w.press_abs;
            }, DispatcherPriority.Background);
        }
        private async void UpdateTubeEnv(mavlink_scaled_pressure2_t pres_temp_t)
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Tube_Temp = pres_temp_t.temperature;
                Tube_Press = pres_temp_t.press_abs;
            }, DispatcherPriority.Background);
        }
        private bool _isEllipseVisible = false;
        public bool IsEllipseVisible
        {
            get => _isEllipseVisible;
            set
            {
                _isEllipseVisible = value;
                OnPropertyChanged(nameof(IsEllipseVisible));
            }
        }
        public void Flash()
        {
            IsEllipseVisible = true;
            _timer.Stop();
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            IsEllipseVisible = false;
            HeartBeatText = "";
            _timer.Stop();
        }
        private async Task SendPIDCommand(int type, int param)
        {
            var command = new MAVLink.mavlink_command_long_t
            {
                target_system = 1,
                target_component = 1,
                confirmation = 0
            };
            command.command = (ushort)MAV_CMD.DO_SET_PARAMETER;
            command.param1 = type;
            switch (type)
            {
                case 0:
                    command.param1 = PID["SKP"].Value;
                    command.param2 = PID["SKI"].Value;
                    command.param3 = PID["SKD"].Value;
                    command.param4 = PID["SALPHA"].Value;
                    command.param5 = PID["SMAX"].Value;
                    command.param6 = PID["SMIN"].Value;
                    command.param7 = PID["SANTI"].Value;
                    break;
                case 1:
                    command.param1 = PID["RKP"].Value;
                    command.param2 = PID["RKI"].Value;
                    command.param3 = PID["RKD"].Value;
                    command.param4 = PID["RALPHA"].Value;
                    command.param5 = PID["RMAX"].Value;
                    command.param6 = PID["RMIN"].Value;
                    command.param7 = PID["RANTI"].Value;
                    break;
                case 2:
                    command.param1 = PID["PKP"].Value;
                    command.param2 = PID["PKI"].Value;
                    command.param3 = PID["PKD"].Value;
                    command.param4 = PID["PALPHA"].Value;
                    command.param5 = PID["PMAX"].Value;
                    command.param6 = PID["PMIN"].Value;
                    command.param7 = PID["PANTI"].Value;
                    break;
                case 3:
                    command.param1 = PID["YKP"].Value;
                    command.param2 = PID["YKI"].Value;
                    command.param3 = PID["YKD"].Value;
                    command.param4 = PID["YALPHA"].Value;
                    command.param5 = PID["YMAX"].Value;
                    command.param6 = PID["YMIN"].Value;
                    command.param7 = PID["YANTI"].Value;
                    break;
            }
            var packet = mavlinkParser.GenerateMAVLinkPacket20(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, command, false, SystemID, ComponentID);
            await MavlinkHandler.SendCommand(packet);
        }
        private async Task SendCommand(int type, int param)
        {
            var command = new MAVLink.mavlink_command_long_t
            {
                target_system = 1,
                target_component = 1,
                confirmation = 0
            };
            switch (type)
            {
                case 0: command.command = (ushort)MAV_CMD.DO_FLIGHTTERMINATION; break;
                case 1: command.command = (ushort)MAV_CMD.COMPONENT_ARM_DISARM; break;
                case 2: command.command = (ushort)MAV_CMD.DO_SET_SERVO; break;
                case 3: command.command = (ushort)MAV_CMD.DO_SET_PARAMETER; break;
            }
            if(type == 3)
            {
                command.param1 = param;
            }
            command.param1 = param;
            var packet = mavlinkParser.GenerateMAVLinkPacket20(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, command, false, SystemID, ComponentID);
            await MavlinkHandler.SendCommand(packet);
        }
        #endregion
        
        #region Stream
        private readonly CAMStream Feed1;
        private BitmapImage _image1;
        public BitmapImage Image1
        {
            get => _image1;
            set
            {
                _image1 = value;
                OnPropertyChanged(nameof(Image1));
            }
        }
        private readonly CAMStream Feed2;
        private BitmapImage _image2;
        public BitmapImage Image2
        {
            get => _image2;
            set
            {
                _image2 = value;
                OnPropertyChanged(nameof(Image2));
            }
        }
        private readonly CAMStream Feed3;
        private BitmapImage _image3;
        public BitmapImage Image3
        {
            get => _image3;
            set
            {
                _image3 = value;
                OnPropertyChanged(nameof(Image3));
            }
        }
        public async Task StartAllAsync()
        {
            await Task.Run(() =>
            {
                Index1 = PIndex1;
                Index2 = PIndex2;
                Index3 = PIndex3;
                using var ssh = new SshClient(Host_IP, UserName, Password);
                if(IsConnect)
                {
                    ssh.Connect();
                    ssh.RunCommand("nohup python3 /home/rov/main.py > /dev/null 2>&1 &");
                    ssh.Disconnect();
                }
                else
                {
                    MessageBox.Show("No Connection.");
                }
                Feed1?.Start();
                Feed2?.Start();
                //Feed3?.Start();
            });
        }
        public async Task StopAllAsync()
        {
            await Task.Run(() =>
            {
                using var ssh = new SshClient(Host_IP, UserName, Password);
                if(IsConnect)
                {
                    ssh.Connect();
                    ssh.RunCommand("pkill -f main.py");
                    ssh.Disconnect();
                }
                else
                {
                    MessageBox.Show("No Connection.");
                }
                Feed1?.Stop();
                Feed2?.Stop();
                //Feed3?.Stop();
            });
        }
        public ICommand StreamCommand { get; }
        private void ToggleStream()
        {

            if (IsConnect)
            {
                IsStream = !IsStream;
                if (IsStream)
                    _ = StartAllAsync();
                else
                    _ = StopAllAsync();
            }
            else
                MessageBox.Show("No Connection.");
        }
        private bool _isStream;
        public bool IsStream
        {
            get { return _isStream; }
            set
            {
                if (_isStream != value)
                {
                    _isStream = value;
                    OnPropertyChanged(nameof(IsStream));
                }
            }
        }
        public ObservableCollection<string> Feed_Comb { get; }
        public int PIndex1;
        public int PIndex2;
        public int PIndex3;
        private int  _index1;
        public int Index1
        {
            get => _index1;
            set
            {
                if (_index1 != value)
                {
                    _index1 = value;
                    OnPropertyChanged(nameof(Index1));
                    _ = FeedChange(1);
                }
            }
        }
        private int _index2;
        public int Index2
        {
            get => _index2;
            set
            {
                if (_index2 != value)
                {
                    _index2 = value;
                    OnPropertyChanged(nameof(Index2));
                    _ = FeedChange(2);
                }
            }
        }
        private int _index3;
        public int Index3
        {
            get => _index3;
            set
            {
                if (_index3 != value)
                {
                    _index3 = value;
                    OnPropertyChanged(nameof(Index3));
                    _ = FeedChange(3);
                }
            }
        }
         private void IndexChange(ref int a, ref int b,ref int c, ref int d,int e)
         {
            int temp = b;
            d = b; 
            b = a;
            if (c > 0) // index(X) should change last after (b) change so no two change happen at the same time
                switch (e)
                {
                    case 1:
                        Index1 = temp;
                        break;
                    case 2:
                        Index2 = temp;
                        break;
                    case 3:
                        Index3 = temp;
                        break;
                    default: break;
                }
        }
        private async Task FeedChange(int a)
        {
            await Task.Run(() =>
            {
                switch (a)
                {
                    case 1:
                        if(Index1 == 0)
                        {
                            if(IsStream)
                                PIndex1 = Index1;
                        }
                        else if (_index1 == _index2 || _index1 == PIndex2)
                        {
                            IndexChange(ref _index1, ref PIndex1, ref _index2, ref PIndex2,2);
                            int p = Feed1.GetPort();
                            Feed1.SetPort(Feed2.GetPort());
                            Feed2.SetPort(p);
                        }
                        else if (_index1 == _index3 || _index1 == PIndex3)
                        {
                            IndexChange(ref _index1, ref PIndex1,ref _index3, ref PIndex3,3);
                            int p = Feed1.GetPort();
                            Feed1.SetPort(Feed3.GetPort());
                            Feed3.SetPort(p);
                        }
                        else
                        {
                            PIndex1 = Index1;
                        }
                        break;
                    case 2:
                        if (Index2 == 0)
                        {
                            if (IsStream)
                                PIndex2 = Index2;
                        }
                        else if (_index2 == _index1 || _index2 == PIndex1)
                        {
                            IndexChange(ref _index2, ref PIndex2, ref _index1, ref PIndex1, 1);
                            int p = Feed2.GetPort();
                            Feed2.SetPort(Feed1.GetPort());
                            Feed1.SetPort(p);
                        }
                        else if (_index2 == _index3 || _index2 == PIndex3)
                        {
                            IndexChange(ref _index2, ref PIndex2, ref _index3, ref PIndex3, 3);
                            int p = Feed2.GetPort();
                            Feed2.SetPort(Feed3.GetPort());
                            Feed3.SetPort(p);
                        }
                        else
                        {
                            PIndex2 = Index2;
                        }
                        break;
                    case 3:
                        if (Index3 == 0)
                        {
                            if (IsStream)
                                PIndex3 = Index3;
                        }
                        else if (_index3 == _index1 || _index3 == PIndex1)
                        {
                            IndexChange(ref _index3, ref PIndex3, ref _index1, ref PIndex1, 1);
                            int p = Feed3.GetPort();
                            Feed3.SetPort(Feed1.GetPort());
                            Feed1.SetPort(p);
                        }
                        else if (_index3 == _index2 || _index3 == PIndex2)
                        {
                            IndexChange(ref _index3, ref PIndex3, ref _index2, ref PIndex2, 2);
                            int p = Feed3.GetPort();
                            Feed3.SetPort(Feed2.GetPort());
                            Feed2.SetPort(p);
                        }
                        else
                        {
                            PIndex3 = Index3;
                        }
                        break;
                    default: break;
                }
            });
        }
        #endregion
        #region Plot
        private readonly Plot tubeTemp;
        private readonly Plot tubePressure;
        private readonly Plot waterTemp;
        private readonly Plot waterPressure;
        public PlotModel TubeTempModel { get; set; }
        public PlotModel TubePressureModel { get; set; }
        public PlotModel WaterTempModel { get; set; }
        public PlotModel WaterPressureModel { get; set; }
        #endregion
        #region Power on/off
        public ICommand PowerCommand { get; }
        private bool _isPower = false;
        public bool IsPower
        {
            get => _isPower;
            set
            {
                if (_isPower != value)
                {
                    _isPower = value;
                    OnPropertyChanged(nameof(IsPower));
                }
            }
        }
        private async void ExecutePowerCommand()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (!IsConnect)
                    {
                        MessageBox.Show("No Connection !");
                    }
                    else
                    {
                        IsPower = !IsPower;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing power command: {ex.Message}");
            }
        }
        #endregion
        #region Enable/Disable
        public ICommand EnableCommand { get; }

        private bool _isEnable = false;
        public bool IsEnable
        {
            get { return _isEnable; }
            set
            {
                if (_isEnable != value)
                {
                    _isEnable = value;
                    OnPropertyChanged(nameof(IsEnable));
                    //_ = SendCommandsAsync(1, _isMotorEnabled ? 1 : 0);
                }
            }
        }
        private async void ExecuteEnableCommand()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (!IsConnect)
                        MessageBox.Show("No Connection !");
                    else
                    {
                        if (IsPower)
                            IsEnable = !IsEnable;
                        else
                            MessageBox.Show("Power is OFF.");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing enable command: {ex.Message}");
            }
        }
        #endregion
        #region Settings
        private readonly IWindowService _windowService;
        public ICommand SettingsCommand { get; }
        private bool _isSettings = false;
        public bool IsSettings
        {
            get => _isSettings;
            set
            {
                if (_isSettings != value)
                {
                    _isConnect = value;
                    OnPropertyChanged(nameof(IsSettings));
                }
            }
        }
        public VariableGroup PID { get; set; } = new VariableGroup();
        private void ExecuteSettingsCommand()
        {
            var (result, ip, cam1port, cam2port, cam3port, joyport, mavport, pid) = _windowService.ShowWindow(Host_IP, Cam1_Port, Cam2_Port, Cam3_Port, Joystick_Port,MAVLink_Port, PID);
            Host_IP = ip;
            Cam1_Port = cam1port;
            Cam2_Port = cam2port;
            Cam3_Port = cam3port;
            Joystick_Port = joyport;
            MAVLink_Port = mavport;
            PID = pid;
        }
        #endregion

        #region Light on/off
        public ICommand LightCommand { get; }
        private bool _isLight = false;
        public bool IsLight
        {
            get { return _isLight; }
            set
            {
                if (_isLight != value)
                {
                    _isLight = value;
                    OnPropertyChanged(nameof(IsLight));
                }
            }
        }
        private async void ExecuteLightCommand()
        {
            await Task.Run(() =>
            {
                IsLight = !IsLight; 
            });
        }
        #endregion
        #region Joystick
        private readonly JOYStick JOYSICK;
        private int _moveid;
        public int MoveID
        {
            get { return _moveid; }
            set
            {
                if (_moveid != value)
                {
                    _moveid = value;
                    OnPropertyChanged(nameof(MoveID));
                }
            }
        }
        private bool _addmark = false;
        public bool AddMark
        {
            get => _addmark;
            set
            {
                _addmark = value;
                OnPropertyChanged(nameof(AddMark));
            }
        }
        #endregion
        #region 3D

        #endregion
        #region PID
        #endregion

        public MainViewModel(string username, string password, IWindowService windowService)
        {
            _windowService = windowService;

            IsConnect = false;
            IsPower = false;
            IsEnable = false;
            IsStream = false;

            #region Stream
            Host_IP = "192.168.0.0";
            UserName = username;
            Password = password;
            Cam1_Port = 5000;
            Cam2_Port = 6000;
            Cam3_Port = 7000;
            Joystick_Port = 14550;
            MAVLink_Port = 14550;

            Feed1 = new CAMStream(Cam1_Port);
            Feed1.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Image")
                {
                    Image1 = Feed1.Image;
                }
            };
            Feed2 = new CAMStream(Cam2_Port);
            Feed2.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Image")
                {
                    Image2 = Feed2.Image;
                }
            };
            Feed3 = new CAMStream(Cam3_Port);
            Feed3.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Image")
                {
                    Image3 = Feed3.Image;
                }
            };
            StreamCommand = new RelayCommand(_ => ToggleStream());

            Index1 = 0;
            Index2 = 0;
            Index3 = 0;

            PIndex1 = 1;
            PIndex2 = 2;
            PIndex3 = 3;
            Feed_Comb =
            [
                "None",
                "Camera 1",
                "Camera 2",
                "Camera 3"
            ];
            #endregion
            #region plot
            TubeTempModel = new PlotModel { Title = "Tube Temp", DefaultFontSize = 9, TitleFontSize = 9, TitleColor = OxyColors.White };
            TubePressureModel = new PlotModel { Title = "Tube Pressure", DefaultFontSize = 9, TitleFontSize = 9, TitleColor = OxyColor.Parse("#FFFFFFFF") };
            WaterTempModel = new PlotModel { Title = "Water Temp", DefaultFontSize = 9, TitleFontSize = 9, TitleColor = OxyColor.Parse("#FFFFFFFF") };
            WaterPressureModel = new PlotModel { Title = "Water Pressure", DefaultFontSize = 9, TitleFontSize = 9, TitleColor = OxyColor.Parse("#FFFFFFFF") };
            tubeTemp = new(TubeTempModel, "Temp (°C)");
            tubePressure = new(TubePressureModel, "Pressure (Pa)");
            waterTemp = new(WaterTempModel, "Temp (°C)");
            waterPressure = new(WaterPressureModel, "Pressure (Pa)");
            #endregion

            #region Joystick
            JOYSICK = new(Host_IP, Joystick_Port);
            JOYSICK.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "MoveID")
                {
                    MoveID = JOYSICK.MoveID;
                }
            };
            JOYSICK.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "AddMark")
                {
                    AddMark = JOYSICK.AddMark;
                }
            };
            #endregion

            #region Mavlink
            MavlinkHandler = new(Host_IP, MAVLink_Port);
            Speedgauge = new GaugeSpeedViewModel() { Speed_Value = 0 };
            Depthgauge = new GaugeDepthViewModel() { Depth_Value = 0 };
            Compassgauge = new GaugeCompassViewModel() { Comp_Value = 0 };
            MavlinkHandler.UpdateHearbeat += UpdateHeartbeat;
            MavlinkHandler.UpdateVFR_HUD += UpdateGauges;
            MavlinkHandler.UpdateStatus += msg => UpdateStatus(msg);
            MavlinkHandler.UpdateAttitude += UpdateAttitude;
            MavlinkHandler.UpdateWaterEnv += UpdateWaterEnv;
            MavlinkHandler.UpdateTubeEnv += UpdateTubeEnv;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += Timer_Tick;
            ConnectCommand = new RelayCommand(_ => ExecuteConnectCommand());
            PowerCommand = new RelayCommand(_ => ExecutePowerCommand());
            EnableCommand = new RelayCommand(_ => ExecuteEnableCommand());
            SettingsCommand = new RelayCommand(_ => ExecuteSettingsCommand());
            LightCommand = new RelayCommand(_ => ExecuteLightCommand());

            #endregion
            #region 3D



            #endregion
            #region PID
            PID.Add("SKP", 1.0f);
            PID.Add("SKI", 0.0f);
            PID.Add("SKD", 0.08f);
            PID.Add("SALPHA", 0.08f);
            PID.Add("SMAX", 20.0f);
            PID.Add("SMIN", -20.0f);
            PID.Add("SANTI", 0.4f);
            PID.Add("RKP", 1.0f);
            PID.Add("RKI", 0.3f);
            PID.Add("RKD", 0.0f);
            PID.Add("RALPHA", 0.08f);
            PID.Add("RMAX", 60.0f);
            PID.Add("RMIN", -60.0f);
            PID.Add("RANTI", 0.5f);
            PID.Add("PKP", 1.0f);
            PID.Add("PKI", 0.2f);
            PID.Add("PKD", 0.0f);
            PID.Add("PALPHA", 0.08f);
            PID.Add("PMAX", 0.6f);
            PID.Add("PMIN", -0.6f);
            PID.Add("PANTI", 0.4f);
            PID.Add("YKP", 1.0f);
            PID.Add("YKI", 0.0f);
            PID.Add("YKD", 0.08f);
            PID.Add("YALPHA", 0.08f);
            PID.Add("YMAX", 20.0f);
            PID.Add("YMIN", -20.0f);
            PID.Add("YANTI", 0.4f);
            #endregion
        }
        public ICommand LEDLightness { get; }
        private double _ledLightness;
        public double LedLightness
        {
            get => _ledLightness;
            set
            {
                if (_ledLightness != value)
                {
                    _ledLightness = value;
                    OnPropertyChanged(nameof(LedLightness));
                }
        }
        }
        /* Note: PropertyChanged event and OnPropertyChanged are inherited from ObservableObject */

        // public event PropertyChangedEventHandler PropertyChanged;
        // protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        //     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        
        public void Dispose()
        {
            Feed1?.Dispose();
            Feed2?.Dispose();
            Feed3?.Dispose();
            JOYSICK?.Dispose();
            MavlinkHandler.Dispose();
        }
    }
}