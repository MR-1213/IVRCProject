using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleWallManager : MonoBehaviour
{
    public string MeltWallNumber;
    [SerializeField] private SerialManager _serialManager;
    public Transform player;
    private Vector2 _allowedMovePositionXY;
    private float[] _allowedMovePositionRangeZ = new float[2];
    public bool IsMeltAllowed = false;

    private void Start()
    {
        // このオブジェクトのワールド座標を取得
        _allowedMovePositionRangeZ[0] = transform.position.z;
        //Debug.Log(_allowedMovePositionRangeZ[0]);
        _allowedMovePositionRangeZ[1] = _allowedMovePositionRangeZ[0] + 48.0f;
        //Debug.Log(_allowedMovePositionRangeZ[1]);
        _allowedMovePositionXY = new Vector2(transform.position.x, transform.position.y);

        //CheckMeltAllowed(1);
    }

    // private void Update() 
    // {
    //     // マイコンからの信号を受信する方に変更
    //     if(OVRInput.GetDown(OVRInput.Button.One))
    //     {
    //         // ここでMeltWallNumberをマイコンに送信？
    //         CheckMeltAllowed(1);
    //         Debug.Log("Melt Allowed");
    //     }
        
    //     if(OVRInput.GetUp(OVRInput.Button.One))
    //     {
    //         CheckMeltAllowed(0);
    //         Debug.Log("Melt Not Allowed");
    //     }
    // }

    public void CheckMeltAllowed(int signal)
    {
        if(signal == 1)
        {
            IsMeltAllowed = true;
            //_serialManager.SetMeltWallNumber(MeltWallNumber);
            this.GetComponent<MeshRenderer>().enabled = false;
            StartCoroutine(InvisibleWallMove());
        }
        else if(signal == 0)
        {
            IsMeltAllowed = false;
        }
    }

    private IEnumerator InvisibleWallMove()
    {
        while(true)
        {
            if(!IsMeltAllowed)
            {
                yield break;
            }

            // 右手のワールド座標を取得
            Vector3 playerPosition = player.position;
            // このオブジェクトのXYは固定、Zは右手のZ座標に合わせる(ただし、_allowedMovePositionRangeZの範囲内)
            Vector3 newPosition = new Vector3(_allowedMovePositionXY.x, _allowedMovePositionXY.y, Mathf.Clamp(player.position.z + 6.0f, _allowedMovePositionRangeZ[0], _allowedMovePositionRangeZ[1] + 15.0f));
            // このオブジェクトの座標を更新
            transform.position = newPosition;

            yield return null;
        }
    }
}
