using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using GameInput;
using UnityEngine.SceneManagement;
using Unit;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UnitStats _dragonStats;// ���̃Q�[���̃{�X
    [SerializeField] private UnitStats _playerStats;// �v���C���[
    [SerializeField] private Canvas _clearCanvas;�@�@�@ 
    [SerializeField] private Canvas _gameOverCanvas;

    private bool _isGameFinish = false; // �Q�[�����I�����Ă���H
    private bool _isCanvasActive = true;// �L�����o�X�i�N���A�A�I�����́j

    void Start()
    {
        // �L�����o�X���\���ɂ���
        _clearCanvas.gameObject.SetActive(false);
        _gameOverCanvas.gameObject.SetActive(false);
    }
    void Update()
    {
        if (_isGameFinish)// �Q�[�����I��������
        {
            if (ConfirmAction.s_Instance.InputAction.Player.Decision.WasPerformedThisFrame())
            {
                SceneManager.LoadScene("TitleScene");
            }
        }
        if (_isCanvasActive)
        {
            if (_dragonStats.IsDead)// �{�X�����񂾂�
                ClearGame();
            else if (_playerStats.IsDead)// �v���C���[�����񂾂�
                OverGame();
        }
    }
    /// <summary>
    /// �Q�[���N���A������
    /// </summary>
    private void ClearGame()
    {
        _isGameFinish = true;
        _isCanvasActive = false;
        _clearCanvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// �Q�[���I��������
    /// </summary>
    private void OverGame()
    {
        _gameOverCanvas.gameObject.SetActive(true);
        _isGameFinish = true;
        _isCanvasActive = false;
    }
}
