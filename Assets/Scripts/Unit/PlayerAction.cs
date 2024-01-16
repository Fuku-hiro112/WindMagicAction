using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // 新Inputシステムの利用に必要
using GameInput;

[RequireComponent(typeof(Animator))]
public class PlayerAction : UnitBase ,IAnimationAttackable
{
    [Header("速度設定")]
    [SerializeField] private float _moveSpeed = 4.0f; // 移動速度
    [SerializeField, Tooltip("回転速度")] private float _rotationSpeed = 8.0f; // 回転速度
    [SerializeField, Tooltip("再誕する時間")] private float _birthInterval = 5.0f; // 再誕生までの時間

    [Header("強化時設定")]
    [SerializeField, Tooltip("強化時間")] private float _strongDuration = 10.0f; // 強化時間
    [SerializeField, Tooltip("強化値")] private int _strongValue = 10; // 強化値　攻撃＋強化値＝攻撃値
    [Space(20)]
    [SerializeField] private int _healValue = 50; // 回復値
    [Space(20)]
    [Header("アタッチ必須オブジェクト")]
    [SerializeField] private GameObject _patDamage; // ダメージエフェクト
    [SerializeField] private GameObject _swordWeapon;

    private Animator _myAnim; // 自身のアニメーター
    private CombatAction _myCA; // 自身のCombatAction
    
    private Vector3 _damagePos = new Vector3(0, 1.5f, 0); // ダメージエフェクトの位置
    private GameObject _patSmoke; // 走行エフェクト
    private GameObject _patStrong; // 強化エフェクト
    private ParticleSystem _patHeal; // 回復エフェクト
    private ParticleSystem.MainModule _smokeMain; // 走行砂煙の本体
    private WeaponAction _weaponAction;
    private ConfirmAction _confirmAction;

    private void Start()
    {
        base.OnStart();
        TryGetComponent(out _myAnim);// 自身のアニメーターを取得
        TryGetComponent(out _myCA); // 自身のCombatActionを取得
        _patSmoke  = transform.Find("PatSmoke").gameObject; // 走行エフェクトを取得
        _patStrong = transform.Find("PatStrong").gameObject; // 強化エフェクトを取得
        _swordWeapon.TryGetComponent(out _weaponAction);// SwordActionを取得
        transform.Find("PatHeal").TryGetComponent(out _patHeal); // 回復エフェクトを取得
        _smokeMain = _patSmoke.GetComponent<ParticleSystem>().main; // 走行砂煙の本体を取得
        _confirmAction = ConfirmAction.s_Instance;

        _patHeal.Stop(); // 回復エフェクトを停止
        _patStrong.SetActive(false); // 強化エフェクトを無効化
    }
    private void FixedUpdate()
    {
        if (_myCA.IsDead) return; // 自身が死んでたら何もしない

        // 移動方向のベクトルを作成
        Vector3 direction = _confirmAction.MoveDirection;
        Quaternion horizontalRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
        Vector3 moveDirection = horizontalRotation * direction;

        _smokeMain.startSize = 1.5f * moveDirection.sqrMagnitude; // 移動方向への量に応じて砂煙サイズを制御
        // 移動指示のベクトル長をアニメーターに渡す
        _myAnim.SetFloat("Speed", moveDirection.magnitude);

        // 入力方向へ移動する
        transform.position += moveDirection * _moveSpeed * Time.fixedDeltaTime;
        // 入力方向へゆっくり回転する
        Vector3 LookDir = Vector3.Slerp(transform.forward, moveDirection, _rotationSpeed * Time.fixedDeltaTime);
        transform.LookAt(transform.position + LookDir);
    }
    private void Update()
    {
        if (_myCA.IsDead || Gamepad.current == null) return; // 自身が死んでたら & ゲームパットが無かったら何もしない

        // 確認用 以下変更が必要
        // Ｙボタン押下で、回復エフェクトが発生する
        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            _patHeal.Play();
            _myCA.ChangeHealth(_healValue);
        }
        // Ｌバンパー押下で、ダメージエフェクトが発生する
        if (Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            //OnDamage(); // ダメージエフェクト発生処理
        }
        // Ｒバンパー押下で、一定時間だけ強化を表現する
        if (Gamepad.current.rightShoulder.wasPressedThisFrame && !_patStrong.activeSelf)
        {
            //StartCoroutine("StrongAction", _strongDuration);
        }


        // 攻撃ボタンを押した時
        if (_confirmAction.InputAction.Player.Fire.WasPressedThisFrame())
        {
            //TODO: 攻撃モーション中に攻撃ボタンを受け付けないようにする　連打すると２回出るから
            _myAnim.SetTrigger("Attack"); // 攻撃モーションの発動
            //_stand.Attack();// 攻撃時ズームする
        }
        if (_confirmAction.InputAction.Player.Magic.WasPressedThisFrame())
        {

        }
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    public override IEnumerator OnDeath()
    {
        _myAnim.SetTrigger("Death"); // ダウンモーション発動
        yield return new WaitForSeconds(_birthInterval);
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
        StartCoroutine(Vibration(0.0f, 0.7f, 0.2f)); // バイブレーション
    }
    /// <summary>
    /// 再生処理
    /// </summary>
    private void ReBirth()
    {
        _myAnim.Rebind(); // アニメーターの初期化
        transform.position = Vector3.zero; // 原点にリスポーン
        transform.rotation = Quaternion.identity; // 回転も初期状態
        _myCA.Ready();
    }
    /// <summary>
    /// パワーアップ制御処理
    /// </summary>
    /// <param name="waitTime"></param>
    /// <returns></returns>
    private IEnumerator StrongAction(float waitTime)
    {
        _patStrong.SetActive(true); // 有効化
        _weaponAction.ChangePower(_strongValue);
        yield return new WaitForSeconds(_strongDuration);
        _patStrong.SetActive(false); // 無効化
        _weaponAction.ChangePower( -_strongValue);// 見にくいですがマイナスが付いてます
    }
    /// <summary>
    /// バイブレーション処理（大振動値,小振動値,持続秒数）
    /// </summary>
    /// <param name="VibL"></param>
    /// <param name="VibR"></param>
    /// <param name="Duration"></param>
    /// <returns></returns>
    private IEnumerator Vibration(float VibL, float VibR, float Duration)
    {
        // ゲームパットの場合振動する
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(VibL, VibR);
            yield return new WaitForSeconds(Duration);
            Gamepad.current.SetMotorSpeeds(0, 0); // バイブレーション停止
        }
    }
    /// <summary>
    /// 魔法攻撃
    /// </summary>
    public void HomingMagic()
    {
        // ホーミング弾魔法を放つ
    }
//------------アニメーションイベント-----------------------------
    /// <summary>
    /// 攻撃有効化
    /// </summary>
    public override void AttackStart()
    {
        _weaponActions[0].WeaponActivate(true);// nullが出る
    }
    /// <summary>
    /// 攻撃無効化
    /// </summary>
    public override void AttackFinish()
    {
        _weaponActions[0].WeaponActivate(false);
    }
//---------------------------------------------------------------
}