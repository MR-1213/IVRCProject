using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    [SerializeField] private GameObject cube;
    private float elapsedTime = 0f;
    private void Generate()
    {
        Instantiate(cube, new Vector3(0f, 1f, 1f), Quaternion.identity);
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > 10f)
        {
            elapsedTime = 0f;
            Generate();
        }
    }
}
