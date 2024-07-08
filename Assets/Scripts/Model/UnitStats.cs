using DG.Tweening;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Unit
{
    [RequireComponent(typeof(UnitBase))]
    public class UnitStats : MonoBehaviour // Unitの状態を保持している
    {
        [Header("Canvas設定")]
        [SerializeField] private GameObject _healthCanvasPrefab;
        [SerializeField] private int _magnificationCanvasScale = 1;// Canvasの大きさ　何倍するか
        [SerializeField] private Vector3 _canvasPos = new Vector3(0, 2, 0); // Canvas の位置
        [SerializeField] private int _maxHealth; // 自身の最大ヘルス値
        [SerializeField] private float _decreaseDuration = 0.5f;
        [SerializeField, Tooltip("無敵時間(秒)")]
        private float _invincibilityTimeSeconds = 0.2f;

        [NonSerialized] public bool IsDead = false; // 死亡の真偽値
        [NonSerialized] public GameObject MyCanvas; // 自身のCanvas
        private UnitBase _myUnit;
        private Image _imgHealth; // ヘルスバー
        private Image _imgDamage;
        private Text _txtHealth; // ヘルス文字
        private Camera _camera;//NOTE: Camera.mainで取るとShake中カメラの切り替えでバグるので
        
        
        private ReactiveProperty<int> _health = new ReactiveProperty<int>(); // 現在のヘルス値
        public Transform TargetDisplayPosition;
        public IReadOnlyReactiveProperty<int> Health => _health;
        public int MaxHealth => _maxHealth;

        private void Awake()
        {
            MyCanvas = Instantiate(_healthCanvasPrefab);
            Assert.IsNotNull(MyCanvas, "MyCanvasがNullです。");
        }
        private void Start()
        {
            // 自身のヘルスを表示
            MyCanvas.transform.localScale *= _magnificationCanvasScale;
            MyCanvas.transform.SetParent(gameObject.transform); // Canvasを自身の子構造に
            MyCanvas.transform.position = transform.position + _canvasPos; // キャンバスの位置補正

            MyCanvas.transform.Find("imgHealth").TryGetComponent(out _imgHealth);
            MyCanvas.transform.Find("imgDamage").TryGetComponent(out _imgDamage);
            MyCanvas.transform.Find("txtHealth").TryGetComponent(out _txtHealth);
            TryGetComponent(out _myUnit);
            Assert.IsNotNull(_imgHealth);
            Assert.IsNotNull(_imgDamage);
            Assert.IsNotNull(_txtHealth);
            Assert.IsNotNull(_myUnit);
            _camera = Camera.main;
            Assert.IsNotNull(_camera, "CameraがNullです");
            Ready();// 初期化

            // ドラゴンでないEnemyなら
            if (gameObject.CompareTag("Enemy") && gameObject.name != "Dragon")
            {
                // キャンバスをカメラの方に向ける
                this.UpdateAsObservable()
                    .Subscribe(_ => MyCanvas.transform.forward = _camera.transform.forward);
            }

            // オブジェクトストリーム停止
            _health.AddTo(this);
            // View healthBarの更新
            _health.Subscribe(health =>
                {
                    float currentFillAmount = health / (float)_maxHealth;
                    // HPBarの変更
                    UpdateBarFillAmount(currentFillAmount, _imgHealth, _imgDamage);

                    // HPテキストの変更
                    UpdateBarText(_txtHealth, _maxHealth, health);

                    // ボス（ドラゴン）で無いなら
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
                    _myUnit.OnDeathAsync().Forget();
                },
                er => { Debug.Log("エラー"); }
                );

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
        /// ImageのFillAmount値を滑らかに更新
        /// </summary>
        /// <param name="currentFillAmount">現在満たされている割合</param>
        /// <param name="frontImage">手前の減らしたい画像</param>
        /// <param name="backImage">後ろの減らしたい画像</param>
        public void UpdateBarFillAmount(float currentFillAmount, Image frontImage, Image backImage)
        {
            // frontを徐々に減らす
            frontImage
                .DOFillAmount(currentFillAmount, _decreaseDuration)
                .OnComplete(() =>
                {
                    float waitTime = 0.5f;
                    // 0.5秒待ってから徐々に減らす
                    backImage
                        .DOFillAmount(currentFillAmount, _decreaseDuration / 2)
                        .SetDelay(waitTime);
                });
        }
        /// <summary>
        /// Barのテキストを更新
        /// </summary>
        /// <param name="barText">変更したいText</param>
        /// <param name="maxValue">最大の値</param>
        /// <param name="currentValue">現在の値</param>
        public void UpdateBarText(Text barText, int maxValue, int currentValue)
        {
            barText.text = currentValue.ToString("f0") + "/" + maxValue.ToString("f0");
        }

        /// <summary>
        /// ダメージ処理
        /// </summary>
        /// <param name="power">ダメージの力の値</param>
        public void OnDamage(int power)
        {
            // 武器の現在の攻撃力Powerを照会し、自身のヘルス値を減らす
            ChangeHealth(- power);
            //ダメージ処理（エフェクトやバイブなど）　HACK: ダサいから点滅もさせたいな
            _myUnit.VisualizationDamege();
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
        /// <param name="Value">HPを変動させる値</param>
        public void ChangeHealth(int Value)
        {
            _health.Value =
                Mathf.Clamp(_health.Value + Value, 0, _maxHealth);// 0以上かつ、_maxHealthより上にならないように
        }
    }
}
