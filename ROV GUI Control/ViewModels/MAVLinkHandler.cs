using System;
using System.Net;
using static MAVLink;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace ROV_GUI_Control.ViewModels
{
    public class MAVLinkHandler : IDisposable
    {
        private readonly UdpClient UDPClient;
        private readonly IPEndPoint RemoteEP;
        private static readonly MavlinkParse mavlinkParser = new();
        private static int Port { get; set; }
        private static string IP { get; set; }

        public event Action<mavlink_heartbeat_t> UpdateHearbeat;
        public event Action<string> UpdateStatus;
        public event Action<mavlink_vfr_hud_t> UpdateVFR_HUD;
        public event Action<mavlink_attitude_t> UpdateAttitude;
        public event Action<mavlink_scaled_pressure_t> UpdateWaterEnv;
        public event Action<mavlink_scaled_pressure2_t> UpdateTubeEnv;
        public event Action<IPEndPoint> OnRemoteEndPointUpdated;

        private IPEndPoint LastReceivedEndPoint;

        public MAVLinkHandler(string ip, int  port)
        {
            Port = port;
            IP = ip;
            UDPClient = new UdpClient(Port);
            RemoteEP = new IPEndPoint(IPAddress.Parse(IP), Port);
            UDPClient.BeginReceive(ReceiveCallback, null);
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint remoteEndPoint = new(IPAddress.Any, Port);
                byte[] receivedBytes = UDPClient.EndReceive(ar, ref remoteEndPoint);

                if (LastReceivedEndPoint == null || !LastReceivedEndPoint.Equals(remoteEndPoint))
                {
                    LastReceivedEndPoint = remoteEndPoint;
                    // Dispatch to UI thread if needed, or just invoke
                    OnRemoteEndPointUpdated?.Invoke(LastReceivedEndPoint);
                }

                foreach (var b in receivedBytes)
                {
                    var packet = mavlinkParser.ReadByte(b);

                    _ = MessageProcessing(packet);
                }
                UDPClient.BeginReceive(ReceiveCallback, null);
            }
            catch (Exception)
            {
               // Dispatcher.Invoke(() => LogTextBox.AppendText($"Error: {ex.Message}\n"));
            }
        }
        private async Task MessageProcessing(MAVLinkMessage message)
        {
            await Task.Run(() =>
            {
                if (message != null)
                {
                    switch (message.msgid)
                    {
                        case (uint)MAVLINK_MSG_ID.HEARTBEAT:
                            var heartbeat = message.ToStructure<mavlink_heartbeat_t>();
                            UpdateHearbeat?.Invoke(heartbeat);
                            break;
                        case (uint)MAVLINK_MSG_ID.STATUSTEXT:
                            mavlink_statustext_t statusText = message.ToStructure<mavlink_statustext_t>();
                            string receivedMessage = System.Text.Encoding.ASCII.GetString(statusText.text).TrimEnd('\0');
                            UpdateStatus?.Invoke(receivedMessage);
                            break;
                        case (uint)MAVLINK_MSG_ID.VFR_HUD:
                            var vfrHud = message.ToStructure<mavlink_vfr_hud_t>();
                            UpdateVFR_HUD?.Invoke(vfrHud);
                            break;
                        case (uint)MAVLINK_MSG_ID.ATTITUDE:
                            var attitude = message.ToStructure<mavlink_attitude_t>();
                            UpdateAttitude?.Invoke(attitude);
                            break;
                        case (uint)MAVLINK_MSG_ID.SCALED_PRESSURE:
                            var pres_temp_w = message.ToStructure<mavlink_scaled_pressure_t>();
                            UpdateWaterEnv?.Invoke(pres_temp_w);
                            break;
                        case (uint)MAVLINK_MSG_ID.SCALED_PRESSURE2:
                            var pres_temp_t = message.ToStructure<mavlink_scaled_pressure2_t>();
                            UpdateTubeEnv?.Invoke(pres_temp_t);
                            break;
                        default:
                            Console.WriteLine($"Unknown Packet ID: {message.msgid}");
                            break;
                    }
                }
            });
        }
        public static async Task<bool> Check()
        {
            try
            {
                using var client = new TcpClient();
                var result = client.ConnectAsync(IP, 22);
                var completed = await Task.WhenAny(result, Task.Delay(1000));
                return result.IsCompleted && client.Connected;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> ConnectionCheck()
        {
            return await Check();
        }
        public async Task SendCommand(byte[] packet)
        {
            await Task.Run(async () =>
            {
                bool connected = await ConnectionCheck();
                if (connected)
                {
                    UDPClient.Send(packet, packet.Length, LastReceivedEndPoint ?? RemoteEP);
                }
            });
        }

        public void Dispose()
        {
            UDPClient?.Close();
        }
    }
}
