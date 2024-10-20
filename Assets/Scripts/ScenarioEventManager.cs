using System.Collections;
using UnityEngine;
using TMPro;
using System;

#nullable enable

public class ScenarioEventManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textMesh; // 会話ダイアログを表示するTextUI
    [SerializeField]
    private AudioSource _seAudioSource; // SE用のAudioSource
    [SerializeField]
    private AudioClip _feedSE;

    [SerializeField]
    private GameObject _meltPointMarker;
    [SerializeField]
    private GameObject[] _goalMarkers;

    private bool _isTutorialCompletedCalled = false;

    [SerializeField]
    [TextArea]
    private string _introductionText = string.Join("\n", new string[] {
        "ここは名古屋駅桜通口付近です。",
        "そしてあなたは大事な会議に遅刻しそうです...！",
        "",
        "向かうべきはあおなみ線の改札口です。",
        "しかし、ここからは遠く、多くの人で混雑する名古屋駅では間に合うか分かりません...",
        "ここはあなたの持つものを融かす能力を使って、あおなみ線までの近道を作ってしまいましょう！",
        "",
        "まずは名古屋駅構内に入り、<color=red>赤色</color>の目印まで向かいましょう！",
        "",
        "『移動方法』左スティックで移動・回転ができます。メニューボタンは押さないように注意してください！",
        "",
    });

    [SerializeField]
    [TextArea]
    private string _tutorialCompletedText = string.Join("\n", new string[] {
        "素晴らしいです！この調子であおなみ線を目指しましょう！",
        "",
        "あおなみ線は左奥の<color=green>緑色</color>に光っている場所です！",
        "赤色の目印の壁を融かして進んでいきましょう！",
    });

    [SerializeField]
    [TextArea]
    private string _gameCompletedText = string.Join("\n", new string[] {
        "おめでとうございます！",
        "あなたは無事にあおなみ線の改札口まで到着しました！",
        "",
        "体験いただきありがとうございました。",
        "ヘッドセットを外してください。",
    });

    private Coroutine? _currentScenarioCoroutine = null;

    public void StartIntroduction(Action? callback = null)
    {
        _currentScenarioCoroutine = StartCoroutine(PlayScenarioEvent(_introductionText, callback));
    }

    public void StartTutorialCompleted(Action? callback = null)
    {
        _isTutorialCompletedCalled = true;
        _currentScenarioCoroutine = StartCoroutine(PlayScenarioEvent(_tutorialCompletedText, callback));
    }

    public void StartGameCompleted(Action? callback = null)
    {
        _currentScenarioCoroutine = StartCoroutine(PlayScenarioEvent(_gameCompletedText, callback));
    }

    private IEnumerator PlayScenarioEvent(string text, Action? callback)
    {
        // TODO: キャンセル処理(コルーチンのキャンセル手法を考える or UniTask+CancellationTokenを使う)

        // テキストUIを表示
        _textMesh.gameObject.SetActive(true);
        _textMesh.fontSize = 9;
        var isFirst = true;
        var meltPointMarkerEnabled = false;
        foreach (var line in text.Split('\n'))
        {
            Debug.Log(line);
            if (line != string.Empty)
            {
                // 融かすポイントの強調テキストに合わせてオブジェクトを表示する
                if (line.Contains("<color=red>"))
                {
                    meltPointMarkerEnabled = true;
                    _meltPointMarker.SetActive(true);
                }
                else if (meltPointMarkerEnabled)
                {
                    meltPointMarkerEnabled = false;
                    _meltPointMarker.SetActive(false);
                }

                // ゴールを強調表示する
                if (line.Contains("<color=green>"))
                {
                    foreach (var item in _goalMarkers)
                    {
                        item.layer = LayerMask.NameToLayer("WallHack");
                    }
                }

                _textMesh.text += line;
                continue;
            }

            // 文字送りを始める
            _textMesh.text += "\n【トリガーボタンで次へ】";
            // 前フレームの入力が残っていないか確認
            if (!isFirst && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                yield return null;
            }
            // 入力を待機
            while (!OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                yield return null;
            }
            Debug.Log("入力を受け付けました");
            if (isFirst)
            {
                isFirst = false;
            }

            // テキストをクリア
            _textMesh.text = "";
            // SEを再生
            _seAudioSource.PlayOneShot(_feedSE);
        }

        if (meltPointMarkerEnabled)
        {
            _meltPointMarker.SetActive(false);
        }

        // テキストUIを非表示
        _textMesh.gameObject.SetActive(false);
        // コルーチン終了・コールバック起動
        _currentScenarioCoroutine = null;
        callback?.Invoke();
    }

    public void CancelScenarioEvent()
    {
        if (_currentScenarioCoroutine != null)
        {
            StopCoroutine(_currentScenarioCoroutine);
            if(_isTutorialCompletedCalled)
            {
                _isTutorialCompletedCalled = false;
                foreach (var item in _goalMarkers)
                {
                    item.layer = LayerMask.NameToLayer("WallHack");
                }
            }
        }
        _textMesh.gameObject.SetActive(false);
        _meltPointMarker.SetActive(false);
    }

    public void StartMeltModeMessage()
    {
        // テキストUIを表示
        _textMesh.gameObject.SetActive(true);
        _textMesh.color = Color.white;
        _textMesh.fontSize = 9;

        _textMesh.text = "壁と対面し、トリガーボタンを押して、\n壁を融かし始めよう!";
    }

    public void PushWallMessage()
    {
        // テキストUIを表示
        _textMesh.gameObject.SetActive(true);
        _textMesh.color = Color.white;
        _textMesh.fontSize = 9;

        _textMesh.text = "壁を押して融かせ！";
    }
    
}
