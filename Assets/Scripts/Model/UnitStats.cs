using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Unit
{
    public class UnitStats : MonoBehaviour
    {
        [NonSerialized] public bool IsDead; // 死亡の真偽値
        private ReactiveProperty<int> _health = new ReactiveProperty<int>(); // 現在のヘルス値
        public IReadOnlyReactiveProperty<int> Health => _health;


        [Header("Canvas設定")]
        [SerializeField] private GameObject _healthCanvasPrefab;
        [SerializeField] private int _magnificationCanvasScale = 1;// Canvasの大きさ　何倍するか
        [SerializeField] private Vector3 _canvasPos = new Vector3(0, 2, 0); // Canvas の位置
        [SerializeField] private int _maxHealth; // 自身の最大ヘルス値
        [SerializeField, Tooltip("無敵時間(秒)")]
        private float _invincibilityTimeSeconds = 1;

        [NonSerialized] public GameObject MyCanvas; // 自身のCanvas
        private UnitBase _myUnit;
        private Image _imgHealth; // ヘルスバー
        private Text _txtHealth; // ヘルス文字
        private Camera _camera;//NOTE: Camera.mainで取るとShake中カメラの切り替えでバグるので

        private void Awake()
        {
            MyCanvas = Instantiate(_healthCanvasPrefab);
        }
        private void Start()
        {
            // 自身のヘルスを表示
            MyCanvas.transform.localScale *= _magnificationCanvasScale;
            MyCanvas.transform.SetParent(gameObject.transform); // Canvasを自身の子構造に
            MyCanvas.transform.position = transform.position + _canvasPos; // キャンバスの位置補正
            MyCanvas.transform.Find("imgHealth").TryGetComponent(out _imgHealth);
            MyCanvas.transform.Find("txtHealth").TryGetComponent(out _txtHealth);
            TryGetComponent(out _myUnit);
            _camera = Camera.main;
            Ready();// 初期化


            // Weaponタグに当たったらダメージをくらう
            this.OnTriggerEnterAsObservable()
                .Where(other => other.gameObject.CompareTag("Weapon") && !IsDead)
                .ThrottleFirst(TimeSpan.FromSeconds(_invincibilityTimeSeconds)) // ２回目以降,設定秒間攻撃処理を受け付けない (１回目は通過)
                .Subscribe(other => TriggerEnter(other));

            // オブジェクトストリーム停止
            _health.AddTo(this);
            // View healthBarの更新
            _health.Subscribe(health =>
                {
                    // HPBarの変更
                    UpdateHealthBar(health);
                    // HPテキストの変更
                    UpdateHealthText(health);
                    if (gameObject.name != "Dragon")
                        // HPBarの色変更
                        ChangeHealthImageColor();
                });

            // Hpがなくなった時死亡処理を行う
            _health.Where(helth => helth <= 0)// Hpが0以下になったら
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
            // キャンバスをカメラに向ける
            MyCanvas.transform.forward = _camera.transform.forward;
        }
        /// <summary>
        /// HP画像を現在のHP割合で変更する
        /// </summary>
        private void ChangeHealthImageColor()
        {
            if (_imgHealth.fillAmount > 0.5f) _imgHealth.color = Color.green;
            else if (_imgHealth.fillAmount > 0.2f) _imgHealth.color = Color.yellow;
            else _imgHealth.color = Color.red;
        }
        /// <summary>
        /// HPBarの更新
        /// </summary>
        /// <param name="health">現在のHP</param>
        private void UpdateHealthBar(int health)
        {
            _imgHealth.fillAmount = health / (float)_maxHealth;
        }
        /// <summary>
        /// HPテキストの更新
        /// </summary>
        /// <param name="health">現在のHP</param>
        private void UpdateHealthText(int health)
        {
            _txtHealth.text = health.ToString("f0") + "/" + _maxHealth.ToString("f0");
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
            _health.Value -= other.gameObject.GetComponent<WeaponAction>().Power;
            Debug.Log($"{this.gameObject.name}は、{other.gameObject.transform.root.name}から{other.gameObject.GetComponent<WeaponAction>().Power}ダメージを受けた");
            _health.Value = Mathf.Clamp(_health.Value, 0, _maxHealth); // ヘルス値を一定範囲内に収める

            //ダメージ処理（エフェクトやバイブなど）　HACK: ダサいから点滅もさせたいな
            _myUnit.OnDamage();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Ready()
        {
            IsDead = false; // 死んでいない
            _health.Value = _maxHealth; // ヘルス値を最大にする
            _imgHealth.fillAmount = 1;
        }
        /// <summary>
        /// HP変動時に最大値と最小値を超えないようにする。
        /// </summary>
        /// <param name="Value"></param>
        public void ChangeHealth(int Value)
        {
            _health.Value =
                Mathf.Clamp(_health.Value + Value, 0, _maxHealth);// 0以上かつ、_maxHealthより上にならないように
        }
    }
}
