using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialManager_Bulb : MonoBehaviour
{
    public SerialHandler_Bulb serialHandler;
    public InvisibleWallManager _invisibleWallManager;


    //受信用変数
    private int data;              //受信データのfloat型版変数
    string receive_data;            //受信した生データを入れる変数

    //送信用変数
    bool onoff = true;              //オンオフどちらにするかを決定する変数（今回はオンで固定）
    bool cansend = true;            //送信するかどうかを判断する変数
    

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

        // マイコンからの信号を送る
        _invisibleWallManager.CheckMeltAllowed(data);
    }

    //どの壁を融かしているかを送信する
    public void SetMeltWallNumber(string number)
    {
        serialHandler.Write(number);
    }
}
