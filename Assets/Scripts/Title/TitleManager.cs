using GameInput;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private Image _imgTitle;
    [SerializeField] private Text _txtButton;
    [SerializeField] private int _duration = 1;
    private void Awake()
    {
        _imgTitle.color = Color.black;
        _txtButton.color = Color.clear;
    }
    void Start()
    {
        AudioManager.Instance.PlayBGM(BGMSoundData.BGM.Play, true);

        // Fadeしてテキスト、ボタンを表示
        _imgTitle.DOColor(Color.white, _duration);
        _txtButton.DOColor(Color.white, _duration);

        // textの点滅
        Sequence sequenceButton = DOTween.Sequence();
        sequenceButton.Append(_txtButton.DOFade(1, _duration))
                      .SetLoops(-1, LoopType.Yoyo);

        this.UpdateAsObservable()// 決定ボタンが押されたら,1度だけ実行
            .First(_ => ConfirmAction.s_Instance.InputAction.Player.Decision.WasPerformedThisFrame())
            .Subscribe(_ => 
            {
                AudioManager.Instance.PlaySE(SESoundData.SE.Decision);

                sequenceButton.Kill();
                // 画面が黒になり終わったらシーン転換
                Sequence sequence = DOTween.Sequence();
                sequence.Append(_imgTitle.DOColor(Color.black, _duration))
                        .Join  (_txtButton.DOColor(Color.black, _duration))
                        .OnComplete(() => SceneManager.LoadScene("GameScene"));// GameSceneへ
            });
    }
}
