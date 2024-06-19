using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // 新Inputシステムの利用に必要
using GameInput;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Assertions;
using SettingCamera;
using UniRx;
using DG.Tweening;

namespace Unit
{
    public enum SerectMagic
    {
        Homing,
        Burst,
        Slash,
        Enhance
    }

    [RequireComponent(typeof(Animator))]
    public class PlayerController : UnitBase
    {
        [Header("速度設定")]
        [SerializeField]                          private float _moveSpeed = 4.0f;     // 移動速度
        [SerializeField, Tooltip("回転速度")]     private float _rotationSpeed = 8.0f; // 回転速度
        [SerializeField, Tooltip("ヒット秒数")]   private float _hitStopTime = 0.2f;   // ヒットストップ時間
        [SerializeField, Tooltip("再誕する時間")] private float _birthInterval = 5.0f; // 再誕生までの時間
        [SerializeField, Tooltip("回避する力")]   private float _avoidPower = 300.0f;  // 回避する力
        [Header("剣攻撃-----------------------------------------------------------")]

        [SerializeField, Tooltip("攻撃力")]   public int AttackPower       = 2; // 通常攻撃の攻撃力
        [SerializeField, Tooltip("強攻撃力")] public int StrongAttackPower = 4; // 強攻撃の攻撃力

        [Tooltip("攻撃時回復MP量")] public int AttackHealMagicPoint = 10;

        [Header("-------------------------------------------------------------------------------")]

        [Header("魔法-----------------------------------------------------------")]

        // 0:誘導弾　1:範囲魔法 2:斬撃魔法 3:強化魔法
        [SerializeField, Tooltip("0:誘導弾　1:範囲魔法 2:斬撃魔法 3:強化魔法")]
        private int[] _requiredMagicPoints = new int[4];

        [Header("-------------------------------------------------------------------------------")]


        [Header("強化時設定")]
        [SerializeField, Tooltip("強化時間")] private float _strongDuration = 10.0f; // 強化時間
        [SerializeField, Tooltip("強化値")]   private int _strongValue = 10; // 強化値　攻撃＋強化値＝攻撃値
        [Space(20)]
        [SerializeField] private int _healValue = 50; // 回復値
        [Space(20)]

        [Header("*カメラシェイク設定*-----------------------------------------------------------")]
        [Header(" ダメージ時")]
        [SerializeField] private Vector3 _positionStrengthDamage = new Vector3(0.2f, 0.2f, 0.2f);
        [SerializeField] private Vector3 _rotationStrengthDamage = new Vector3(2, 2, 2);
        [SerializeField] private float _shakeDurationDamage = 0.3f;
        [Header(" 攻撃ヒット時")]
        [SerializeField] private Vector3 _positionHitAttack = new Vector3(0.1f, 0.2f, 0.2f);
        [SerializeField] private Vector3 _rotationHitAttack = new Vector3(2, 2, 2);
        [SerializeField] private float _shakeDurationHitAttack = 0.15f;
        [Header("-------------------------------------------------------------------------------")]

        [Header("アタッチ必須オブジェクト")]
        [SerializeField] private GameObject _patDamage; // ダメージエフェクト
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private TargetDeterminationModel _targetDeterminationModel;
        [SerializeField] private MagicShoot _magicShoot;
        [SerializeField] private EnemyManager _enemyManager;

        private bool _isAttacking = false; // 攻撃中か
        private bool _isEnhance = false; // 強化中か
        private HomingBullet _testBullet;
        private ShakeCamera _shakeCamera;
        private Animator _myAnim; // 自身のアニメーター
        private UnitStats _myStats; // 自身のCombatAction
        private PlayerStats _myPlayerStats;
        private Rigidbody _myRigidbody;

