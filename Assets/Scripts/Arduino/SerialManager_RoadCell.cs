using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialManager_RoadCell : MonoBehaviour
{
    public SerialHandler_RoadCell serialHandler;
    [SerializeField] private MeltMode[] _meltModes = new MeltMode[2];

    private MeltMode _meltMode;
    private bool _isModeChanging = false;
    private int index = 0;

    //受信用変数
    private int data;              //受信データのfloat型版変数
    private string receive_data;            //受信した生データを入れる変数

    private void Start()
    {
        serialHandler.OnDataReceived += OnDataReceived;
        _meltMode = _meltModes[index];
    }

    //データを受信したら
    private void OnDataReceived(string message)
    {
        if(_isModeChanging) return;

        receive_data = message; //受信データをreceive_dataに入れる
        data = int.Parse(receive_data);   //int型に変換してdataに入れる
        //Debug.Log("受信した圧力値: " + data);

        // マイコンからの信号を送る
        _meltMode.Power = data;
    }

    public void NextMeltMode()
    {
        _isModeChanging = true;
        if(index == 3)
        {
            index = -1;
        }
        _meltMode = _meltModes[index + 1];
        index++;
        _isModeChanging = false;
    }
}
