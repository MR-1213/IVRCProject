using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialManager_Stepping : MonoBehaviour
{
    public SerialHandler_Stepping serialHandler;


    //��M�p�ϐ�
    private int data;              //��M�f�[�^��float�^�ŕϐ�
    string receive_data;            //��M�������f�[�^������ϐ�

    //���M�p�ϐ�
    bool onoff = true;              //�I���I�t�ǂ���ɂ��邩�����肷��ϐ��i����̓I���ŌŒ�j
    public bool cansend = true;            //���M���邩�ǂ����𔻒f����ϐ�


    private void Start()
    {
        serialHandler.OnDataReceived += OnDataReceived;
    }

    //�f�[�^����M������

    private void OnDataReceived(string message)
    {
        receive_data = (message);           //��M�f�[�^��receive_data�ɓ����
        data = int.Parse(receive_data);   //int�^�ɕϊ�����data�ɓ����
        Debug.Log("��M�f�[�^: " + data);
    }


    //���ˊ���������߂���(1: ����, -1:�߂�)
    public void SetSyringeState(string number)
    {
        //Debug.Log("cansend: " + cansend);
        serialHandler.Write(number);
        //cansend = false;
    }
}
