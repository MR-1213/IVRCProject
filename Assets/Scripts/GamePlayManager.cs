using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GamePlayManager : MonoBehaviour
{
    [SerializeField] private GameObject _cameraUICanvas;
    [SerializeField] private GameObject _screenFadeCanvas;
    [SerializeField] private AudioSource _SEAudioSource;
    public AudioClip SEAudioClip;
    [SerializeField] private AudioSource _BGMAudioSource;

    private TMP_Text _cameraUIText;
    private Transform _centerEyeAnchor;
    private float _centerEyeAnchorRotationThreshold;

    private string[] _gameExplanationTexts_1 =
    {
        "ここは名古屋駅桜通口付近です。\nそしてあなたは大事な会議に遅刻しそうです...！\n【トリガーボタンで次へ】",
        "向かうべきはあおなみ線の改札口です。\nしかし、ここからは遠く、多くの人で混雑する名古屋駅では間に合うか分かりません...\n【トリガーボタンで次へ】",
        "ここはあなたの持つものを融かす能力を使って、あおなみ線までの近道を作ってしまいましょう！\n【トリガーボタンで次へ】",
        "まずは名古屋駅構内に入り、<color=red>赤色</color>の目印まで向かいましょう！\n【トリガーボタンで次へ】",
        "『移動方法』左スティックで移動・回転ができます。メニューボタンは押さないように注意してください！\n【トリガーボタンで閉じる】",
    };
    [SerializeField] private GameObject _redMarker;

    private string[] _gameExplanationTexts_2 = 
    {
        "素晴らしいです！この調子であおなみ線を目指しましょう！\n【トリガーボタンで次へ】",
        "あおなみ線は左奥の緑色に光っている場所です！\n赤色の目印の壁を融かして進んでいきましょう！\n【トリガーボタンで閉じる】",
    };
    [SerializeField] private GameObject[] _aonamiStations;

    private void Start()
    {
        _cameraUICanvas.SetActive(true);
        _cameraUIText = _cameraUICanvas.transform.GetChild(0).GetComponent<TMP_Text>();
        _cameraUIText.text = _gameExplanationTexts_1[0];
        _cameraUIText.fontSize = 9;

        _centerEyeAnchor = _screenFadeCanvas.transform.parent;
        _centerEyeAnchorRotationThreshold = _centerEyeAnchor.localEulerAngles.y;

        StartCoroutine(GoToMeltPoint1());
    }

    private void Update() 
    {
        //Debug.Log("CenterEyeAnchor Rotation: " + _centerEyeAnchor.localEulerAngles.y);
        //if(_centerEyeAnchor.localEulerAngles.y > _centerEyeAnchorRotationThreshold + 100f || _centerEyeAnchor.localEulerAngles.y < 360f + _centerEyeAnchorRotationThreshold - 100f)
        //{
        //    _screenFadeCanvas.SetActive(true);
       // }
        //else
        //{
        ////    _screenFadeCanvas.SetActive(false);
        //}
    }

    public void StationBGMPlay(Collider collider)
    {
        if(collider.gameObject.CompareTag("Player"))
        {
            _BGMAudioSource.Play();
        }
    }

    private IEnumerator GoToMeltPoint1()
    {
        int textIndex = 1;
        while(true)
        {
            // Xボタンで会話ダイアログを進める
            if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                if(textIndex >= _gameExplanationTexts_1.Length)
                {
                    _cameraUICanvas.SetActive(false);
                    yield break;
                }

                if(_gameExplanationTexts_1[textIndex].Contains("赤色"))
                {
                    _redMarker.SetActive(true);
                }
                else
                {
                    _redMarker.SetActive(false);
                }

                _cameraUIText.text = _gameExplanationTexts_1[textIndex];
                _SEAudioSource.PlayOneShot(SEAudioClip);
                textIndex++;
            }

            yield return null;
        }
    }

    public void GoToMeltPoint2()
    {
        StartCoroutine(GoToMeltPoint2Coroutine());
    }

    private IEnumerator GoToMeltPoint2Coroutine()
    {
        int textIndex = 0;
        _cameraUICanvas.SetActive(true);
        _cameraUIText.text = _gameExplanationTexts_2[textIndex];
        textIndex++;
        while(true)
        {
            // Xボタンで会話ダイアログを進める
            if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                if(textIndex >= _gameExplanationTexts_2.Length)
                {
                    _cameraUICanvas.SetActive(false);
                    yield break;
                }

                if(_gameExplanationTexts_2[textIndex].Contains("緑色"))
                {
                    foreach(GameObject obj in _aonamiStations)
                    {
                        obj.layer = LayerMask.NameToLayer("WallHack");
                    }
                }
                else
                {
                    foreach(GameObject obj in _aonamiStations)
                    {
                        obj.layer = LayerMask.NameToLayer("Default");
                    }
                }

                _cameraUIText.text = _gameExplanationTexts_2[textIndex];
                _SEAudioSource.PlayOneShot(SEAudioClip);
                textIndex++;
            }

            yield return null;
        }
    }
}
