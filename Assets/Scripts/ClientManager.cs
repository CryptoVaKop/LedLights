using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class ClientManager : MonoBehaviour
{
    ColorPicker ColorPicker;

    const int NumLeds = 1;

    const string address = "192.168.1.4";
    const int port = 12100;

    int Step = 0;

    TcpClient Client = new TcpClient();

    Task ConnectTask;
    Task SendTask;

    SoftTimer Timer = new SoftTimer();


    string Message = "Hello!";

    byte[] Data = new byte[sizeof(uint) + NumLeds * 3];
    uint Preample = 0xAABBCCDD;



    NetworkStream Stream;



    // Start is called before the first frame update
    void Start()
    {
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
                            //Data = Encoding.ASCII.GetBytes(Message);
                            Timer.Start(1000);
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
                        if (Timer.Match())
                        {
                            Timer.Start(1000);

                            //Debug.Log(string.Format("Sending: {0}", Message));
                            Color32 color = ColorPicker.GetColor();

                            int n = 0;
                            Data[n++] = (byte)Preample;
                            Data[n++] = (byte)(Preample >> 8);
                            Data[n++] = (byte)(Preample >> 16);
                            Data[n++] = (byte)(Preample >> 24);
                            Data[n++] = color.r;
                            Data[n++] = color.g;
                            Data[n++] = color.b;

                            Debug.Log(string.Format("Sending color: {0}", color));
                            SendTask = Stream.WriteAsync(Data, 0, Data.Length);
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
            Client.Close();
            Timer.Start(5000);
            Step = 0;
            Debug.Log(ex.Message);
        }
    }
}
