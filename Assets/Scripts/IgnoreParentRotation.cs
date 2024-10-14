using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreParentRotation : MonoBehaviour
{
    public float height = 150f;
    public bool IsIgnoreRotation = true;
    public Vector3 rotation = new Vector3(90f, -90f, 0f);
    
    private void Update()
    {
        transform.position = new Vector3(transform.parent.position.x, height, transform.parent.position.z);

        if(IsIgnoreRotation)
            transform.rotation = Quaternion.Euler(rotation);
        else
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
    }
}
