using System.Collections;
using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;
using DG.Tweening;
using System;

public class MeltMode : MonoBehaviour
{
    [SerializeField] private GamePlayManager _gamePlayManager;
    [SerializeField] private ScenarioEventManager _scenarioEventManager;
    [SerializeField] private PlayerMovementController _playerMovementController;
    [SerializeField] private WallAnchorManager _wallAnchorManager;
    [SerializeField] private SerialManager_Bulb _serialManager_Bulb;
    [SerializeField] private SerialManager_Stepping _serialManager_Stepping;
    [SerializeField] private SerialManager_RoadCell _serialManager_RoadCell;

    [SerializeField] private ParticleSystem particleSystem;

    public enum MeltModeStateEnum
    {
        Idle,
        WaitForMelting,
        MeltExcecute,
        MeltEnd,
    }

    public MeltModeStateEnum MeltModeState { get; private set; }
    private bool _stateEnter = false;
    [HideInInspector] public Transform CurrentMeltPoint;
    [SerializeField] private GameObject _meltWall;
    [SerializeField] private GameObject _invisibleWall;
    private AlembicStreamPlayer _alembicPlayer;
    private Material _meltWallMaterial;

    [SerializeField][Range(0f, 1f)] private float _meltability;
    public float endStepForwardX = 0f;
    public float endStepForwardZ = 0f;
    public bool IsHeightChange = false;
    public float HeightChangeStart = 0f;
    public float HeightChangeEnd = 0f;

    private float meltTime = 0f;
    public float meltDistance = 56f;
    public float reverseTime = 10f;
    private float _meltedDistance;

    private float _initialHeight;

    private Rigidbody _playerRigidbody;
    private Coroutine _excecuteCoroutine;
    private bool _meltEnter = false;

    [SerializeField] private Transform _meltStartPoint;
    [SerializeField] private Transform _meltEndPoint;
    [SerializeField] private OVRCameraRig _ovrCameraRig;

    private readonly object _lock = new object();