        private Vector3 _damagePos = new Vector3(0, 1.5f, 0); // ダメージエフェクトの位置
        private GameObject _patSmoke; // 走行エフェクト
        private GameObject _patStrong; // 強化エフェクト
        private ParticleSystem _patHeal; // 回復エフェクト
        private ParticleSystem.MainModule _smokeMain; // 走行砂煙の本体
        private ConfirmAction _confirmAction;
        private Camera _camera;//NOTE: Camera.mainで取るとShake中カメラの切り替えでバグるので

        private ReactiveProperty<SerectMagic> _currentMagic = new ReactiveProperty<SerectMagic>();
        
        // パブリック
        public IReadOnlyReactiveProperty<SerectMagic> CurrentMagic => _currentMagic;
        public bool CanMove { get; private set; } = true;


        // プロパティ
        public bool IsAvoiding { get; private set; } = false; // 回避中,ダメージを受けない状態にTrueにする
        public float HitStopTime => _hitStopTime;
        public Dictionary<SerectMagic, int> RequiredMagicDictionary { get; private set; }
            = new Dictionary<SerectMagic, int>(4);

        private void Reset()
        {
            _targetDeterminationModel = Camera.main.GetComponent<TargetDeterminationModel>();
            _magicShoot = GetComponent<MagicShoot>();
        }
        private void Awake()
        {
            _currentMagic.Value = Unit.SerectMagic.Homing;

            // 消費MPのDictionaryを初期化
            RequiredMagicDictionary = new Dictionary<SerectMagic, int>(4);
            for (int i = 0; i < 4; i++)
            {
                RequiredMagicDictionary.Add((SerectMagic)i, _requiredMagicPoints[i]);
                Assert.AreNotEqual(_requiredMagicPoints[i], 0);// 0ならエラーを出す
            };
        }
        private new void Start() //NOTE: 継承元にStartがあるため自動でnewされる
        {
            base.Start(); //NOTE: 自動でnewされ、呼び出されなくなるためここで呼び出し
            _camera = Camera.main;
            TryGetComponent(out _myAnim);// 自身のアニメーターを取得
            TryGetComponent(out _myStats); // 自身のCombatActionを取得
            TryGetComponent(out _myPlayerStats);
            TryGetComponent(out _myRigidbody);
            TryGetComponent(out _testBullet);
            _camera.transform.GetChild(0).TryGetComponent(out _shakeCamera);
            _patSmoke = transform.Find("PatSmoke").gameObject; // 走行エフェクトを取得
            _patStrong = transform.Find("PatStrong").gameObject; // 強化エフェクトを取得
            transform.Find("PatHeal").TryGetComponent(out _patHeal); // 回復エフェクトを取得
            _smokeMain = _patSmoke.GetComponent<ParticleSystem>().main; // 走行砂煙の本体を取得
            _confirmAction = ConfirmAction.s_Instance;

            Assert.IsNotNull(_shakeCamera, $"{this}の_shakeCameraがエラーです");
            Assert.IsNotNull(_myAnim, $"{this}の_myAnimがエラーです");
            Assert.IsNotNull(_myStats, $"{this}の_myCAがエラーです");
            Assert.IsNotNull(_myPlayerStats, $"{this}の_myPlayerStatsがエラーです");
            Assert.IsNotNull(_patSmoke, $"{this}の_patSmokeがエラーです");
            Assert.IsNotNull(_patStrong, $"{this}の_patStrongがエラーです");
            Assert.IsNotNull(_patHeal, $"{this}の_patHealがエラーです");
            Assert.IsNotNull(_confirmAction, $"{this}の_confirmActionがエラーです");

            _patHeal.Stop(); // 回復エフェクトを停止
            _patStrong.SetActive(false); // 強化エフェクトを無効化
            _myAnim.SetFloat("Speed", 0);
        }

