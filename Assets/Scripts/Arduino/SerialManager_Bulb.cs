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
    

    void Start()
    {
        serialHandler.OnDataReceived += OnDataReceived;
    }

    //データを受信したら
    void OnDataReceived(string message)
    {
        receive_data = (message);           //受信データをreceive_dataに入れる
        data = int.Parse(receive_data);   //int型に変換してdataに入れる
        Debug.Log("受信データ: " + data);

        // マイコンからの信号を送る
        _invisibleWallManager.CheckMeltAllowed(data);
    }

    // private void Update()
    // {
    //     if (onoff)
    //     {
    //         usendmsg();     //オン用メソッド呼び出し
    //     }
    //     else
    //     {
    //         dsendmsg();     //オフ用メソッド呼び出し
    //     }
    // }

    //どの壁を融かしているかを送信する
    public void SetMeltWallNumber(string number)
    {
        serialHandler.Write(number);   //Arduinoに1を送信
        //cansend = false;
    }

    // //オフ用メソッド
    // void dsendmsg()
    // {
    //     if (cansend == false)           //送信可能かチェック
    //     {
    //         serialHandler.Write("0");   //Arduinoに0を送信
    //         Debug.Log("0を送信");
    //         cansend = true;             //オフになるまで送信不可に設定
    //     }
    // }
}
