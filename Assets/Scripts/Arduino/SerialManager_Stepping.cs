using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialManager_Stepping : MonoBehaviour
{
    public SerialHandler_Stepping serialHandler;


    //受信用変数
    private int data;              //受信データのfloat型版変数
    string receive_data;            //受信した生データを入れる変数

    //送信用変数
    bool onoff = true;              //オンオフどちらにするかを決定する変数（今回はオンで固定）
    public bool cansend = true;            //送信するかどうかを判断する変数


    private void Start()
    {
        serialHandler.OnDataReceived += OnDataReceived;
    }

    //データを受信したら

    private void OnDataReceived(string message)
    {
        receive_data = (message);           //受信データをreceive_dataに入れる
        data = int.Parse(receive_data);   //int型に変換してdataに入れる
        Debug.Log("受信データ: " + data);
    }


    //注射器を押すか戻すか(1: 押す, -1:戻す)
    public void SetSyringeState(string number)
    {
        //Debug.Log("cansend: " + cansend);
        serialHandler.Write(number);
        //cansend = false;
    }
}