        private void FixedUpdate()
        {
            if (_myStats.IsDead || !CanMove) // 自身が死んでいる、動けない時は何もしない
            {
                if(_myStats.IsDead)
                {
                    CanMove = false;
                    _myAnim.SetFloat("Speed", 0);
                }
                return;
            }

            // 移動方向のベクトルを作成
            Vector3 horizontalMoveDirection = _confirmAction.InputVectorFromPosition(_camera.transform);

            // 移動方向への量に応じて砂煙サイズを制御
            _smokeMain.startSize = 1.5f * horizontalMoveDirection.sqrMagnitude;

            // 移動指示のベクトル長をアニメーターに渡す                                                          
            _myAnim.SetFloat("Speed", horizontalMoveDirection.magnitude);

            ControlMove(horizontalMoveDirection);
            ControlRotate(horizontalMoveDirection);
        }

        private void Update()
        {
            if (_myStats.IsDead) return; // 自身が死んでたら何もしない

            switch (_cameraManager.CameraModeType.Value)
            {
                case CameraMode.Default:
                    _targetDeterminationModel.NullTarget();
                    break;
                case CameraMode.Aim:
                    _targetDeterminationModel.OnUpdate();
                    break;
                case CameraMode.LookTarget:
                    break;
            }

            SwitchAim();
            SerectMagic();

            if (CanMove)
            {
                OnAttack();//TODO: コンボ攻撃を実装したい
                OnMagic();
            }
            // 回避中でも、攻撃中でもない時
            if (!IsAvoiding && !_isAttacking)//TODO: 余裕があれば先行入力させたいね
            {
                OnAvoid();
            }
        }
        /// <summary>
        /// 移動制御
        /// </summary>
        /// <param name="moveDirection">移動方向</param>
        private void ControlMove(Vector3 moveDirection)
        {
            // 入力方向へ移動する
            transform.position += moveDirection * _moveSpeed * Time.fixedDeltaTime;
            float y = Terrain.activeTerrain.SampleHeight(transform.position); // Terrainに高さを合わせる
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }
        /// <summary>
        /// 回転制御
        /// </summary>
        /// <param name="moveDirection">移動方向</param>
        private void ControlRotate(Vector3 moveDirection)
        {
            // カメラモードが通常（Default）なら
            if (_cameraManager.CameraModeType.Value == CameraMode.Default)
            {
                // プレイヤーを移動方向へゆっくり回転する
                Vector3 LookDir = Vector3.Slerp(transform.forward, moveDirection, _rotationSpeed * Time.fixedDeltaTime);
                transform.LookAt(transform.position + LookDir);
            }// エイム状態なら
            else if (_cameraManager.CameraModeType.Value == CameraMode.Aim)
            {
                //NOTE: カメラの向きにプレイヤーを回転する
                // カメラの位置から画面中央に向かってレイを飛ばす
                Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

                // レイの原点から方向に10m伸ばした座標
                Vector3 targetPosition = ray.origin + ray.direction * 10f;//NOTE: 別に何mでも良い

                // ターゲットオブジェクトの向きを緩やかに追従
                Vector3 targetDirection = targetPosition - transform.position;
                targetDirection.y = 0f; // 高さは考慮しない場合、y軸の回転を無効にする

                // 線形補間を使用して緩やかな回転を行う
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
            }
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        public override async UniTaskVoid OnDeathAsync()
        {
            _myAnim.SetTrigger("Death"); // ダウンモーション発動
        }
        /// <summary>
        /// ダメージ発生処理
        /// </summary>
        public override void VisualizationDamege()
        {
            GameObject Fx = Instantiate(_patDamage); // ダメージエフェクトを生成
            Fx.transform.position = transform.position + _damagePos; // 位置を補正
            Destroy(Fx, 1.0f); // 1.0秒後にエフェクトを破棄
            VibrationAsync(0.0f, 0.7f, 0.2f).Forget(); // バイブレーション
            
            // 画面をシェイク
            _shakeCamera.Shake(_positionStrengthDamage, _rotationStrengthDamage, _shakeDurationDamage);
        }
        /// <summary>
        /// バイブレーション処理
        /// </summary>
        /// <param name="VibL">大振動値</param>
        /// <param name="VibR">小振動値</param>
        /// <param name="Duration">持続秒数</param>
        /// <returns></returns>
        private async UniTask VibrationAsync(float VibL, float VibR, float Duration)
        {
            if (Gamepad.current != null)// ゲームパットの場合
            {
                Gamepad.current.SetMotorSpeeds(VibL, VibR);//振動
                await UniTask.Delay(TimeSpan.FromSeconds(Duration), cancellationToken:token);//Durarion秒待機
                Gamepad.current.SetMotorSpeeds(0, 0); // 振動停止
            }
        }
        /// <summary>
        /// 攻撃処理
        /// </summary>
        private void OnAttack()
        {
            // 攻撃ボタンを押した時
            if (_confirmAction.InputAction.Player.Fire.WasPressedThisFrame())
            {
                // 攻撃モーションの発動
                _myAnim.SetTrigger("Attack");//NOTE: アニメーションイベントで攻撃処理をしている
                CanMove = false;
            }
            // 強攻撃ボタンを押した時
            if (_confirmAction.InputAction.Player.StrongAttack.WasPressedThisFrame())
            {
                _myAnim.SetTrigger("StrongAttack");
                CanMove = false;
            }
        }
        /// <summary>
        /// 攻撃ヒット処理
        /// </summary>
        /// <param name="hitStopTime"></param>
        public void AttackHit(float hitStopTime)
        {
            // 画面をシェイク
            _shakeCamera.Shake(_positionHitAttack, _rotationHitAttack, _shakeDurationHitAttack);
            // ヒットストップアニメーションを指定秒数止める
            _myAnim.speed = 0;
            var sequence = DOTween.Sequence();
            sequence.SetDelay(hitStopTime);
            sequence.AppendCallback(() => _myAnim.speed = 1);
        }

        /// <summary>
        /// 回避処理
        /// </summary>
        private void OnAvoid()
        {
            if (_confirmAction.InputAction.Player.Avoid.WasPressedThisFrame())
            {
                // 無敵になる
                CanMove = false;
                _myAnim.SetTrigger("MagicCancel");

                //NOTE: BurstMagicの貯め中に回避した場合Particleが残ってしまうので非表示にする
                _magicShoot.StopParticle();

                // 入力方向に回転させる
                Vector3 InputDirection = _confirmAction.InputVectorFromPosition(_camera.transform);
                transform.rotation = Quaternion.LookRotation(InputDirection);

                _myAnim.SetTrigger("Avoid");
            }
        }
        /// <summary>
        /// カメラモードのAim状態の切り替え
        /// </summary>
        private void SwitchAim()
        {
            if (_confirmAction.InputAction.Player.Aim.WasPressedThisFrame())// 押した瞬間
            {
                _cameraManager.SwitchMode(CameraMode.Aim);
            }
            else if (_confirmAction.InputAction.Player.Aim.IsPressed())// 押している間
            {

            }
            else if (_confirmAction.InputAction.Player.Aim.WasReleasedThisFrame())// 離した瞬間
            {
                _cameraManager.SwitchMode(CameraMode.Default);
            }
        }
        /// <summary>
        /// Playerをdirectionの向きに回転
        /// </summary>
        /// <param name="direction"></param>
        private void PlayerRotate(Vector3 direction)
        {
            // y軸の回転のみを行うため、y軸成分を0に設定する
            direction.y = 0f;

            // 方向ベクトルから回転を求める
            Quaternion rotation = Quaternion.LookRotation(direction);

            // Playerに回転を適用する
            transform.rotation = rotation;
        }
        #region Magic

        /// <summary>
        /// 魔法選択
        /// </summary>
        private void SerectMagic()
        {
            if (_confirmAction.CurrentMagic.x > 0)// 右ボタン
            {
                _currentMagic.Value = Unit.SerectMagic.Burst;
            }
            else if (_confirmAction.CurrentMagic.x < 0)// 左ボタン
            {
                _currentMagic.Value = Unit.SerectMagic.Slash;
            }
            else if (_confirmAction.CurrentMagic.y > 0)// 下ボタン
            {
                _currentMagic.Value = Unit.SerectMagic.Homing;
            }
            else if (_confirmAction.CurrentMagic.y < 0)// 上ボタン
            {
                _currentMagic.Value = Unit.SerectMagic.Enhance;
            }
        }

        /// <summary>
        /// 魔法を発動する
        /// </summary>
        private void OnMagic()
        {
            // 魔法ボタンを押したら
            if (_confirmAction.InputAction.Player.Magic.WasPerformedThisFrame())
            {
                switch (_currentMagic.Value)
                {
                    case Unit.SerectMagic.Homing:
                        HomingMagic();
                        break;

                    case Unit.SerectMagic.Burst:
                        BurstMagic();
                        break;

                    case Unit.SerectMagic.Slash:
                        SlashMagic();
                        break;

                    case Unit.SerectMagic.Enhance:
                        StrongMagicAsync().Forget();
                        break;

                    default:
                        Debug.LogError("CurrentMagic.Valueが許容しない値になっています");
                        break;
                }
            }
        }

        /// <summary>
        /// ホーミング弾魔法を放つ
        /// </summary>
        private void HomingMagic()
        {
            int homing = (int)Unit.SerectMagic.Homing;

            // MPが足りるか
            if (_myPlayerStats.IsMagicPointEnough(_requiredMagicPoints[homing]))//NOTE: SerectMagicをキャストしても良いけど...重そうだし手書きで
            {
                // プレイヤーの向きを回転
                PlayerRotate(transform.position - _camera.transform.position);

                // ホーミング弾を生み出す
                _testBullet.GenerateBullet();

                // MP消費
                _myPlayerStats.ChangeMagicPoint(-_requiredMagicPoints[homing]);

                // アニメーション開始
                _myAnim.SetTrigger("BulletMagic");
            }
            else
            {
                AudioManager.Instance.PlaySE(SESoundData.SE.NotMP);
            }
        }

        /// <summary>
        /// 広範囲魔法  詳細: 長い貯めの後、火の玉を生み出しtargetに飛んでいく（いないなら近くの敵に）着弾時広範囲攻撃を生み出す
        /// </summary>
        private void BurstMagic()
        {
            int burst = (int)Unit.SerectMagic.Burst;

            // 立ち止まって放つので、移動アニメーションを止める
            _myAnim.SetFloat("Speed", 0);
            
            PlayMagic(burst, "BurstMagic", false);
        }
        /// <summary>
        /// 斬撃魔法 まっすぐ飛んでいく
        /// </summary>
        private void SlashMagic()
        {
            int slash = (int)Unit.SerectMagic.Slash;

            PlayMagic(slash, "SlashMagic", false);
        }
        /// <summary>
        /// 魔法を使う
        /// </summary>
        /// <param name="mp">消費MP</param>
        /// <param name="magicName">魔法のTrigger名</param>
        /// <param name="canMove">アニメーション中移動出来るか</param>
        private void PlayMagic(int mp, string magicName, bool canMove = true)
        {
            // MPが足りるか
            if (_myPlayerStats.IsMagicPointEnough(_requiredMagicPoints[mp]))
            {
                // プレイヤーの向きを回転
                PlayerRotate(transform.position - _camera.transform.position);

                // アニメーション開始
                _myAnim.SetTrigger(magicName);

                // MP消費
                _myPlayerStats.ChangeMagicPoint(-_requiredMagicPoints[mp]);

                CanMove = canMove;
            }
            else
            {
                AudioManager.Instance.PlaySE(SESoundData.SE.NotMP);
            }
        }

        /// <summary>
        /// パワーアップ制御処理
        /// </summary>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        private async UniTaskVoid StrongMagicAsync()
        {
            int enhance = (int)Unit.SerectMagic.Enhance;
            // MPが足りるか
            if (_myPlayerStats.IsMagicPointEnough(_requiredMagicPoints[enhance]) && !_isEnhance)
            {
                // 強化開始
                _isEnhance = true;
                _patStrong.SetActive(true); // エフェクト有効化
                _weaponActions[0].ChangePower(_strongValue);// 攻撃強化

                // MP消費
                _myPlayerStats.ChangeMagicPoint( -_requiredMagicPoints[enhance]);

                // 待機
                await UniTask.Delay(TimeSpan.FromSeconds(_strongDuration), cancellationToken: token);// _strongDurarion秒待機

                // 強化終了
                _isEnhance = false;
                _patStrong.SetActive(false); // エフェクト無効化
                _weaponActions[0].ChangePower( -_strongValue);//HACK: 見にくいですがマイナスが付いてます
            }
            else
            {
                AudioManager.Instance.PlaySE(SESoundData.SE.NotMP);
            }
        }


#endregion

        #region AnimationEvent

        /// <summary>
        /// 攻撃有効化
        /// </summary>
        public override void AttackStart()
        {
            _weaponActions[0].PlayerWeaponActivate(true, AttackPower);// nullが出る
            _isAttacking = true;
        }
        /// <summary>
        /// 強攻撃有効化
        /// </summary>
        public void StrongAttackStart()
        {
            _weaponActions[0].PlayerWeaponActivate(true, StrongAttackPower);
            _isAttacking = true;
        }
        /// <summary>
        /// 攻撃無効化
        /// </summary>
        public override void AttackFinish()
        {
            _weaponActions[0].PlayerWeaponActivate(false, -AttackPower);
            _isAttacking = false;
        }
        /// <summary>
        /// 強攻撃無効化
        /// </summary>
        public void StrongAttackFinish()
        {
            _weaponActions[0].PlayerWeaponActivate(false, -StrongAttackPower);
            _isAttacking = false;
        }

        public void AvoidStart()
        {
            IsAvoiding = true;
            Vector3 move = transform.forward * _avoidPower;
            _myRigidbody.AddForce(move, ForceMode.Impulse);
        }
        /// <summary>
        /// 回避終了処理
        /// </summary>
        public void AvoidFinish()
        {
            // 無敵時間を終了
            IsAvoiding = false;
        }
        public void MakeMovable()
        {
            CanMove = true;
        }

        /// <summary>
        /// Burst攻撃：貯め開始
        /// </summary>
        public void BurstChargeStart()
        {
            _magicShoot.Charge();
        }
        /// <summary>
        /// Burst攻撃：貯め終了
        /// </summary>
        public void BurstChargeFinish()
        {
            _magicShoot.StopParticle();
        }

        /// <summary>
        /// 斬撃を飛ばす
        /// </summary>
        public void SlashShoot()
        {
            // 照準に向かって飛ばす
            Vector3 targetPosition = _magicShoot.ToScreenCenter();
            Transform target = new GameObject().transform;
            target.position = targetPosition;

            _magicShoot.InstanceSlash(target);
            CanMove = true;
        }
        /// <summary>
        /// Burst攻撃：貯めたのち、火を放つ
        /// </summary>
        public void BurstFlareShoot()
        {
            GameObject targetObj = _targetDeterminationModel.TargetObj.Value;
            Transform targetTransform = new GameObject().transform;
            // Targetが居なかったら
            if (targetObj == null)
            {
                // 敵が居れば
                if (_targetDeterminationModel.HasExistsEnemy())
                {
                    // 近くの敵に飛んでいく　HACK:障害物があっても飛んでいくので壁に妨害される
                    targetTransform = _enemyManager.NearestEnemy(transform);
                }
                else// 敵が居なければ
                {
                    targetTransform.position = _magicShoot.ToScreenCenter();
                }
            }
            else// Targetが居れば
            {
                targetTransform = targetObj.transform;
            }

            // 火を飛ばす
            _magicShoot.InstanceFlame(targetTransform);
            //CanMove = true;
            Assert.AreNotEqual(targetTransform, default);
        }

        #endregion
    }
}
