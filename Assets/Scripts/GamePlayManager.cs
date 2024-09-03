using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GamePlayManager : MonoBehaviour
{
    [SerializeField] private GameObject _cameraUICanvas;
    [SerializeField] private GameObject _screenFadeCanvas;

    private TMP_Text _cameraUIText;
    private Transform _centerEyeAnchor;
    private float _centerEyeAnchorRotationThreshold;

    private string[] _conversationalTexts =
    {
        "ここは名古屋駅桜通口付近です。\nそしてあなたは大事な会議に遅刻しそうです...！\n【Xボタンで次へ】",
        "向かうべきはあおなみ線の改札口です。\nまずは名古屋駅構内に入り、金時計広場を目指しましょう。\n【Xボタンで次へ】",
        "『移動方法』左スティックで移動・回転ができます。メニューボタンは押さないように注意してください！【Xボタンで次へ】",
    };

    private void Start()
    {
        _cameraUICanvas.SetActive(true);
        _cameraUIText = _cameraUICanvas.transform.GetChild(0).GetComponent<TMP_Text>();
        _cameraUIText.text = _conversationalTexts[0];
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

    private IEnumerator GoToMeltPoint1()
    {
        int textIndex = 1;
        while(true)
        {
            // Xボタンで会話ダイアログを進める
            if(OVRInput.GetDown(OVRInput.Button.Three))
            {
                if(textIndex >= _conversationalTexts.Length)
                {
                    _cameraUICanvas.SetActive(false);
                    yield break;
                }

                _cameraUIText.text = _conversationalTexts[textIndex];
                textIndex++;
            }

            yield return null;
        }
    }
}
