using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using GameInput;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CombatAction _dragonAction;// このゲームのボス
    [SerializeField] private CombatAction _playerAction;// プレイヤー
    [SerializeField] private Canvas _clearCanvas;　　　 
    [SerializeField] private Canvas _gameOverCanvas;

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
                SceneManager.LoadScene("TitleScene");
            }
        }
        if (_isCanvasActive)
        {
            if (_dragonAction.IsDead)// ボスが死んだら
                ClearGame();
            else if (_playerAction.IsDead)// プレイヤーが死んだら
                OverGame();
        }
    }
    /// <summary>
    /// ゲームクリア時処理
    /// </summary>
    private void ClearGame()
    {
        _isGameFinish = true;
        _isCanvasActive = false;
        _clearCanvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// ゲーム終了時処理
    /// </summary>
    private void OverGame()
    {
        _gameOverCanvas.gameObject.SetActive(true);
        _isGameFinish = true;
        _isCanvasActive = false;
    }
}
