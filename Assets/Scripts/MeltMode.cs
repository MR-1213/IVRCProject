using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeltMode : MonoBehaviour
{
    [SerializeField] private OVRPlayerController _playerController;
    [SerializeField] private GameObject _uiCanvas;
    [SerializeField] private GameObject _invisibleWall;

    private Coroutine _waitCoroutine;
    private Transform _currentMeltPoint;

    public void UICanvasEnable(Collider collider)
    {
        if(collider.gameObject.CompareTag("Player"))
        {
            _uiCanvas.SetActive(true);
            _currentMeltPoint = collider.transform;
            _waitCoroutine = StartCoroutine(WaitForStartMeltMode());
        }
    }

    public void UICanvasDisable(Collider collider)
    {
        if(collider.gameObject.CompareTag("Player") && _waitCoroutine != null)
        {
            _uiCanvas.SetActive(false);
            StopCoroutine(_waitCoroutine);
        }
    }

    private IEnumerator WaitForStartMeltMode()
    {
        while(true)
        {
            if(OVRInput.GetDown(OVRInput.Button.Three))
            {
                Debug.Log("—Z‚©‚µŽn‚ß‚é");
                StartCoroutine(MeltModeExcecute());
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator MeltModeExcecute()
    {
        _playerController.enabled = false;
        _playerController.transform.position = new Vector3(_currentMeltPoint.position.x, 4f, _currentMeltPoint.position.z);
        while (true)
        {
            if(OVRInput.Get(OVRInput.Button.Four))
            {
                _invisibleWall.GetComponent<MeshRenderer>().enabled = false;
                var nextPos = new Vector3(_playerController.transform.position.x, _playerController.transform.position.y, _playerController.transform.position.z + 0.1f);
                _playerController.transform.position = nextPos;
            }

            yield return null;
        }
    }
}
