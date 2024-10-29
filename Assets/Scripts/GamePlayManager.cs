using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GamePlayManager : MonoBehaviour
{
    // 管理下スクリプト
    [SerializeField] private ScenarioEventManager _scenarioEventManager;
    [SerializeField] private WallAnchorManager _wallAnchorManager;
    [SerializeField] private NPCInstantiate _npcInstantiate;

    // BGM関連
    [SerializeField] 
    private AudioSource _BGMAudioSource;
    public AudioClip StationBGM;
    public AudioClip MeltBGM;

    // タイマー関連
    [Header("タイマー(s)")]
    public float _timer = 300f;
    private float _elapsedTime = 0f;
    private bool _isGameStart = false;
    [SerializeField] private Text _timerText;

    private int pointNum = 1;
    private bool _isScenarioEventCompleted = false;

    private void Start()
    {
        _isScenarioEventCompleted = false;
        _scenarioEventManager.StartIntroduction(ScenarioEventCompleted);
    }

    private void Update()
    {
        if(_isGameStart)
        {
            _elapsedTime += Time.deltaTime;
            if(_elapsedTime >= _timer)
            {
                Debug.Log("時間切れ！");
                _timerText.text = "時間切れ！";
                _isGameStart = false;
                return;
            }

            float remainingTime = _timer - _elapsedTime;
            int minute = (int)remainingTime / 60;
            int second = (int)remainingTime % 60;

            if(minute != 0)
            {
                _timerText.text = $"残り{minute}分{second}秒";
            }
            else
            {
                _timerText.text = $"残り{second}秒";
            }
        }
    }

    #region シナリオイベント関連
    // シナリオイベントのコールバック
    private void ScenarioEventCompleted()
    {
        Debug.Log("シナリオイベント完了");
        _isScenarioEventCompleted = true;
    }

    public void CancelScenarioEvent()
    {
        // シナリオイベントが完了していない場合はその時点で終了
        if(!_isScenarioEventCompleted)
        {
            _scenarioEventManager.CancelScenarioEvent();
            _isScenarioEventCompleted = true;
        }
    }

    /// <summary>
    /// 駅構内入口の透明コライダーから呼ばれるイベント
    /// シナリオイベントを中断し、駅構内BGMを再生する
    /// </summary>
    /// <param name="collider"></param>
    public void EnterStation(Collider collider)
    {
        if(!collider.gameObject.CompareTag("Player"))
        {
            return;
        }

        _isGameStart = true;

        CancelScenarioEvent();

        StationBGMPlay();
    }
    
    /// <summary>
    /// 最終目的地を表示し、チュートリアルを完了する
    /// </summary>
    public void TutorialComplete()
    {
        _isScenarioEventCompleted = false;
        _scenarioEventManager.StartTutorialCompleted();
    }

    /// <summary>
    /// ゲームを終了する
    /// </summary>
    public void GameEnding()
    {
        _isScenarioEventCompleted = false;
        _scenarioEventManager.StartGameCompleted();
    }
    #endregion

    #region 融かすモード関連

    /// <summary>
    /// 融かすポイントの赤色目印に入った時のイベント
    /// 
    /// </summary>
    /// <param name="collider"></param>
    public void EnterMeltMode(Collider collider)
    {
        var _meltMode = collider.transform.parent.GetComponent<MeltMode>();
        _meltMode.CurrentMeltPoint = collider.transform;
        _meltMode.ChangeMeltModeState(MeltMode.MeltModeStateEnum.WaitForMelting);

        _scenarioEventManager.StartMeltModeMessage();

        _wallAnchorManager.AnchorsVisibility();
    }

    /// <summary>
    /// 融かすポイントの赤色目印から出た時のイベント
    /// UIメッセージを非表示にする
    /// </summary>
    public void ExitMeltMode(Collider collider)
    {
        _scenarioEventManager.CancelScenarioEvent();
        Debug.Log(collider.gameObject.name);
        var _meltMode = collider.transform.parent.GetComponent<MeltMode>();
        if(_meltMode.MeltModeState == MeltMode.MeltModeStateEnum.MeltExcecute)
        {
            return;
        }

        _wallAnchorManager.AnchorsInvisibility();
    }

    /// <summary>
    /// 融かしきった時のイベント
    /// </summary>
    /// <param name="collider"></param>
    public void MeltEndEnter(Collider collider)
    {
        _wallAnchorManager.AnchorsInvisibility();

        var _meltMode = collider.transform.parent.GetComponent<MeltMode>();
        if(_meltMode.MeltModeState == MeltMode.MeltModeStateEnum.Idle)
        {
            return;
        }
        
        _meltMode.ChangeMeltModeState(MeltMode.MeltModeStateEnum.MeltEnd);
    }

    /// <summary>
    /// 融かしきった後、融かしポイントから出た時のイベント
    /// </summary>
    /// <param name="collider"></param>
    public void MeltEndExit(Collider collider)
    {
        var _meltMode = collider.transform.parent.GetComponent<MeltMode>();
        if(_meltMode.MeltModeState == MeltMode.MeltModeStateEnum.Idle)
        {
            return;
        }

        _meltMode.ChangeMeltModeState(MeltMode.MeltModeStateEnum.Idle);

        StationBGMPlay();
        _wallAnchorManager.Inactive_FollowAnchorToRightHand();

        _npcInstantiate.SetNPCNextPoint(pointNum);
    }

    public void SetNPCNextPointNum(int pointNum)
    {
        this.pointNum = pointNum;
    }

    #endregion

    #region BGM関連
    public void StationBGMPlay()
    {
        _BGMAudioSource.Stop();
        _BGMAudioSource.clip = StationBGM;
        _BGMAudioSource.Play();
    }

    public void MeltBGMPlay()
    {
        _BGMAudioSource.Stop();
        _BGMAudioSource.clip = MeltBGM;
        _BGMAudioSource.Play();
    }

    public void BGMStop()
    {
        _BGMAudioSource.Stop();
    }
    #endregion
}
