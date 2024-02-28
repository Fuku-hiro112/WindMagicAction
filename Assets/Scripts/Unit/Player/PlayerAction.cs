using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // 新Inputシステムの利用に必要
using GameInput;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Assertions;
using SettingCamera;
using System.Xml.Linq;

namespace Unit
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAction : UnitBase
    {
        [Header("速度設定")]
        [SerializeField] private float _moveSpeed = 4.0f; // 移動速度
        [SerializeField, Tooltip("回転速度")] private float _rotationSpeed = 8.0f; // 回転速度
        [SerializeField, Tooltip("再誕する時間")] private float _birthInterval = 5.0f; // 再誕生までの時間

        [Header("MP設定")]
        [SerializeField] private int _homingMagicPoint = 20;
        public int HealMagicPoint = 10;

        [Header("強化時設定")]
        [SerializeField, Tooltip("強化時間")] private float _strongDuration = 10.0f; // 強化時間
        [SerializeField, Tooltip("強化値")] private int _strongValue = 10; // 強化値　攻撃＋強化値＝攻撃値
        [Space(20)]
        [SerializeField] private int _healValue = 50; // 回復値
        [Space(20)]

        [Header("*カメラシェイク設定*-----------------------------------------------------------")]
        [Header(" ダメージ時")]
        [SerializeField] private Vector3 _positionStrengthDamage = new Vector3(0.2f, 0.2f, 0.2f);
        [SerializeField] private Vector3 _rotationStrengthDamage = new Vector3(2, 2, 2);
        [SerializeField] private float _shakeDurationDamage = 0.3f;
        [Header("-------------------------------------------------------------------------------")]

        [Header("アタッチ必須オブジェクト")]
        [SerializeField] private GameObject _patDamage; // ダメージエフェクト
        [SerializeField] private GameObject _swordWeapon;
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private TargetDeterminationModel _targetDeterminationModel;

        private ShakeCamera _shakeCamera;
        private Animator _myAnim; // 自身のアニメーター
        private UnitStats _myStats; // 自身のCombatAction
        private PlayerStats _myPlayerStats;

        private Vector3 _damagePos = new Vector3(0, 1.5f, 0); // ダメージエフェクトの位置
        private GameObject _patSmoke; // 走行エフェクト
        private GameObject _patStrong; // 強化エフェクト
        private ParticleSystem _patHeal; // 回復エフェクト
        private ParticleSystem.MainModule _smokeMain; // 走行砂煙の本体
        private WeaponAction _weaponAction;
        private ConfirmAction _confirmAction;
        private Camera _camera;//NOTE: Camera.mainで取るとShake中カメラの切り替えでバグるので

        private bool _isHoming = false;

        private void Reset()
        {
            _targetDeterminationModel = Camera.main.GetComponent<TargetDeterminationModel>();
        }

        private new void Start() //NOTE: 継承元にStartがあるため自動でnewされる
        {
            base.Start(); //NOTE: 自動でnewされ、呼び出されなくなるためここで呼び出し
            _camera = Camera.main;
            TryGetComponent(out _myAnim);// 自身のアニメーターを取得
            TryGetComponent(out _myStats); // 自身のCombatActionを取得
            TryGetComponent(out _myPlayerStats);
            _camera.transform.GetChild(0).TryGetComponent(out _shakeCamera);
            _patSmoke = transform.Find("PatSmoke").gameObject; // 走行エフェクトを取得
            _patStrong = transform.Find("PatStrong").gameObject; // 強化エフェクトを取得
            _swordWeapon.TryGetComponent(out _weaponAction);// SwordActionを取得
            transform.Find("PatHeal").TryGetComponent(out _patHeal); // 回復エフェクトを取得
            _smokeMain = _patSmoke.GetComponent<ParticleSystem>().main; // 走行砂煙の本体を取得
            _confirmAction = ConfirmAction.s_Instance;

            Assert.IsNotNull(_shakeCamera, $"{this}の_shakeCameraがエラーです");
            Assert.IsNotNull(_myAnim, $"{this}の_myAnimがエラーです");
            Assert.IsNotNull(_myStats, $"{this}の_myCAがエラーです");
            Assert.IsNotNull(_myPlayerStats, $"{this}の_myPlayerStatsがエラーです");
            Assert.IsNotNull(_patSmoke, $"{this}の_patSmokeがエラーです");
            Assert.IsNotNull(_patStrong, $"{this}の_patStrongがエラーです");
            Assert.IsNotNull(_weaponAction, $"{this}の_weaponActionがエラーです");
            Assert.IsNotNull(_patHeal, $"{this}の_patHealがエラーです");
            Assert.IsNotNull(_confirmAction, $"{this}の_confirmActionがエラーです");

            _patHeal.Stop(); // 回復エフェクトを停止
            _patStrong.SetActive(false); // 強化エフェクトを無効化
        }

        private void FixedUpdate()
        {
            if (_myStats.IsDead) return; // 自身が死んでたら何もしない

            // 移動方向のベクトルを作成
            Vector3 direction = _confirmAction.MoveDirection;
            Quaternion horizontalRotation = Quaternion.AngleAxis(_camera.transform.eulerAngles.y, Vector3.up);
            Vector3 moveDirection = horizontalRotation * direction;

            _smokeMain.startSize = 1.5f * moveDirection.sqrMagnitude; // 移動方向への量に応じて砂煙サイズを制御
                                                                      // 移動指示のベクトル長をアニメーターに渡す
            _myAnim.SetFloat("Speed", moveDirection.magnitude);

            // 入力方向へ移動する
            transform.position += moveDirection * _moveSpeed * Time.fixedDeltaTime;
            float y = Terrain.activeTerrain.SampleHeight(transform.position); // Terrainの
            transform.position = new Vector3(transform.position.x, y, transform.position.z);

            // プレイヤーの向き
            if (_cameraManager.CameraModeType.Value == CameraMode.Default)
            {
                // 入力方向へゆっくり回転する
                Vector3 LookDir = Vector3.Slerp(transform.forward, moveDirection, _rotationSpeed * Time.fixedDeltaTime);
                transform.LookAt(transform.position + LookDir);
            }
            else if (_cameraManager.CameraModeType.Value == CameraMode.Aim)
            {
                // カメラの位置から画面中央に向かってレイを飛ばす
                Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                // レイの原点から方向に10m伸ばした座標
                Vector3 targetPosition = ray.origin + ray.direction * 10f;

                // ターゲットオブジェクトの向きを緩やかに追従
                Vector3 targetDirection = targetPosition - transform.position;
                targetDirection.y = 0f; // 高さは考慮しない場合、y軸の回転を無効にする

                // 線形補間を使用して緩やかな追従を行う
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
            }
        }

        private void Update()
        {
            if (_myStats.IsDead) return; // 自身が死んでたら何もしない

            switch (_cameraManager.CameraModeType.Value)
            {
                case CameraMode.Default:
                    break;
                case CameraMode.Aim:
                    _targetDeterminationModel.OnUpdate();
                    break;
                case CameraMode.LookTarget:
                    break;
            }

            #region Debug

            if (Gamepad.current != null)// 確認用 以下変更が必要
            {
                // Ｙボタン押下で、回復エフェクトが発生する
                if (Gamepad.current.buttonNorth.wasPressedThisFrame)
                {
                    _patHeal.Play();
                    _myStats.ChangeHealth(_healValue);
                }
                // Ｌバンパー押下で、ダメージエフェクトが発生する
                if (Gamepad.current.buttonNorth.wasPressedThisFrame)
                {
                    //OnDamage(); // ダメージエフェクト発生処理
                    _shakeCamera.Shake(_positionStrengthDamage, _rotationStrengthDamage, _shakeDurationDamage);
                }
                // Ｒバンパー押下で、一定時間だけ強化を表現する
                if (Gamepad.current.rightShoulder.wasPressedThisFrame && !_patStrong.activeSelf)
                {
                    //StartCoroutine("StrongAction", _strongDuration);
                }
            }

            #endregion

            OnAttack();
            HomingMagic();
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        public override async UniTaskVoid OnDeath()
        {
            _myAnim.SetTrigger("Death"); // ダウンモーション発動
            await UniTask.Delay(TimeSpan.FromSeconds(_birthInterval));
            ReBirth(); // 再生処理を予約 //HACK: Invokeを使うとどこから呼び出されているかわからないため変更が必要
        }
        /// <summary>
        /// ダメージ発生処理
        /// </summary>
        public override void OnDamage()
        {
            GameObject Fx = Instantiate(_patDamage); // ダメージエフェクトを生成
            Fx.transform.position = transform.position + _damagePos; // 位置を補正
            Destroy(Fx, 1.0f); // 1.0秒後にエフェクトを破棄
            Vibration(0.0f, 0.7f, 0.2f).Forget(); // バイブレーション
                                                  //TODO: 画面をシェイク
            _shakeCamera.Shake(_positionStrengthDamage, _rotationStrengthDamage, _shakeDurationDamage);
        }
        /// <summary>
        /// 再誕処理
        /// </summary>
        private void ReBirth()
        {
            _myAnim.Rebind(); // アニメーターの初期化
            transform.position = Vector3.zero; // 原点にリスポーン
            transform.rotation = Quaternion.identity; // 回転も初期状態
            _myStats.Ready();
        }
        /// <summary>
        /// パワーアップ制御処理
        /// </summary>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        private async UniTask StrongAction(float waitTime)
        {
            _patStrong.SetActive(true); // 有効化
            _weaponAction.ChangePower(_strongValue);// 攻撃強化

            await UniTask.Delay(TimeSpan.FromSeconds(_strongDuration));// _strongDurarion秒待機

            _patStrong.SetActive(false); // 無効化
            _weaponAction.ChangePower(-_strongValue);//HACK: 見にくいですがマイナスが付いてます
        }
        /// <summary>
        /// バイブレーション処理
        /// </summary>
        /// <param name="VibL">大振動値</param>
        /// <param name="VibR">小振動値</param>
        /// <param name="Duration">持続秒数</param>
        /// <returns></returns>
        private async UniTask Vibration(float VibL, float VibR, float Duration)
        {
            if (Gamepad.current != null)// ゲームパットの場合
            {
                Gamepad.current.SetMotorSpeeds(VibL, VibR);//振動
                await UniTask.Delay(TimeSpan.FromSeconds(Duration));//Durarion秒待機
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
                //TODO: 攻撃モーション中に攻撃ボタンを受け付けないようにする　連打すると２回出るから
                // 攻撃モーションの発動
                _myAnim.SetTrigger("Attack");//NOTE: アニメーションイベントで攻撃処理をしている
            }
        }
        /// <summary>
        /// ホーミング弾魔法を放つ
        /// </summary>
        public void HomingMagic()
        {

            if (_confirmAction.InputAction.Player.Magic.WasPressedThisFrame())// 押した瞬間
            {
                _cameraManager.SwitchMode(CameraMode.Aim);
                _isHoming = true;
            }
            else if (_confirmAction.InputAction.Player.Magic.IsPressed())// 押している間
            {

            }
            else if (_confirmAction.InputAction.Player.Magic.WasReleasedThisFrame())// 離した瞬間
            {
                if (_myPlayerStats.IsMagicPointEnough(_homingMagicPoint) && _isHoming)
                {
                    GetComponent<TestBullet>().GenerateBullet(gameObject.transform);
                    _myPlayerStats.ChangeMagicPoint(-_homingMagicPoint);
                    _isHoming = false;
                }
                _cameraManager.SwitchMode(CameraMode.Default);
            }

            // ×でキャンセル　分かりやすいUIが必要
            if (_confirmAction.InputAction.Player.Avoid.WasPressedThisFrame())
            {
                _isHoming = false;
                _cameraManager.SwitchMode(CameraMode.Default);
            }
        }

        #region AnimationEvent

        /// <summary>
        /// 攻撃有効化
        /// </summary>
        public override void AttackStart()
        {
            _weaponActions[0].PlayerWeaponActivate(true);// nullが出る
        }
        /// <summary>
        /// 攻撃無効化
        /// </summary>
        public override void AttackFinish()
        {
            _weaponActions[0].PlayerWeaponActivate(false);
        }

        #endregion
    }
}
