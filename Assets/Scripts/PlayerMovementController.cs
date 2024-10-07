using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private Transform _mainCamera;
    public float speed = 3.0f;
    public int angle = 30;

    private void Start()
    {
        var trackingPos = this.transform.GetChild(0).localPosition;
        this.transform.GetChild(0).localPosition = new Vector3(trackingPos.x, trackingPos.y - 1.5f, trackingPos.z);
        _mainCamera = Camera.main.transform;
    }

    private void Update()
    {
        ChangeDirection();
        Move();
    }

    //OVRCameraRigの角度変更
    void ChangeDirection()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickLeft))
        {
            this.transform.Rotate(0,-angle, 0);
        }
        else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickRight))
        {
            this.transform.Rotate(0, angle, 0);
        }
    }

    void Move()
    {
        //右ジョイスティックの情報取得
        Vector2 stickL = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        //OVRCameraRigの位置変更
        var nextPos = _mainCamera.rotation * (new Vector3(0f, 0f, (stickL.y * speed * Time.deltaTime)));
        nextPos.y = 0f;
        this.transform.position += nextPos;
    }
}
