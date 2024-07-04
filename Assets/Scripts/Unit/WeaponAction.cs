using System.Collections.Generic;
using UnityEngine;
using Unit;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Assertions;
using Unity.VisualScripting;

[RequireComponent(typeof(AudioSource))]
public class WeaponAction : MonoBehaviour
{
    [SerializeField] private int _maxPower = 2; //最大攻撃力
    [SerializeField] private Collider _weaponCollier;
    [SerializeField] private ComplementCollider _complementCollier;
    [SerializeField] private ParticleSystem _weaponParticleParent;
    [SerializeField] private ParticleSystem _weaponParticle = null;
    [SerializeField] private bool _weaponStartActive = false;
    [SerializeField] private bool _hasPlayer;
    [SerializeField] private AudioClip _audioClip;

    private int _power; //現在の攻撃力
    private PlayerStats _playerStats;
    private Unit.PlayerController playerController;
    private int _healMagicPoint;
    private float _hitStopTime;
    private AudioSource _seAudioSource;

    // hitしたObjのリスト
    private List<GameObject> _hitObjectList = new List<GameObject>(32); 

    private void Reset()
    {
        if (gameObject.layer == LayerMask.NameToLayer("PlayerSide"))
        {
            _hasPlayer = true;
        }

        _weaponCollier = GetComponent<Collider>();
        TryGetComponent(out _complementCollier);
    }

    //private void OnParticleCollision(GameObject other)
    //{
    //    if(_weaponParticle != null)
    //    if(IsFirstTimeCollidingUnit(other)) ApplyDamage(other);
    //}

    private void Start()
    {
        // Playerが持っているなら
        if (_hasPlayer)
        {
            GameObject player = GameObject.FindWithTag("Player");
            playerController = player.GetComponent<Unit.PlayerController>();
            Assert.IsNotNull(playerController, "PlayerActionがNullです");
            _healMagicPoint = playerController.AttackHealMagicPoint;
            _hitStopTime = playerController.HitStopTime;

            player.TryGetComponent(out _playerStats);
        }

        // パーティクルの場合止める
        if (_weaponCollier == null)
        {
            _weaponParticleParent.Stop();
        }

        // AudioSourceの取得
        TryGetComponent(out _seAudioSource);
        
        //攻撃力を最大にする
        ChangePower(_maxPower);

        //武器を有無を決める
        WeaponActivate(_weaponStartActive);

        // 当たったUnitにダメージを与える
        if (_weaponParticle != null)
        // パーティクル衝突時のストリーム
        this.OnParticleCollisionAsObservable()
            .Where(other => 
            {
                bool suicide = false;
                if (_hasPlayer) suicide = other.CompareTag("Player");
                return IsFirstTimeCollidingUnit(other) && !suicide; 
            })
            .Subscribe(other => ApplyDamage(other));
        else
            // Collision（Trigger）衝突時のストリーム　
            this.OnTriggerEnterAsObservable()
                .Where(other => IsFirstTimeCollidingUnit(other))
                .Subscribe(other => ApplyDamage(other));
    }

    /// <summary>
    /// ダメージを与える
    /// </summary>
    /// <param name="obj"></param>
    private void ApplyDamage(Collider obj) => ApplyDamage(obj.gameObject);
    private void ApplyDamage(GameObject obj)
    {
        GameObject rootObj = obj.transform.root.gameObject;

        // ダメージ処理
        rootObj.GetComponent<UnitStats>().OnDamage(_power);

        if (_audioClip != null)
        {
            _seAudioSource.PlayOneShot(_audioClip);
        }

        Debug.Log($"{this.gameObject.name}の攻撃:{_power}");
        
        // プレイヤーが持っているなら
        if (_hasPlayer)
        {
            // プレイヤーの攻撃ヒット処理　
            playerController.AttackHit(_hitStopTime);
            // 敵の被弾処理 enemyActionBase.OnDamage();
            rootObj.GetComponent<EnemyControllerBase>().OnDamage(_hitStopTime);

            // Mp回復
            _playerStats.ChangeMagicPoint(_healMagicPoint);

            //TODO: MP回復エフェクト発生
        }// 敵が持っているなら
        else
        {
            IMissingAttackCountReseter iAttackCounter;

            // IAttackCounterがあるなら
            if (rootObj.TryGetComponent(out iAttackCounter))
                iAttackCounter.MissingAttackCountReset();// 攻撃が外れた回数をリセット
        }
    }

    /// <summary>
    /// 初めて当たったUnitか
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    private bool IsFirstTimeCollidingUnit(Collider other) => IsFirstTimeCollidingUnit(other.gameObject);
    private bool IsFirstTimeCollidingUnit(GameObject obj)
    {
        bool isFirstHit = false;
        GameObject rootObj = obj.transform.root.gameObject;
        // ヒットリストに無ければリストに入れる
        if (!_hitObjectList.Contains(rootObj))
        {
            isFirstHit = true;
            _hitObjectList.Add(rootObj);//TODO: 一番上から２番目のオブジェクトをリストに入れる
        }
        // UnitStatsがあるか
        bool hasUnitStats = rootObj.GetComponent<UnitStats>() != null;

        bool isAvoiding = false;
        // Playerに当たったら
        if (rootObj.CompareTag("Player"))
        {
            // プレイヤーが回避中か取得
            isAvoiding = rootObj.GetComponent<Unit.PlayerController>().IsAvoiding;
            Debug.Log($"回避{isAvoiding}");
        }
        else
        {
            if (rootObj == transform.root.gameObject)
            {
                isAvoiding = true;
            }
        }

        return isFirstHit && hasUnitStats && !isAvoiding;
    }

    /// <summary>
    /// 攻撃力の増減処理
    /// </summary>
    /// <param name="Value"></param>
    public void ChangePower(int Value)
    {
        _power += Value;
        if (_power < 0) _power = 0;
    }

#region Activate
    /// <summary>
    /// 武器の有効無効処理
    /// </summary>
    /// <param name="active"></param>
    public void WeaponActivate(bool active)
    {
        // 武器有効時にListの要素を削除する
        if (active) _hitObjectList.Clear();

        if      (_weaponCollier != null) _weaponCollier.enabled = active;
        else if (_weaponParticle != null)//Colliderが無くて、パーティクルがある場合
        {
            if(active) _weaponParticleParent?.Play();
            else       _weaponParticleParent?.Stop();
        }
    }
    /// <summary>
    /// プレイヤーの武器の有効無効処理
    /// </summary>
    /// <param name="active"></param>
    public void PlayerWeaponActivate(bool active, int attackPower)
    {
        // 当たり判定の補完　ONOFF
        if (_complementCollier != null) //NOTE: 不具合があるのでデバックの為にもNullチェックを入れている
        { 
            _complementCollier.isAttack = active; 
        }
        WeaponActivate(active);
        ChangePower(attackPower);
    }
#endregion
}
