using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;

public class SerialHandler_Stepping : MonoBehaviour
{
    public delegate void SerialDataReceivedEventHandler(string message);
    public event SerialDataReceivedEventHandler OnDataReceived;

    [SerializeField] private SerialManager_Stepping _serialManager_step;

    public string portName = "COM3";//ここにはArduinoのポート番号を記入
    int baudRate = 9600;

    private SerialPort serialPort_;
    private Thread thread_;
    private bool isRunning_ = false;

    private string message_;
    private bool isNewMessageReceived_ = false;

    public string readline;

    void Awake()
    {
        Open();
    }

    void Update()
    {
        if (isNewMessageReceived_)
        {
            OnDataReceived(message_);
        }
        isNewMessageReceived_ = false;
    }

    void OnDestroy()
    {
        Close();
    }

    private void Open()
    {
        serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        serialPort_.Open();

        serialPort_.ReadTimeout = 20;

        isRunning_ = true;

        thread_ = new Thread(Read);
        thread_.Start();
    }

    private void Close()
    {
        Write("0"); // 停止時にポンプが止まらなくなったので緊急停止用に追加
        isNewMessageReceived_ = false;
        isRunning_ = false;

        if (thread_ != null && thread_.IsAlive)
        {
            thread_.Join();
        }

        if (serialPort_ != null && serialPort_.IsOpen)
        {
            serialPort_.Close();
            serialPort_.Dispose();
        }
    }


    private void Read()
    {
        /*
        while (isRunning_ && serialPort_ != null && serialPort_.IsOpen)
        {
            try
            {
                message_ = serialPort_.ReadLine();
                isNewMessageReceived_ = true;
            }
            catch (System.Exception e)
            {
                readline = e.Message;
                Debug.LogWarning("1:" + readline);
            }
        }
        */
    }


    public void Write(string message)
    {
        try
        {
            serialPort_.Write(message);
            //_serialManager_step.cansend = true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("ステッピングモーター:" + e.Message);

        }
    }
}
