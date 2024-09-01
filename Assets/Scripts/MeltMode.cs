using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public class MeltMode : MonoBehaviour
{
    [SerializeField] private OVRPlayerController _playerController;
    [SerializeField] private GameObject _cameraUICanvas;
    [SerializeField] private GameObject _invisibleWall;
    [SerializeField] private GameObject _invisbleWall2;

    [SerializeField][Range(0f, 1f)] private float _meltability;
    private float[] meltDistance = new float[4] {56f, 56f, 56f, 56f};
    private float _meltedDistance;

    private Coroutine _waitCoroutine;
    private Transform _currentMeltPoint;
    private TMP_Text _cameraUIText;

    private void Start()
    {
        _cameraUIText = _cameraUICanvas.transform.GetChild(0).GetComponent<TMP_Text>();
    }

    public void UICanvasEnable(Collider collider)
    {
        if(collider.gameObject.CompareTag("Player"))
        {
            _cameraUIText.text = "Xボタン(左コントローラー)を押して\n壁を融かし始めよう!";
            _cameraUICanvas.SetActive(true);
            _currentMeltPoint = collider.transform;
            _waitCoroutine = StartCoroutine(WaitForStartMeltMode());
        }
    }

    public void UICanvasDisable(Collider collider)
    {
        if(collider.gameObject.CompareTag("Player") && _waitCoroutine != null)
        {
            _cameraUICanvas.SetActive(false);
            StopCoroutine(_waitCoroutine);
        }
    }

    public void MeltEnd(Collider collider)
    {
        if(collider.gameObject.CompareTag("Player"))
        {
            _invisbleWall2.GetComponent<BoxCollider>().enabled = true;
            _playerController.enabled = true;
        }
    }

    private IEnumerator WaitForStartMeltMode()
    {
        while(true)
        {
            if(OVRInput.GetDown(OVRInput.Button.Three))
            {
                Debug.Log("融かし始める");
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
        float meltTime = 0f;
        while (true)
        {
            if(OVRInput.GetDown(OVRInput.Button.Four))
            {
                var value = Random.Range(10, 100);
                Power = value;
                _invisibleWall.GetComponent<MeshRenderer>().enabled = false;
                _invisibleWall.GetComponent<BoxCollider>().enabled = false;
                while(true)
                {
                    if(OVRInput.GetUp(OVRInput.Button.Four))
                    {
                        meltTime = 0f;
                        break;
                    }

                    meltTime += Time.deltaTime;
                    float progressSpeed = CalcProgressSpeed(meltTime);
                    Debug.Log("進行速度: " + progressSpeed);

                    var nextPos = new Vector3(_playerController.transform.position.x, _playerController.transform.position.y, _playerController.transform.position.z + progressSpeed);
                    // 今どれくらいの距離が融けているかを計算
                    _meltedDistance += progressSpeed;
                    // 残りの距離を計算
                    var remainingDistance = meltDistance[0] - _meltedDistance;
                    Debug.Log("残りの距離: " + remainingDistance);
                    _playerController.transform.position = nextPos;

                    yield return null;
                }
            }
            else
            {
                meltTime = 0f;
            }

            yield return null;
        }
    }

    private float CalcProgressSpeed(float meltTime)
    {
        // 抵抗力0 ~ 1の範囲(0:強い, 1:弱い)
        float resistancePower = 1f - Mathf.Pow(1f - _meltability, meltTime);
        float progressSpeed = (Power / 2000f) * resistancePower;

        return progressSpeed;
    }

    public int Power { private get; set; }
}
