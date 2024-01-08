using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // ナビメッシュの利用に必要
public class EnemyAction : MonoBehaviour
{
    NavMeshAgent _myNavi; // 自身のナビメッシュ
    Animator _myAnim; // 自身のアニメーター
    GameObject _player; // プレイヤー
    [SerializeField] float _deathTime = 3.0f; // 死亡後に消えるまでの時間
    CombatAction _myCA; // 自身のCombatAction
    CombatAction _playerCA; // プレイヤーのCombatAction
    [SerializeField] GameObject _patDamage; // ダメージエフェクト
    Vector3 _damagePos = new Vector3(0, 1.5f, 0); // ダメージエフェクトの位置
    [SerializeField] GameObject _weapon;
    WeaponAction _weaponAction;
    void Start()
    {
        TryGetComponent(out _myAnim); // 自身のアニメーターを取得
        TryGetComponent(out _myNavi); // 自身のナビメッシュを取得
        TryGetComponent(out _myCA); // 自身のCombatActionを取得
        _weapon.TryGetComponent(out _weaponAction);
        _player = GameObject.FindGameObjectWithTag("Player"); // プレイヤーを取得
        if (_player)
        {
            // プレイヤーのCombatActionを取得
            _player.TryGetComponent(out _playerCA);
        }
    }
    // 攻撃有効化
    void AttackStart()
    {
        _weaponAction.WeaponActivate(true);
    }
    // 攻撃無効化
    void AttackFinish()
    {
        _weaponAction.WeaponActivate(false);
    }
    // ダメージ視覚処理
    void OnDamage()
    {
        GameObject Fx = Instantiate(_patDamage); // ダメージエフェクトを生成
        Fx.transform.position = transform.position + _damagePos; // 位置を補正
        Destroy(Fx, 1.0f); // エフェクトを1.0秒後に破棄
    }
    // 死亡処理
    void OnDeath()
    {
        _myNavi.enabled = false; // ナビメッシュ切る
        _myAnim.SetFloat("Speed", 0); // 移動はしない
        _myAnim.SetBool("Attack", false); // 攻撃停止
        _myAnim.SetTrigger("Death"); // 死亡モーション発動
        Destroy(gameObject, _deathTime); // deathTime後に自身を撤去
    }
    void Update()
    {
        if (!_player || _myCA.IsDead)
        {
            return; // プレイヤー未発見なら、何もしない
        }
        // プレイヤー死亡時の対応
        if (_playerCA.IsDead)
        {
            _myAnim.SetFloat("Speed", 0); // 移動はしない
            _myAnim.SetBool("Attack", false); // 攻撃停止
            return; // 以降の処理は行わない
        }
        // プレイヤーとの距離を求める
        float D = Vector3.Distance(transform.position, _player.transform.position);
        if (D > 5.0f)
        {
            // プレイヤーが5m以上先
            _myNavi.enabled = false; // ナビメッシュ切る
            _myAnim.SetFloat("Speed", 0); // 移動はしない
            _myAnim.SetBool("Attack", false); // 攻撃停止
        }
        else if (D <= 1.0f)
        {
            // プレイヤーとの距離が1m以下、立ち止まって攻撃開始
            _myNavi.enabled = false; // ナビメッシュ切る
            _myAnim.SetFloat("Speed", 0); // 移動はしない
            _myAnim.SetBool("Attack", true); // 攻撃開始
        }
        else
        {
            // プレイヤーとの距離が1～5mなら、追いかける
            _myNavi.enabled = true; // ナビメッシュで追う
            _myNavi.destination = _player.transform.position; // ターゲットを指示
            _myAnim.SetFloat("Speed", _myNavi.velocity.magnitude); // 移動モーション
            _myAnim.SetBool("Attack", false); // 攻撃停止
        }
    }
}