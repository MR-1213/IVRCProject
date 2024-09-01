using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialManager_RoadCell : MonoBehaviour
{
    public SerialHandler_RoadCell serialHandler;
    [SerializeField] private MeltMode _meltMode;

    //受信用変数
    private int data;              //受信データのfloat型版変数
    private string receive_data;            //受信した生データを入れる変数

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
        Debug.Log("受信した圧力値: " + data);

        // マイコンからの信号を送る
        _meltMode.Power = data;
    }

    // public void SetMeltWallNumber(string number)
    // {
    //     if (cansend == true)            //送信可能かチェック
    //     {
    //         serialHandler.Write(number);   //Arduinoに1を送信
    //         Debug.Log("1を送信");
    //         cansend = false;            //オフになるまで送信不可に設定
    //     }
    // }
}
