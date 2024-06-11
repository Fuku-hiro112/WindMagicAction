using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using GameInput;
using UniRx;
using UniRx.Triggers;
using UnityEngine.SceneManagement;
using Unit;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UnitStats _dragonStats;// このゲームのボス
    [SerializeField] private UnitStats _playerStats;// プレイヤー
    [SerializeField] private Canvas _clearCanvas;　　　 
    [SerializeField] private Canvas _gameOverCanvas;
    [SerializeField] private Image _panel;

    private int _duration = 1;
    private bool _isGameFinish = false; // ゲームが終了している？

    void Start()
    {
        // キャンバスを非表示にする
        _clearCanvas.gameObject.SetActive(false);
        _gameOverCanvas.gameObject.SetActive(false);


        // 以下UpdateAsObservable()
        // ゲームが終了した後、決定ボタンが押されたら,1度だけ実行
        this.UpdateAsObservable()
            .Where(_ => _isGameFinish)
            .First(_ => ConfirmAction.s_Instance.InputAction.Player.Decision.WasPerformedThisFrame())
            .Subscribe(_ =>
            {
                AudioManager.Instance.StopBGM(true);
                AudioManager.Instance.PlaySE(SESoundData.SE.Decision);

                _clearCanvas.gameObject.SetActive(false);
                _gameOverCanvas.gameObject.SetActive(false);
                _panel.DOColor(Color.black, _duration)
                      .OnComplete(() => SceneManager.LoadScene("TitleScene"));
            }).AddTo(this);

        // ゲーム中、ボスとプレイヤーの生死を監視
        this.UpdateAsObservable()
            .Where(_ => !_isGameFinish)
            .First(_ => _dragonStats.IsDead || _playerStats.IsDead)//NOTE: 先にやられた方を優先するため
            .Subscribe(_ =>
            {
                // ボスが死んだら
                if (_dragonStats.IsDead) ClearGame();
                // プレイヤーが死んだら
                if (_playerStats.IsDead) OverGame();

            }).AddTo(this);

    }

    /// <summary>
    /// ゲームクリア時処理
    /// </summary>
    private void ClearGame()
    {
        AudioManager.Instance.PlaySE(SESoundData.SE.GameClear);

        _isGameFinish = true;
        _clearCanvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// ゲーム終了時処理
    /// </summary>
    private void OverGame()
    {
        AudioManager.Instance.PlaySE(SESoundData.SE.GameOver);
        AudioManager.Instance.StopBGM(true);

        _isGameFinish = true;
        _gameOverCanvas.gameObject.SetActive(true);
    }
}
