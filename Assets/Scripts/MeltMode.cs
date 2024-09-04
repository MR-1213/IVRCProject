using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public class MeltMode : MonoBehaviour
{
    [SerializeField] private GamePlayManager _gamePlayManager;
    [SerializeField] private OVRPlayerController _playerController;
    [SerializeField] private SerialManager_Bulb _serialManager_Bulb;
    [SerializeField] private SerialManager_Stepping _serialManager_Stepping;
    [SerializeField] private SerialManager_RoadCell _serialManager_RoadCell;
    [SerializeField] private GameObject _cameraUICanvas;
    [SerializeField] private GameObject _invisibleWall;

    [SerializeField][Range(0f, 1f)] private float _meltability;
    public bool IsHeightChange = false;
    public float HeightChangeValue = 0f;


    public float meltDistance = 56f;
    private float _meltedDistance;

    private float _initialHeight;

    private Coroutine _waitCoroutine;
    private Coroutine _excecuteCoroutine;
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
        if(collider.gameObject.CompareTag("Player") && _excecuteCoroutine != null)
        {
            collider.enabled = false;
            _playerController.transform.position = new Vector3(_playerController.transform.position.x, _initialHeight + HeightChangeValue, _playerController.transform.position.z + 3f);
            StopCoroutine(_excecuteCoroutine);
            StartCoroutine(SteppingReverse());

            StartCoroutine(_gamePlayManager.GoToMeltPoint2());

            
            _playerController.gameObject.GetComponent<CharacterController>().enabled = true;
            _playerController.enabled = true;

            _serialManager_RoadCell.NextMeltMode();
        }
    }

    private IEnumerator SteppingReverse()
    {
        yield return null;
        _serialManager_Stepping.SetSyringeState("-1");
        Debug.Log("-1を送りました");
         yield return new WaitForSeconds(10f);
        _serialManager_Stepping.SetSyringeState("0");
        Debug.Log("0を送りました");
    }

    

    private IEnumerator WaitForStartMeltMode()
    {
        while(true)
        {
            if(OVRInput.GetDown(OVRInput.Button.Three))
            {
                Debug.Log("融かし始める");
                _excecuteCoroutine = StartCoroutine(MeltModeExcecute());
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator MeltModeExcecute()
    {
        _playerController.enabled = false;
        _playerController.transform.position = new Vector3(_currentMeltPoint.position.x, _playerController.transform.position.y, _currentMeltPoint.position.z);
        _initialHeight = _playerController.transform.position.y;

        float meltTime = 0f;
        float steppingTime = 0f;
        while (true)
        {
            Power = 0;
            yield return null;

            steppingTime += Time.deltaTime;
            if(steppingTime > 2f)
            {
                steppingTime = 0f;
            }

            if (Power >= 10 && Power < 300)
            {
                //var value = Random.Range(10, 100);
                //Power = value;
                _invisibleWall.GetComponent<MeshRenderer>().enabled = false;
                _invisibleWall.GetComponent<BoxCollider>().enabled = false;

                // バルブを開ける
                _serialManager_Bulb.SetMeltWallNumber("2");
                _serialManager_Stepping.SetSyringeState("1");

                while (true)
                {
                    if(Power > 300)
                    {
                        yield return null;
                        continue;
                    }

                    if(Power < 1)
                    {
                        _serialManager_Bulb.SetMeltWallNumber("0");
                        _serialManager_Stepping.SetSyringeState("0");
                        meltTime = 0f;
                        yield return null;
                        break;
                    }

                    meltTime += Time.deltaTime;
                    float progressSpeed = CalcProgressSpeed(meltTime);
                    //Debug.Log("進行速度: " + progressSpeed);

                    // 今どれくらいの距離が融けているかを計算
                    _meltedDistance += progressSpeed;
                    // 残りの距離を計算
                    var remainingDistance = meltDistance - _meltedDistance;

                    float height;
                    if (IsHeightChange)
                    {
                        height = Mathf.Lerp(0f, HeightChangeValue, _meltedDistance / meltDistance);
                    }
                    else
                    {
                        height = 0f;
                    }

                    var nextPos = new Vector3(_playerController.transform.position.x, _initialHeight + height, _playerController.transform.position.z + progressSpeed);
                    
                    //Debug.Log("残りの距離: " + remainingDistance);
                    _playerController.transform.position = nextPos;

                    yield return null;
                }
            }
            else
            {
                meltTime = 0f;
            }

            //yield return null;
        }
    }

    private float CalcProgressSpeed(float meltTime)
    {
        // 抵抗力0 ~ 1の範囲(0:強い, 1:弱い)
        float resistancePower = 1f - Mathf.Pow(1f - _meltability, meltTime);
        float progressSpeed = (Power / 1000f) * resistancePower;

        return progressSpeed;
    }

    public int Power { private get; set; } = 0;
}
