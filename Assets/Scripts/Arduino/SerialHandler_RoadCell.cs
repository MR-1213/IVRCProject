using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;

public class SerialHandler_RoadCell : MonoBehaviour
{
    public delegate void SerialDataReceivedEventHandler(string message);
    public event SerialDataReceivedEventHandler OnDataReceived = delegate { };
    public string portName = "COM3";//ここにはArduinoのポート番号を記入


    private int baudRate = 9600;
    private SerialPort _serialPort;
    private Thread _thread;
    private bool _isRunning = false;
    private string _message;
    private bool _isNewMessageReceived = false;

    public string readline;

    private void Awake()
    {
        Open();
    }

    private void Update()
    {
        if (_isNewMessageReceived)
        {
            OnDataReceived(_message);
        }
        _isNewMessageReceived = false;
    }

    

    private void Open()
    {
        _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        _serialPort.Open();

        _serialPort.ReadTimeout = 350;

        _isRunning = true;

        _thread = new Thread(Read);
        _thread.Start();
    }

    private void Close()
    {
        //Write("0"); //！！！今回のコードではこの行がないと実行終了時にLEDが消えないので注意！！！
        _isNewMessageReceived = false;
        _isRunning = false;

        if (_thread != null && _thread.IsAlive)
        {
            _thread.Join();
        }

        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
            _serialPort.Dispose();
        }
    }

    private void Read()
    {
        
        while (_isRunning && _serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                _message = _serialPort.ReadLine();
                _isNewMessageReceived = true;
            }
            catch (System.Exception e)
            {
                readline = e.Message;
                Debug.LogWarning("1:" + readline);
            }
        }
        
    }

    public void Write(string message)
    {
        try
        {
            _serialPort.Write("2:" + message);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("3:" + e.Message);

        }
    }

    private void OnDestroy()
    {
        Close();
    }
}
