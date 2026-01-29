using System;
using System.Net;
using static MAVLink;
using System.Threading;
using System.Net.Sockets;
using SharpDX.DirectInput;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace ROV_GUI_Control.ViewModels
{
    public class JOYStick : INotifyPropertyChanged, IDisposable
    {
        private readonly byte systemId = 1;
        private readonly byte componentId = 1;
        private static readonly MavlinkParse mavlinkParser = new();

        private UdpClient UDPClient;
        private IPEndPoint IPEP;
        private readonly string IP;
        private readonly int PORT;

        private DirectInput directInput;
        private Joystick joystick;
        private JoystickState previousState = new();

        private Thread backgroundThread;
        private bool running = false;
        private bool move;
        private float prevSpeed;
        private int prevdir;

        private readonly int[,,] BasicMov;

        private int _moveid = -1;
        public int MoveID
        {
            get => _moveid;
            set
            {
                _moveid = value;
                OnPropertyChanged(nameof(MoveID));
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
        public JOYStick(string ip, int port)
        {
            IP = ip;
            PORT = port;
            InitializeJoystick();
            BasicMov = new int[3, 3, 3]
            {
                {
                    { 5, 5, 5 },
                    { 3, 3, 3 },
                    { 7, 7, 7 }
                },
                {
                    { 0, 0, 0 },
                    { 9,-1, 8 },
                    { 1, 1, 1 }
                },
                {
                    { 4, 4, 4 },
                    { 2, 2, 2 },
                    { 6, 6, 6 }
                }
            };
        }
        private void InitializeJoystick()
        {
            move = false;
            prevSpeed = 0;
            prevdir = -1;
            directInput = new DirectInput();
            var joystickDevices = directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly);
            if (joystickDevices.Count == 0)
            {
                Console.WriteLine("Joystick not found!");
                return;
            }
            joystick = new Joystick(directInput, joystickDevices[0].InstanceGuid);
            joystick.Acquire();

            UDPClient = new UdpClient();
            IPEP = new IPEndPoint(IPAddress.Parse(IP), PORT);

            running = true;
            backgroundThread = new(Run)
            {
                IsBackground = true
            };
            backgroundThread.Start();

            Console.WriteLine("Joystick initialized.");
        }
        private void Run()
        {
            try
            {
                while (running)
                {
                    UpdateJoystickNonUI();
                    Thread.Sleep(30);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in background thread: {ex.Message}");
            }
        }
        private void UpdateJoystickNonUI()
        {
            if (joystick == null) return;
            joystick.Poll();
            var state = joystick.GetCurrentState();
            if (state == null) return;
            float xAxis = state.X / 65535.0f * 2 - 1;
            float yAxis = state.Y / 65535.0f * 2 - 1;
            float zAxis = state.Z / 65535.0f * 2 - 1;
            float rzAxis = state.RotationZ / 65535.0f * 2 - 1;
            if (xAxis != (previousState.X / 65535.0f * 2 - 1) ||
                yAxis != (previousState.Y / 65535.0f * 2 - 1) ||
                zAxis != (previousState.Z / 65535.0f * 2 - 1) ||
                rzAxis != (previousState.RotationZ / 65535.0f * 2 - 1))
            {
                if (Math.Abs(xAxis) >= 0.1 || Math.Abs(yAxis) >= 0.1 || Math.Abs(zAxis) >= 0.1 || Math.Abs(rzAxis) >= 0.1)
                {
                    if (Math.Abs(rzAxis) >= 0.1)
                    {
                        int speed = (int)(Math.Abs(rzAxis) * 312);
                        if (rzAxis < 0)
                            v_Thrusters(10, speed);
                        else
                            v_Thrusters(11, speed);
                        move = true;
                    }
                    else
                    {
                        h_Thrusters(xAxis, yAxis, zAxis);
                        move = true;
                    }
                }
                else
                {
                    MoveID = BasicMov[1, 1, 1];
                    if(move)
                    {
                        SendManualControl(-1, 0);
                        move = false;
                    }
                }
            }
            if (state.Buttons[0] != previousState.Buttons[0])
            {
                AddMark = !AddMark;
            }
            previousState = state;
        }
        public void v_Thrusters(int r, int s)
        {
            MoveID = r;
            Task.Run(async () =>
            {
                if (prevdir == r)
                {
                    if(Math.Abs(s - prevSpeed) > 5)
                    {
                        Console.WriteLine(s);
                        SendManualControl(prevdir, s);
                    await Task.Delay(100);
                    prevSpeed = s;

                    }
                }
                else
                {
                    SendManualControl(r, s);
                    await Task.Delay(100);
                    prevSpeed = s;
                    prevdir = r;
                }
            });
        }
        public void h_Thrusters(float x, float y, float z)
        {
            x = (Math.Abs(x) < 0.19) ? 0 : x;
            y = (Math.Abs(y) < 0.19) ? 0 : y;
            z = (Math.Abs(z) < 0.19) ? 0 : z;
            int xn = (int)RoundAwayFromZero(x) + 1;
            int yn = (int)RoundAwayFromZero(y) + 1;
            int zn = (int)RoundAwayFromZero(z) + 1;
            MoveID = BasicMov[xn, yn, zn];
            // int speed = (int)(312*(Math.Abs(x) + Math.Abs(y) + Math.Abs(z)) /(Math.Abs(xn-1) + Math.Abs(yn-1) + Math.Abs(zn-1)));

            int denominator = Math.Abs(xn - 1) + Math.Abs(yn - 1) + Math.Abs(zn - 1);
            int speed = denominator > 0
                ? (int)(312 * (Math.Abs(x) + Math.Abs(y) + Math.Abs(z)) / denominator)
                : 0;
            
            Task.Run(async () =>
            {
                if (prevdir == BasicMov[xn, yn, zn])
                {
                    if (Math.Abs(speed - prevSpeed) > 5)
                    {
                        SendManualControl(prevdir, speed);
                        await Task.Delay(100);
                        prevSpeed = (int)speed;
                    }
                }
                else
                {
                    SendManualControl(BasicMov[xn, yn, zn], speed);
                    await Task.Delay(100);
                    prevSpeed = (int)speed;
                    prevdir = BasicMov[xn, yn, zn];
                }
            });
        }
        static int RoundAwayFromZero(float num)
        {
            if (num == 0)
                return 0;
            return (int)(num > 0 ? Math.Ceiling(num) : Math.Floor(num));
        }
        private void SendManualControl(int t, float s)
        {
            var command = new mavlink_manual_control_t
            {
                target = 1,
                x = 0,
                y = 0,
                z = 0,
                s = (short)s,
                r = (short)t,
                buttons = 0
            };
            var packet = mavlinkParser.GenerateMAVLinkPacket20(MAVLink.MAVLINK_MSG_ID.MANUAL_CONTROL, command, false, systemId, componentId);
            UDPClient.Send(packet, packet.Length, IPEP);
        }
        public void Stop()
        {
            running = false;
            backgroundThread?.Join();
            UDPClient?.Close();
            joystick?.Unacquire();
            directInput?.Dispose();
            Console.WriteLine("Joystick stopped.");
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public void Dispose()
        {
            Stop();
        }
    }
}