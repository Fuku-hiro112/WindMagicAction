using UnityEngine;
using UnityEngine.UI;

public class CombatAction : MonoBehaviour
{
    [System.NonSerialized] public bool IsDead; // 死亡の真偽値
    [System.NonSerialized] public int _health; // 現在のヘルス値
    [Header("Canvas設定")]
    [SerializeField] private GameObject _healthCanvasPrefab;
    [SerializeField] private int _magnificationCanvasScale = 1;// Canvasの大きさ　何倍するか
    [SerializeField] private Vector3 _canvasPos = new Vector3(0, 2, 0); // Canvas の位置
    [SerializeField] private int _maxHealth; // 自身の最大ヘルス値
    private GameObject _myCanvas; // 自身のCanvas
    private UnitBase _myUnit;
    private Image _imgHealth; // ヘルスバー
    private Text _txtHealth; // ヘルス文字

    private void Start()
    {
        // 自身のヘルスを表示
        _myCanvas = Instantiate(_healthCanvasPrefab);
        _myCanvas.transform.localScale *= _magnificationCanvasScale;
        _myCanvas.transform.SetParent(gameObject.transform); // Canvasを自身の子構造に
        _myCanvas.transform.position = transform.position + _canvasPos; // キャンバスの位置補正
        _myCanvas.transform.Find("imgHealth").TryGetComponent(out _imgHealth);
        _myCanvas.transform.Find("txtHealth").TryGetComponent(out _txtHealth);
        TryGetComponent(out _myUnit);
        Ready();
    }
    private void Update()
    {
        // ヘルスバーの増減と彩色
        _imgHealth.fillAmount = _health / (float)_maxHealth;
        if (_imgHealth.fillAmount > 0.5f)
        {
            _imgHealth.color = Color.green;
        }
        else if (_imgHealth.fillAmount > 0.2f)
        {
            _imgHealth.color = Color.yellow;
        }
        else
        {
            _imgHealth.color = Color.red;
        }
        // ヘルス値
        _txtHealth.text = _health.ToString("f0") + "/" + _maxHealth.ToString("f0");
        // キャンバスをカメラに向ける
        _myCanvas.transform.forward = Camera.main.transform.forward;
    }
    private void OnTriggerEnter(Collider other)
    {
        // 死んでなくて、タグWeaponが侵入した時
        if (other.gameObject.CompareTag("Weapon") && !IsDead)
        {
            // 武器の現在の攻撃力Powerを照会し、自身のヘルス値を減らす
            _health -= other.gameObject.GetComponent<WeaponAction>().Power;
            Debug.Log($"{this.gameObject.name}は、{other.gameObject.transform.root.name}から{other.gameObject.GetComponent<WeaponAction>().Power}ダメージを受けた");
            _health = Mathf.Clamp(_health, 0, _maxHealth); // ヘルス値を一定範囲内に収める
            if (_health <= 0.0f)
            { // 死亡判定
                IsDead = true; // 死亡を指定する
                _myUnit.StartCoroutine(_myUnit.OnDeath());
            }
            //ダメージエフェクト生成
            _myUnit.OnDamage();
        }
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Ready()
    {
        IsDead = false; // 死んでいない
        _health = _maxHealth; // ヘルス値を最大にする
        _imgHealth.fillAmount = 1;
    }
    /// <summary>
    /// HP変動時に最大値と最小値を超えないようにする。
    /// </summary>
    /// <param name="Value"></param>
    public void ChangeHealth(int Value)
    {
        _health = Mathf.Clamp(_health + Value, 0, _maxHealth);
    }
}
