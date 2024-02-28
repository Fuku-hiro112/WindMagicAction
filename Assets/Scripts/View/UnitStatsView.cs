using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatsView : MonoBehaviour
{
    [Header("Canvas�ݒ�")]
    [SerializeField] private GameObject _healthCanvasPrefab;
    [SerializeField] private int _magnificationCanvasScale = 1;// Canvas�̑傫���@���{���邩
    [SerializeField] private Vector3 _canvasPos = new Vector3(0, 2, 0); // Canvas �̈ʒu

    private GameObject _myCanvas; // ���g��Canvas
    private Image _imgHealth; // �w���X�o�[
    private Text _txtHealth; // �w���X����
    private Camera _camera;

    void Start()
    {
        _camera = Camera.main;
    }
    private void Update()
    {
        UpdateCanvasDirection();
    }
    /// <summary>
    /// Health�o�[�̍X�V
    /// </summary>
    /// <param name="health">���݂�HP</param>
    /// <param name="maxHealth">�ő�HP</param>
    public void UpdateHealthBar(int health, int maxHealth)
    {
        _imgHealth.fillAmount = health / (float)maxHealth;
    }
    /// <summary>
    /// Health�e�L�X�g�̍X�V
    /// </summary>
    /// <param name="health">���݂�HP</param>
    /// <param name="maxHealth">�ő�HP</param>
    public void UpdateHealthText(int health, int maxHealth)
    {
        _txtHealth.text = health.ToString("f0") + "/" + maxHealth.ToString("f0");
    }
    /// <summary>
    /// �L�����o�X�̌�����ς���
    /// </summary>
    public void UpdateCanvasDirection()
    {
        _myCanvas.transform.forward = _camera.transform.forward;
    }
}
