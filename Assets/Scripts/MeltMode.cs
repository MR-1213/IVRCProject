using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.Formats.Alembic.Importer;
using UnityEditor;
using DG.Tweening;
using NUnit.Framework.Internal;
using UnityEngine.Rendering.Universal.Internal;

public class MeltMode : MonoBehaviour
{
    [SerializeField] private GamePlayManager _gamePlayManager;
    [SerializeField] private PlayerMovementController _playerController;
    [SerializeField] private SerialManager_Bulb _serialManager_Bulb;
    [SerializeField] private SerialManager_Stepping _serialManager_Stepping;
    [SerializeField] private SerialManager_RoadCell _serialManager_RoadCell;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private GameObject _cameraUICanvas;
    [SerializeField] private GameObject _invisibleWall;

    [SerializeField] private Transform _mainCamera;
    [SerializeField] private SkinnedMeshRenderer _rightHand;
    [SerializeField] private Material _lavaMaterial;
    [SerializeField] private Material _normalMaterial;
    [SerializeField] private GameObject _meltWall;
    private AlembicStreamPlayer _alembicPlayer;
    private Material _meltWallMaterial;

    [SerializeField][Range(0f, 1f)] private float _meltability;
    public float endStepForwardX = 0f;
    public float endStepForwardZ = 0f;
    public bool IsHeightChange = false;
    public float HeightChangeValue = 0f;


    public float meltDistance = 56f;
    public float reverseTime = 10f;

    public bool IsMeltByController;
    private float _meltedDistance;

    private float _initialHeight;

    private Rigidbody _playerRigidbody;
    private Coroutine _waitCoroutine;
    private Coroutine _excecuteCoroutine;
    private Transform _currentMeltPoint;
    private TMP_Text _cameraUIText;

    private Transform _startPoint;
    public Transform _endPoint;

    private readonly object _lock = new object();

    private void Start()
    {
        _cameraUIText = _cameraUICanvas.transform.GetChild(0).GetComponent<TMP_Text>();
        _alembicPlayer = _meltWall.GetComponent<AlembicStreamPlayer>();
        _alembicPlayer.CurrentTime = 0f;
        _meltWallMaterial = _meltWall.GetComponentInChildren<MeshRenderer>().material;
        _playerRigidbody = _playerController.GetComponent<Rigidbody>();
        //StartCoroutine(Test());
    }

    private IEnumerator Test()
    {
        _serialManager_Stepping.SetSyringeState("1");
        yield return new WaitForSeconds(10f);
        _serialManager_Stepping.SetSyringeState("0");
    }

    public void UICanvasEnable(Collider collider)
    {
        // if(_startPoint == null)
        // {
        //     _startPoint = collider.gameObject.transform;
        // }
        if(collider.gameObject.CompareTag("Player"))
        {
            _cameraUIText.color = Color.white;
            _cameraUIText.text = "壁と対面し、トリガーボタンを押して\n壁を融かし始めよう!";
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
            Power = 0;
            StartCoroutine(MeltComplete(_meltWallMaterial.GetFloat("_Current"), _alembicPlayer.CurrentTime));
            _playerController.transform.position = new Vector3(_playerController.transform.position.x - endStepForwardX, _initialHeight + HeightChangeValue, _playerController.transform.position.z + endStepForwardZ);
            _playerRigidbody.useGravity = true;
            //_rightHand.material = _normalMaterial;
            StopCoroutine(_excecuteCoroutine);
            StartCoroutine(SteppingReverse());

            _playerController.enabled = true;

            _serialManager_RoadCell.NextMeltMode();
        }
    }

    private IEnumerator MeltComplete(float M_StartValue, float ABC_StartValue)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOVirtual.Float(M_StartValue, 1f, 2.0f, value => _meltWallMaterial.SetFloat("_Current", value))).Join(DOVirtual.Float(ABC_StartValue, 10f, 2.0f, value => _alembicPlayer.CurrentTime = value));
        yield return sequence.Play().WaitForCompletion();
    }

    private IEnumerator SteppingReverse()
    {
        yield return null;
        _serialManager_Stepping.SetSyringeState("-1");
        Debug.Log("-1を送りました");
        yield return new WaitForSeconds(reverseTime);
        _serialManager_Stepping.SetSyringeState("0");
        Debug.Log("0を送りました");
        Power = 0;
    }

    

    private IEnumerator WaitForStartMeltMode()
    {
        while(true)
        {
            if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                Debug.Log("融かし始める");
                _playerRigidbody.useGravity = false;
                // var direction = _endPoint.position - _startPoint.position;
                // Vector3 angle = _mainCamera.localEulerAngles;
                // angle.y = direction.y;
                // _mainCamera.localEulerAngles = angle;
                //_rightHand.sharedMaterial = _lavaMaterial;
                _cameraUIText.text = "壁を押して融かせ！";
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
        Power = 0;
        while (true)
        {
            
            //Debug.Log("受信した圧力値: " + Power);
            //yield return null;

            //if(OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
            if ((Power >= 3 && Power < 1000))
            {
                Debug.Log($"入った！！{Power}");
                //_particleSystem.Play();   
                Power = 100;
                _cameraUICanvas.SetActive(false);
                _invisibleWall.GetComponent<BoxCollider>().enabled = false;
                _gamePlayManager.MeltBGMPlay();

                // バルブを開ける
                _serialManager_Bulb.SetMeltWallNumber("2");
                _serialManager_Stepping.SetSyringeState("1");

                while (true)
                {
                    if(Power > 1000)
                    {
                        yield return null;
                        continue;
                    }

                    if(OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger))
                    //if(Power < 1)
                    {
                        //_particleSystem.Stop();
                        _serialManager_Bulb.SetMeltWallNumber("0");
                        _serialManager_Stepping.SetSyringeState("0");
                        _gamePlayManager.BGMStop();
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
                        speedZ = progressSpeed * 1.66f;
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

                    float currentTime= Mathf.Lerp(0.01666f, 10f, (_meltedDistance + 4.0f) / meltDistance);
                    if(currentTime >= 10f)
                    {
                        currentTime = 10f;
                    }
                    _alembicPlayer.CurrentTime = currentTime;
                    _meltWallMaterial.SetFloat("_Current", currentTime / 10f);
                    if(currentTime / 10f < (1f / 15f))
                    {
                        // 0~1/15, 0~3000
                        float heatValue = (3000f * 15f) * (currentTime / 10f);
                        //Debug.Log(heatValue);
                        _meltWallMaterial.SetFloat("_Heat", heatValue);
                    }
                    else
                    {
                        _meltWallMaterial.SetFloat("_Heat", 3000f);
                    }
                    

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

    private int _power;
    public int Power
    {
        private get
        {
            lock (_lock)
            {
                //Debug.Log("Get: " + _power);
                return _power;
            }

        }
        set
        {
            lock (_lock)
            {
                Debug.Log("Set: " + value);
                _power = value;
            }
        }
    }
}
