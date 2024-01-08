using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // 新Inputシステムの利用に必要
using GameInput;

public class PlayerAction : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 4.0f; // 移動速度
    [SerializeField] private float _rotSpeed = 8.0f; // 回転速度
    [SerializeField] private float _strongDuration = 10.0f; // 強化時間
    [SerializeField] private int _strongValue = 10; // 強化係数
    [SerializeField] private GameObject _patDamage; // ダメージエフェクト
    [SerializeField] private float _birthInterval = 5.0f; // 再誕生までの時間
    [SerializeField] private int _healAmount = 50; // 回復ヘルス量
    [SerializeField] private GameObject _standObj; // Stand
    [SerializeField] private GameObject _swordWeapon;
    private Vector3 _damagePos = new Vector3(0, 1.5f, 0); // ダメージエフェクトの位置
    private GameObject _patSmoke; // 走行エフェクト
    private GameObject _patStrong; // 強化エフェクト
    private ParticleSystem.MainModule _smokeMain; // 走行砂煙の本体
    private ParticleSystem _patHeal; // 回復エフェクト
    private Animator _myAnim; // 自身のアニメーター
    private CombatAction _myCA; // 自身のCombatAction
    private StandAction _stand;
    private WeaponAction _swordAction;
    private ConfirmAction _confirmAction = ConfirmAction.s_Instance;

    void Start()
    {
        _patSmoke = transform.Find("PatSmoke").gameObject; // 走行エフェクトを取得
        _patStrong = transform.Find("PatStrong").gameObject; // 強化エフェクトを取得
        //Camera.main.TryGetComponent(out _confirmAction);
        _standObj.TryGetComponent(out _stand); // StandActionを取得
        _swordWeapon.TryGetComponent(out _swordAction);// 
        TryGetComponent(out _myAnim);// 自身のアニメーターを取得
        TryGetComponent(out _myCA); // 自身のCombatActionを取得
        transform.Find("PatHeal").TryGetComponent(out _patHeal); // 回復エフェクトを取得
        _smokeMain = _patSmoke.GetComponent<ParticleSystem>().main; // 走行砂煙の本体を取得

        _patHeal.Stop(); // 回復エフェクトを停止
        _patStrong.SetActive(false); // 強化エフェクトを無効化
    }
    // パワーアップ制御処理
    IEnumerator StrongAction(float waitTime)
    {
        _patStrong.SetActive(true); // 有効化
        _swordAction.ChangePower(_strongValue);
        yield return new WaitForSeconds(_strongDuration);
        _patStrong.SetActive(false); // 無効化
        _swordAction.ChangePower( -_strongValue);// 見にくいですがマイナスが付いてます
    }
    // 死亡処理
    void OnDeath()
    {
        _myAnim.SetTrigger("Death"); // ダウンモーション発動
        Invoke("ReBirth", _birthInterval); // 再生処理を予約
    }
    // 再生処理
    void ReBirth()
    {
        _myAnim.Rebind(); // アニメーターの初期化
        transform.position = Vector3.zero; // 原点にリスポーン
        transform.rotation = Quaternion.identity; // 回転も初期状態
        _myCA.Ready();
    }
    void FixedUpdate()
    {
        if (_myCA.IsDead) return; // 自身が死んでたら何もしない
        /*
        // 左ジョイスティックのベクトルを作成
        Vector3 Dir = Vector3.zero;
        Dir.x = Gamepad.current.leftStick.ReadValue().x;
        Dir.z = Gamepad.current.leftStick.ReadValue().y;
        */
        Vector3 direction = _confirmAction.MoveDirection;

        _smokeMain.startSize = 1.5f * direction.sqrMagnitude; // 移動方向への量に応じて砂煙サイズを制御
        // 移動指示のベクトル長をアニメーターに渡す
        _myAnim.SetFloat("Speed", direction.magnitude);

        //if (Dir.sqrMagnitude < 0.01f) return; // 十分にジョイスティックが倒れていない判定

        // 入力方向へ移動する
        transform.position += direction * _moveSpeed * Time.fixedDeltaTime;
        // 入力方向へゆっくり回転する
        Vector3 LookDir = Vector3.Slerp(transform.forward, direction, _rotSpeed * Time.fixedDeltaTime);
        transform.LookAt(transform.position + LookDir);
    }
    /// <summary>
    /// ダメージ発生処理
    /// </summary>
    void OnDamage()
    {
        GameObject Fx = Instantiate(_patDamage); // ダメージエフェクトを生成
        Fx.transform.position = transform.position + _damagePos; // 位置を補正
        Destroy(Fx, 1.0f); // 1.0秒後にエフェクトを破棄
        StartCoroutine(Vibration(0.0f, 0.7f, 0.2f)); // バイブレーション
    }
    /// <summary>
    /// バイブレーション処理（大振動値,小振動値,持続秒数）
    /// </summary>
    /// <param name="VibL"></param>
    /// <param name="VibR"></param>
    /// <param name="Duration"></param>
    /// <returns></returns>
    IEnumerator Vibration(float VibL, float VibR, float Duration)
    {
        Gamepad.current.SetMotorSpeeds(VibL, VibR);
        yield return new WaitForSeconds(Duration);
        Gamepad.current.SetMotorSpeeds(0, 0); // バイブレーション停止
    }
    // 攻撃有効化
    public void AttackStart()
    {
        _swordAction.WeaponActivate(true);
    }
    // 攻撃無効化
    public void AttackFinish()
    {
        _swordAction.WeaponActivate(false);
    }
    void Update()
    {
        if (_myCA.IsDead || Gamepad.current == null) return; // 自身が死んでたら & ゲームパットが無かったら何もしない

        // 確認用
        // Ｙボタン押下で、回復エフェクトが発生する
        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            _patHeal.Play();
            _myCA.ChangeHealth(_healAmount);
        }
        // Ｌバンパー押下で、ダメージエフェクトが発生する
        if (Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            OnDamage(); // ダメージエフェクト発生処理
        }
        // Ｒバンパー押下で、一定時間だけ強化を表現する
        if (Gamepad.current.rightShoulder.wasPressedThisFrame && !_patStrong.activeSelf)
        {
            StartCoroutine("StrongAction", _strongDuration);
        }

        if (_confirmAction.InputAction.Player.Fire.WasPressedThisFrame())
        {
            //TODO: 攻撃モーション中に攻撃ボタンを受け付けないようにする
            _myAnim.SetTrigger("Attack"); // 攻撃モーションの発動
            _stand.Attack();
        }
    }
}