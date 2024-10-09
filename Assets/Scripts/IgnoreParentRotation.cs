using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreParentRotation : MonoBehaviour
{
    
    
    private void Update()
    {
        transform.position = new Vector3(transform.parent.position.x, 150f, transform.parent.position.z);
        transform.rotation = Quaternion.Euler(90f, -90f, 0f);
    }
}
