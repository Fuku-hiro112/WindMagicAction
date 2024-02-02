using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]// AnimatorとNavMeshを必須に
public class EnemyActionBase : UnitBase
{
    [Header("範囲")]
    [SerializeField, Tooltip("攻撃開始距離")] protected float _fireDistance = 1.0f;
    [SerializeField, Tooltip("索敵範囲")] protected float _searchRange = 5.0f;
    [Space(20)]
    [SerializeField] protected float _deathTime = 3.0f; // 死亡後に消えるまでの時間
    [SerializeField] private GameObject _patDamage; // ダメージエフェクト
    [SerializeField] protected Vector3 _damagePos = new Vector3(0, 1.5f, 0); // ダメージエフェクトの位置
    [Space(20)]
    protected NavMeshAgent _myNavi; // 自身のナビメッシュ
    protected Animator _myAnim; // 自身のアニメーター
    private CombatAction _myCA;   // 自身のCombatAction

    private GameObject _player; // プレイヤー
    private CombatAction _playerCA; // プレイヤーのCombatAction

    private void Start()
    {
        base.Start();
        TryGetComponent(out _myAnim); // 自身のアニメーターを取得
        TryGetComponent(out _myNavi); // 自身のナビメッシュを取得
        TryGetComponent(out _myCA);　 // 自身のCombatActionを取得
        _player = GameObject.FindGameObjectWithTag("Player"); // プレイヤーを取得
        if (_player)
        {
            // プレイヤーのCombatActionを取得
            _player.TryGetComponent(out _playerCA);
        }
    }
    private void Update()
    {
        ActionEnemy();
    }
    protected void ActionEnemy()
    {
        if (!_player || _myCA.IsDead) return;// プレイヤー未発見時
        // プレイヤー死亡時の対応
        if (_playerCA.IsDead)
        {
            SomeAnimationsStopped();
            return;
        }
        // プレイヤーとの距離を求める
        float distance = Vector3.Distance(transform.position, _player.transform.position);

        if (distance > _searchRange)
        {
            // 移動停止
            _myNavi.enabled = false; // ナビメッシュ切る
            _myAnim.SetFloat("Speed", 0); //移動はしない
            _myAnim.SetBool("Attack", false); //攻撃停止
        }
        else if (distance <= _fireDistance)// プレイヤーとの距離が_fireDistance以下、
        {
            // 立ち止まって攻撃
            _myNavi.enabled = false; // ナビメッシュ切る
            _myAnim.SetFloat("Speed", 0); //移動はしない
            _myAnim.SetBool("Attack", true); //攻撃開始
        }
        else // プレイヤーとの距離が_fireDistance～_searchRangeなら
        {
            // プレイヤーを追従
            _myNavi.enabled = true; // ナビメッシュをオン
            _myNavi.destination = _player.transform.position; // ターゲットを指示
            _myAnim.SetFloat("Speed", _myNavi.velocity.magnitude); //移動モーション
            _myAnim.SetBool("Attack", false); //攻撃停止
        }
    }
    /// <summary>
    /// ダメージ視覚処理
    /// </summary>
    /// <returns></returns>
    public override void OnDamage()
    {
        GameObject Fx = Instantiate(_patDamage); // ダメージエフェクトを生成
        Fx.transform.position = transform.position + _damagePos; // 位置を補正
        Destroy(Fx, 1.0f); // エフェクトを1.0秒後に破棄
    }
    /// <summary>
    /// 死亡処理
    /// </summary>
    /// <returns></returns>
    public override async UniTaskVoid OnDeath()
    {
        _myNavi.enabled = false; // ナビメッシュ切る
        
        _myAnim.SetFloat("Speed", 0); // 移動はしない
        _myAnim.SetBool("Attack", false); // 攻撃停止

        _myAnim.SetTrigger("Death"); // 死亡モーション発動
        gameObject.tag = "Untagged";
        Destroy(gameObject, _deathTime); // _deathTime秒後に自身を削除
    }
    protected virtual void SomeAnimationsStopped()
    {
        _myAnim.SetFloat("Speed", 0); // 移動はしない
        _myAnim.SetBool("Attack", false); // 攻撃停止
    }
    /// <summary>
    /// 死亡演出
    /// </summary>
    protected virtual void DeathProduction()
    {
        // だんだん小さくなる
        transform.DOScale(Vector3.zero, _deathTime)
            .SetEase(Ease.OutCirc)
            .OnUpdate(() =>
            {
                transform.rotation *= Quaternion.Euler(Vector3.up * Time.deltaTime * 360);
            })
            .Play();
    }

//----------アニメーションイベント------------------------
    /// <summary>
    /// 攻撃有効化
    /// </summary>
    public override void AttackStart()
    {
        _weaponActions[0].WeaponActivate(true);
    }
    /// <summary>
    /// 攻撃無効化
    /// </summary>
    public override void AttackFinish()
    {
        _weaponActions[0].WeaponActivate(false);
    }
//-------------------------------------------------------
}