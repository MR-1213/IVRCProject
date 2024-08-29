using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInstantiate : MonoBehaviour
{
    [SerializeField] private GameObject _npcPrefab;
    [SerializeField] private GameObject[] _npcMovePoints = new GameObject[3];
    
    public int NPCCount;

    private void Start()
    {
        SetNPCNextPoint(1);
    }

    private void SetNPCNextPoint(int pointNum)
    {
        if(pointNum > 1)
        {
            _npcMovePoints[pointNum - 2].SetActive(false);
        }
        _npcMovePoints[pointNum - 1].SetActive(true);

        Transform _minPoint = _npcMovePoints[pointNum - 1].transform.GetChild(0);
        Transform _maxPoint = _npcMovePoints[pointNum - 1].transform.GetChild(1);
        for(int i = 0; i < NPCCount; i++)
        {
            Instantiate(_npcPrefab, 
                        new Vector3(Random.Range(_minPoint.position.x, _maxPoint.position.x), 0f, Random.Range(_minPoint.position.z, _maxPoint.position.z)), 
                        Quaternion.identity, 
                        this.transform);
        }
    }
}
