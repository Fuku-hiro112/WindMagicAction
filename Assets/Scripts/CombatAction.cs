using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class CombatAction : MonoBehaviour
{
    [NonSerialized] public bool IsDead; // 死亡の真偽値
    [NonSerialized] public ReactiveProperty<int> ReactivePropertyHealth; // 現在のヘルス値

    [Header("Canvas設定")]
    [SerializeField] private GameObject _healthCanvasPrefab;
    [SerializeField] private int _magnificationCanvasScale = 1;// Canvasの大きさ　何倍するか
    [SerializeField] private Vector3 _canvasPos = new Vector3(0, 2, 0); // Canvas の位置
    [SerializeField] private int _maxHealth; // 自身の最大ヘルス値
    [SerializeField, Tooltip("無敵時間(秒)")] 
                     private float _invincibilityTimeSeconds = 1;

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
        Ready();// 初期化


        // Weaponタグに当たったらダメージをくらう
        this.OnTriggerEnterAsObservable()
            .Where(other => other.gameObject.CompareTag("Weapon") && !IsDead)
            .ThrottleFirst(TimeSpan.FromSeconds(_invincibilityTimeSeconds)) // ２回目以降,設定秒間攻撃処理を受け付けない (１回目は通過)
            .Subscribe(other => TriggerEnter(other));

        ReactivePropertyHealth
            .Subscribe(health => 
            {
                _imgHealth.fillAmount = health / (float)_maxHealth;
            }).AddTo(this);
        // Hpがなくなった時死亡処理を行う
        ReactivePropertyHealth.Where(helth => helth <= 0)// Hpが0以下になったら
            .Subscribe(_ =>
            {
                // 死亡処理
                IsDead = true; // 死亡を指定する
                _myUnit.OnDeath().Forget();
            },
            er => { Debug.Log("エラー"); }
            );

    }
    private void Update()
    {
        // ヘルスバーの増減と彩色
        //_imgHealth.fillAmount = ReactivePropertyHealth.Value / (float)_maxHealth;
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
        _txtHealth.text = ReactivePropertyHealth.Value.ToString("f0") + "/" + _maxHealth.ToString("f0");
        // キャンバスをカメラに向ける
        _myCanvas.transform.forward = Camera.main.transform.forward;
    }
    /*
    private void OnTriggerEnter(Collider other)
    {
        // 死んでなくて、タグWeaponが侵入した時
        if (other.gameObject.CompareTag("Weapon") && !IsDead)
        {
            // 武器の現在の攻撃力Powerを照会し、自身のヘルス値を減らす
            ReactivePropertyHealth.Value -= other.gameObject.GetComponent<WeaponAction>().Power;
            Debug.Log($"{this.gameObject.name}は、{other.gameObject.transform.root.name}から{other.gameObject.GetComponent<WeaponAction>().Power}ダメージを受けた");
            ReactivePropertyHealth.Value = Mathf.Clamp(ReactivePropertyHealth.Value, 0, _maxHealth); // ヘルス値を一定範囲内に収める
            //ダメージエフェクト生成
            _myUnit.OnDamage();

            if (ReactivePropertyHealth.Value <= 0.0f)
            { // 死亡判定
                IsDead = true; // 死亡を指定する
                _myUnit.OnDeath().Forget();
            }
        }
    }
    */
    private void TriggerEnter(Collider other)
    {
        // 武器の現在の攻撃力Powerを照会し、自身のヘルス値を減らす
        ReactivePropertyHealth.Value -= other.gameObject.GetComponent<WeaponAction>().Power;
        Debug.Log($"{this.gameObject.name}は、{other.gameObject.transform.root.name}から{other.gameObject.GetComponent<WeaponAction>().Power}ダメージを受けた");
        ReactivePropertyHealth.Value = Mathf.Clamp(ReactivePropertyHealth.Value, 0, _maxHealth); // ヘルス値を一定範囲内に収める
        
        //ダメージエフェクト生成　HACK: ダサいから点滅もさせたいな
        _myUnit.OnDamage();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Ready()
    {
        IsDead = false; // 死んでいない
        ReactivePropertyHealth = new ReactiveProperty<int>(_maxHealth); // ヘルス値を最大にする
        _imgHealth.fillAmount = 1;
    }
    /// <summary>
    /// HP変動時に最大値と最小値を超えないようにする。
    /// </summary>
    /// <param name="Value"></param>
    public void ChangeHealth(int Value)
    {
        ReactivePropertyHealth.Value = 
            Mathf.Clamp(ReactivePropertyHealth.Value + Value, 0, _maxHealth);// 0以上かつ、_maxHealthより上にならないように
    }
}
