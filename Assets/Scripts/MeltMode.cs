using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.Formats.Alembic.Importer;
using UnityEditor;

public class MeltMode : MonoBehaviour
{
    [SerializeField] private GamePlayManager _gamePlayManager;
    [SerializeField] private PlayerMovementController _playerController;
    [SerializeField] private SerialManager_Bulb _serialManager_Bulb;
    [SerializeField] private SerialManager_Stepping _serialManager_Stepping;
    [SerializeField] private SerialManager_RoadCell _serialManager_RoadCell;
    [SerializeField] private NPCInstantiate _npcInstantiate;
    [SerializeField] private GameObject _cameraUICanvas;
    [SerializeField] private GameObject _invisibleWall;

    [SerializeField] private GameObject _meltWall;
    private AlembicStreamPlayer _alembicPlayer;

    [SerializeField][Range(0f, 1f)] private float _meltability;
    public float endStepForwardX = 0f;
    public float endStepForwardZ = 0f;
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
        _alembicPlayer = _meltWall.GetComponent<AlembicStreamPlayer>();
        _alembicPlayer.CurrentTime = 0f;

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
            _playerController.transform.position = new Vector3(_playerController.transform.position.x - endStepForwardX, _initialHeight + HeightChangeValue, _playerController.transform.position.z + endStepForwardZ);
            StopCoroutine(_excecuteCoroutine);
            StartCoroutine(SteppingReverse());

            //StartCoroutine(_gamePlayManager.GoToMeltPoint2());

            
            //_playerController.gameObject.GetComponent<CharacterController>().enabled = true;
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
                _npcInstantiate.SetNPCNextPoint(GetNPCNextPointIndex());
                yield break;
            }

            yield return null;
        }
    }

    private int GetNPCNextPointIndex()
    {
        if(_meltWall.gameObject.name == "melt3_1")
        {
            return 2;
        }
        else if(_meltWall.gameObject.name == "melt3_2")
        {
            return 3;
        }
        else if(_meltWall.gameObject.name == "melt3_3")
        {
            return 4;
        }
        return default;
    }

    private IEnumerator MeltModeExcecute()
    {
        _playerController.enabled = false;
        _playerController.transform.position = new Vector3(_currentMeltPoint.position.x, _playerController.transform.position.y, _currentMeltPoint.position.z);
        _initialHeight = _playerController.transform.position.y;

        float meltTime = 0f;
        float meltingRatio = 0.01666f;
        while (true)
        {
            Power = 0;
            //Debug.Log("受信した圧力値: " + Power);
            yield return null;

            if(OVRInput.GetDown(OVRInput.Button.Four))
            //if (Power >= 10 && Power < 300)
            {
                Power = 100;
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

                    if(OVRInput.GetUp(OVRInput.Button.Four))
                    //if(Power < 1)
                    {
                        _serialManager_Bulb.SetMeltWallNumber("0");
                        _serialManager_Stepping.SetSyringeState("0");
                        meltTime = 0f;
                        yield return null;
                        break;
                    }

                    meltTime += Time.deltaTime;
                    float progressSpeed = CalcProgressSpeed(meltTime);

                    float speedX, speedZ;
                    if(_meltWall.gameObject.name == "melt3_3")
                    {
                        speedX = progressSpeed;
                        speedZ = 0f;
                        _meltedDistance += speedX;
                    }
                    else if(_meltWall.gameObject.name == "melt3_4")
                    {
                        speedX = progressSpeed;
                        speedZ = progressSpeed + 0.027f;
                        _meltedDistance += Mathf.Sqrt(Mathf.Pow(speedX, 2) + Mathf.Pow(speedZ, 2));
                    }
                    else 
                    {
                        speedX = 0f;
                        speedZ = progressSpeed;
                        _meltedDistance += speedZ;
                    }
                    

                    float height;
                    if (IsHeightChange)
                    {
                        height = HeightChangeValue;
                    }
                    else
                    {
                        height = 0f;
                    }

                    var nextPos = new Vector3(_playerController.transform.position.x - speedX, _initialHeight + height, _playerController.transform.position.z + speedZ);
                    _playerController.transform.position = nextPos;

                    float currentTime= Mathf.Lerp(0.01666f, 10f, (_meltedDistance + 3.5f) / meltDistance);
                    if(currentTime >= 10f)
                    {
                        currentTime = 10f;
                    }
                    _alembicPlayer.CurrentTime = currentTime;

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
