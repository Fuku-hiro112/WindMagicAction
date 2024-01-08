using UnityEngine;
using UnityEngine.UI;

public class CombatAction : MonoBehaviour
{
    GameObject _myCanvas; // 自身のCanvas
    Image _imgHealth; // ヘルスバー
    Text _txtHealth; // ヘルス文字
    [SerializeField] GameObject _healthCanvasPrefab;
    [SerializeField] Vector3 _canvasPos = new Vector3(0, 2, 0); // Canvas の位置
    [System.NonSerialized] public bool IsDead; // 死亡の真偽値
    [System.NonSerialized] int _health; // 現在のヘルス値
    [SerializeField] int _maxHealth; // 自身の最大ヘルス値

    void Start()
    {
        // 自身のヘルスを表示
        _myCanvas = Instantiate(_healthCanvasPrefab);
        _myCanvas.transform.SetParent(gameObject.transform); // Canvasを自身の子構造に
        _myCanvas.transform.position = transform.position + _canvasPos; // キャンバスの位置補正
        _myCanvas.transform.Find("imgHealth").TryGetComponent(out _imgHealth);
        _myCanvas.transform.Find("txtHealth").TryGetComponent(out _txtHealth);
        Ready();
    }
    void Update()
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

    public void Ready()
    {
        IsDead = false; // 死んでいない
        _health = _maxHealth; // ヘルス値を最大にする
        _imgHealth.fillAmount = 1;
    }
    public void ChangeHealth(int Value)
    {
        _health = Mathf.Clamp(_health + Value, 0, _maxHealth);
    }

    void OnTriggerEnter(Collider other)
    {
        // 死んでなくて、タグWeaponが侵入した時
        if (other.gameObject.tag == "Weapon" && !IsDead)
        {
            // 武器の現在の攻撃力Powerを照会し、自身のヘルス値を減らす
            _health -= other.gameObject.GetComponent<WeaponAction>().Power;
            _health = Mathf.Clamp(_health, 0, _maxHealth); // ヘルス値を一定範囲内に収める
            if (_health <= 0.0f)
            { // 死亡判定
                IsDead = true; // 死亡を指定する
                SendMessage("OnDeath", SendMessageOptions.DontRequireReceiver);
            }
            //ダメージエフェクト生成
            SendMessage("OnDamage", SendMessageOptions.DontRequireReceiver);
        }
    }
}
