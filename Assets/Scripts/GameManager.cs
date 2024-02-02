using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using GameInput;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CombatAction _dragonAction;// ���̃Q�[���̃{�X
    [SerializeField] private CombatAction _playerAction;// �v���C���[
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
            if (_dragonAction.IsDead)// �{�X�����񂾂�
                ClearGame();
            else if (_playerAction.IsDead)// �v���C���[�����񂾂�
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
