using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using GameInput;
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
    private bool _isCanvasActive = true;// キャンバス（クリア、終了時の）

    void Start()
    {
        // キャンバスを非表示にする
        _clearCanvas.gameObject.SetActive(false);
        _gameOverCanvas.gameObject.SetActive(false);
    }
    void Update()
    {
        if (_isGameFinish)// ゲームが終了したか
        {
            if (ConfirmAction.s_Instance.InputAction.Player.Decision.WasPerformedThisFrame())
            {
                AudioManager.Instance.StopBGM(true);
                AudioManager.Instance.PlaySE(SESoundData.SE.Decision);

                _clearCanvas.gameObject.SetActive(false);
                _gameOverCanvas.gameObject.SetActive(false);
                _panel.DOColor(Color.black, _duration)
                      .OnComplete(() => SceneManager.LoadScene("TitleScene"));
            }
        }
        if (_isCanvasActive)
        {
            if (_dragonStats.IsDead)// ボスが死んだら
                ClearGame();
            else if (_playerStats.IsDead)// プレイヤーが死んだら
                OverGame();
        }
    }
    /// <summary>
    /// ゲームクリア時処理
    /// </summary>
    private void ClearGame()
    {
        AudioManager.Instance.PlaySE(SESoundData.SE.GameClear);

        _isGameFinish = true;
        _isCanvasActive = false;
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
        _isCanvasActive = false;
        _gameOverCanvas.gameObject.SetActive(true);
    }
}
