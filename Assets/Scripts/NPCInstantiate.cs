using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInstantiate : MonoBehaviour
{
    [SerializeField] private GameObject _npcPrefab;
    [SerializeField] private Transform _minPoint;
    [SerializeField] private Transform _maxPoint;
    public int NPCCount;

    private void Start()
    {
        for(int i = 0; i < NPCCount; i++)
        {
            Instantiate(_npcPrefab, new Vector3(Random.Range(_minPoint.position.x, _maxPoint.position.x), 0f, Random.Range(_minPoint.position.z, _maxPoint.position.z)), Quaternion.identity);
        }
    }
}