    private void Start()
    {
        _alembicPlayer = _meltWall.GetComponent<AlembicStreamPlayer>();
        _alembicPlayer.CurrentTime = 0f;
        _meltWallMaterial = _meltWall.GetComponentInChildren<MeshRenderer>().material;
        _playerRigidbody = _playerMovementController.GetComponent<Rigidbody>();

        ChangeMeltModeState(MeltModeStateEnum.Idle);
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
                if (value != _power)
                {
                    Debug.Log("Set: " + value);
                    _power = value;
                }
            }
        }
    }

    public void ChangeMeltModeState(MeltModeStateEnum state)
    {
        MeltModeState = state;
        _stateEnter = true;
    }

    private void Update()
    {
        switch(MeltModeState)
        {
            case MeltModeStateEnum.Idle:
            {
                // 何もしない
                return;
            }
            case MeltModeStateEnum.WaitForMelting:
            {
                WaitForStartMeltMode();
                return;
            }
            case MeltModeStateEnum.MeltExcecute:
            {
                if(_stateEnter)
                {
                    _stateEnter = false;

                    _playerMovementController.enabled = false;
                    _playerMovementController.transform.position = new Vector3(CurrentMeltPoint.position.x, _playerMovementController.transform.position.y, CurrentMeltPoint.position.z);
                    _initialHeight = _playerMovementController.transform.position.y;

                    

                    meltTime = 0f;
                    Power = 0;
                }
                MeltModeExcecute(meltTime);
                return;
            }
            case MeltModeStateEnum.MeltEnd:
            {
                if(_stateEnter)
                {
                    _stateEnter = false;
                    Power = 0;
                    StartCoroutine(MeltComplete(_meltWallMaterial.GetFloat("_Current"), _alembicPlayer.CurrentTime));
                    _playerMovementController.transform.position = new Vector3(_playerMovementController.transform.position.x - endStepForwardX, _initialHeight + HeightChangeEnd, _playerMovementController.transform.position.z + endStepForwardZ);
                    _playerRigidbody.useGravity = true;
                    
                    StartCoroutine(SteppingReverse());

                    _playerMovementController.enabled = true;

                    _serialManager_RoadCell.NextMeltMode();
                }
                return;
            }
            default:
            {
                Debug.LogError("MeltModeStateEnumの値が不正です");
                return;
            }
        }
    }

    
    #region 融かす準備待ち
    private void WaitForStartMeltMode()
    {
        //if(/OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        if(Power >= 30 && Power < 10000)
        {
            // トリガーボタンを押したら融かすモードを開始
            Debug.Log("WaitForMeltMode");
            _playerRigidbody.useGravity = false;
            LookAtWall();

            _scenarioEventManager.PushWallMessage();

            _wallAnchorManager.SetAnchorInstance();

            ChangeMeltModeState(MeltModeStateEnum.MeltExcecute);
        }
    }

    private void LookAtWall()
    {
        float currentRotY = _ovrCameraRig.centerEyeAnchor.eulerAngles.y;
        Vector3 lookDirection = _meltEndPoint.position - _meltStartPoint.position;
        float targetRot = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;

        float difference = targetRot - currentRotY;
        _ovrCameraRig.transform.Rotate(0f, difference, 0f);

        particleSystem.transform.rotation = Quaternion.LookRotation(-lookDirection.normalized, Vector3.up);

        Vector3 currentPosition = _ovrCameraRig.centerEyeAnchor.localPosition;
        _ovrCameraRig.trackingSpace.localPosition = new Vector3(OffsetValue(currentPosition.x), -1.5f, OffsetValue(currentPosition.z));
    }

    private float OffsetValue(float value)
    {
        if(value != 0)
        {
            value *= -1f;
        }
        return value;
    }
    #endregion

    #region 融かすモード実行
    private void MeltModeExcecute(float meltTime)
    {
        //if(OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        if ((Power >= 30 && Power < 10000) && !_meltEnter)
        {
            // シナリオUIを終了し、非表示にする
            _scenarioEventManager.CancelScenarioEvent();
            _invisibleWall.GetComponent<BoxCollider>().enabled = false;

            //Power = 300;
            Debug.Log($"融かすモード開始 Power => {Power}");

            // 融ける音を再生
            _gamePlayManager.MeltBGMPlay();

            //アンカーを手に追従させる
            _wallAnchorManager.Active_FollowAnchorToRightHand();
            
            // バルブを開ける
            _serialManager_Bulb.SetMeltWallNumber("2");
            _serialManager_Stepping.SetSyringeState("1");

            _meltEnter = true;

            if (!particleSystem.isPlaying) { particleSystem.Play(); }
        }
        //else if(OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger))
        else if(Power < 30)
        {
            _meltEnter = false;
            meltTime = 0f;

            _serialManager_Bulb.SetMeltWallNumber("0");
            _serialManager_Stepping.SetSyringeState("0");

            _gamePlayManager.BGMStop();

            _wallAnchorManager.Inactive_FollowAnchorToRightHand();

            if (particleSystem.isPlaying) { particleSystem.Stop(); }
        }
        else if(_meltEnter)
        {
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
                float t = (_meltedDistance / meltDistance);
                height = Mathf.Lerp(HeightChangeStart, HeightChangeEnd, t);
            }
            else
            {
                height = 0f;
            }

            var nextPos = new Vector3(_playerMovementController.transform.position.x - speedX, _initialHeight + height, _playerMovementController.transform.position.z + speedZ);
            _playerMovementController.transform.position = nextPos;

            float current = (_meltedDistance + 4.0f) / meltDistance;
            float currentTime = current * _alembicPlayer.Duration;

            if (_meltWall.TryGetComponent<MeltWall>(out var wall))
            {
                // NOTE: アニメーションの挙動が意図したものと異なっていたため変更。高さが変わるのはMeltWall2だけであるためこれを条件にする
                wall.Current = current;
                // _playerMovementController.transform.position = wall.CurrentPosition;
                return;
            }
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
        }
        else
        {
            meltTime = 0f;
            if (particleSystem.isPlaying) { particleSystem.Stop(); }
        }
    }

    private float CalcProgressSpeed(float meltTime)
    {
        // 抵抗力0 ~ 1の範囲(0:強い, 1:弱い)
        float resistancePower = 1f - Mathf.Pow(1f - _meltability, meltTime);
        float progressSpeed = (Power / 150f) * resistancePower;

        return progressSpeed;
    }
    #endregion

    #region 融かすモード終了
    private IEnumerator MeltComplete(float M_StartValue, float ABC_StartValue)
    {
        if (particleSystem.isPlaying) { particleSystem.Stop(); }
        _serialManager_Bulb.SetMeltWallNumber("0");
        _serialManager_Stepping.SetSyringeState("0");
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
    #endregion
}
