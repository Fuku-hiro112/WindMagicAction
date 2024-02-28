using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatsView : MonoBehaviour
{
    [Header("Canvas設定")]
    [SerializeField] private GameObject _healthCanvasPrefab;
    [SerializeField] private int _magnificationCanvasScale = 1;// Canvasの大きさ　何倍するか
    [SerializeField] private Vector3 _canvasPos = new Vector3(0, 2, 0); // Canvas の位置

    private GameObject _myCanvas; // 自身のCanvas
    private Image _imgHealth; // ヘルスバー
    private Text _txtHealth; // ヘルス文字
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
    /// Healthバーの更新
    /// </summary>
    /// <param name="health">現在のHP</param>
    /// <param name="maxHealth">最大HP</param>
    public void UpdateHealthBar(int health, int maxHealth)
    {
        _imgHealth.fillAmount = health / (float)maxHealth;
    }
    /// <summary>
    /// Healthテキストの更新
    /// </summary>
    /// <param name="health">現在のHP</param>
    /// <param name="maxHealth">最大HP</param>
    public void UpdateHealthText(int health, int maxHealth)
    {
        _txtHealth.text = health.ToString("f0") + "/" + maxHealth.ToString("f0");
    }
    /// <summary>
    /// キャンバスの向きを変える
    /// </summary>
    public void UpdateCanvasDirection()
    {
        _myCanvas.transform.forward = _camera.transform.forward;
    }
}
