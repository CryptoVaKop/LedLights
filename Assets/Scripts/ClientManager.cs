using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;


public class ClientManager : MonoBehaviour
{
    /// <summary>
    /// Address and port to connect
    /// </summary>
    public string address = "192.168.1.4";
    public int port = 12100;

    /// <summary>
    /// Avaible commands.
    /// </summary>
    private enum Command_t
    {
        GET_LEDS,  // Get led states(not used)
        SET_LEDS,  // Set led states
    }

    /// <summary>
    /// Reference to color picker.
    /// </summary>
    private ColorPicker ColorPicker;

    /// <summary>
    /// Maximum packet data lenght.
    /// </summary>
    private const int PaylodLenMax = 128;

    /// <summary>
    /// Reference to TCP client. 
    /// </summary>
    private TcpClient Client; //= new TcpClient();

    /// <summary>
    /// Reference to connect task and reference to send task. 
    /// </summary>
    private Task ConnectTask;
    private Task SendTask;

    /// <summary>
    /// Reference to NetworkStream to send data.
    /// </summary>
    private NetworkStream Stream;

    /// <summary>
    /// Timer for periodic sending packets.
    /// </summary>
    private SoftTimer Timer = new SoftTimer();

    /// <summary>
    /// Bytes array for create packet.
    /// </summary> 
    private byte[] Packet = new byte[sizeof(uint) + sizeof(ushort) + PaylodLenMax];

    /// <summary>
    /// Packet preamble.
    /// </summary>
    private uint Preample = 0xAABBCCDD;

    /// <summary>
    /// State machine step.
    /// </summary>
    private int Step = 0;

    /// <summary>
    /// Last color picker color.
    /// </summary>
    private Color LastColor;



    // Start is called before the first frame update
    void Start()
    {
        // Find ColorPicker script in the scene
        ColorPicker = FindObjectOfType<ColorPicker>();
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            switch (Step)
            {
                case 0:
                {
                    if (Timer.Match())
                    {
                        // Create TCP client and asynchronously connect to server
                        Client = new TcpClient();
                        ConnectTask = Client.ConnectAsync(IPAddress.Parse(address), port);
                        Debug.Log(string.Format("Connecting on {0} : {1}", address, port));
                        Step++;
                    }
                    break;
                }

                case 1:
                {
                    if (ConnectTask.Status == TaskStatus.RanToCompletion)
                    {
                        Debug.Log("Connected!");
                        Stream = Client.GetStream();
                        LastColor = ColorPicker.Color;
                        Timer.Start(100);
                        Step++;
                    }
                    else if (ConnectTask.Exception != null)
                    {
                        throw ConnectTask.Exception.InnerException;
                    }
                    break;
                }

                case 2:
                {
                    if ((LastColor != ColorPicker.Color) && Timer.Match())
                    {
                        LastColor = ColorPicker.Color;
                        Timer.Start(100);

                        int startLed = 0;
                        int stopLed = 0;
                        Color32 color = LastColor;

                        // Create packet
                        int n = 0;
                        Packet[n++] = (byte)Preample;
                        Packet[n++] = (byte)(Preample >> 8);
                        Packet[n++] = (byte)(Preample >> 16);
                        Packet[n++] = (byte)(Preample >> 24);

                        int packetLen = 8;
                        Packet[n++] = (byte)packetLen;
                        Packet[n++] = (byte)(packetLen >> 8);
                        int dataOffset = n;
                        Packet[n++] = (byte)Command_t.SET_LEDS;
                        Packet[n++] = (byte)startLed;
                        Packet[n++] = (byte)(startLed >> 8);
                        Packet[n++] = (byte)stopLed;
                        Packet[n++] = (byte)(stopLed >> 8);
                        Packet[n++] = color.r;
                        Packet[n++] = color.g;
                        Packet[n++] = color.b;

                        ushort crc = Crc.CRC16(Packet, dataOffset, n - dataOffset, 0);
                        Packet[n++] = (byte)crc;
                        Packet[n++] = (byte)(crc >> 8);

                        // Send packet
                        Debug.Log(string.Format("Sending color: {0}", color));
                        SendTask = Stream.WriteAsync(Packet, 0, n);
                        Step++;
                    }

                    break;
                }

                case 3:
                {
                    if (SendTask.Status == TaskStatus.RanToCompletion)
                    {
                        Debug.Log("Sended!");
                        Step--;
                    }
                    else if (SendTask.Exception != null)
                    {
                        throw SendTask.Exception.InnerException;
                    }
                    break;
                }

                default:
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            // if something went wrong - close everything and try to connect again
            if (Stream != null)
            {
                Stream.Close();
                Stream = null;
            }

            if (Client != null)
            {
                Client.Close();
                Client = null;
            }
            Timer.Start(5000);
            Step = 0;
            Debug.Log(ex.Message);
        }
    }
}
