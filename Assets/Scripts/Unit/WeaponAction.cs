using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unit;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Assertions;

[RequireComponent(typeof(AudioSource))]
public class WeaponAction : MonoBehaviour
{
    [SerializeField] private int _maxPower = 2; //最大攻撃力
    [SerializeField] private Collider _weaponCollier;
    [SerializeField] private ComplementCollider _complementCollier;
    [SerializeField] private bool _weaponStartActive = false;
    [SerializeField] private bool _hasPlayer;
    [SerializeField] private AudioClip _audioClip;

    private int _power; //現在の攻撃力
    private PlayerStats _playerStats;
    private int _healMagicPoint;
    private AudioSource _seAudioSource;
    //private LayerMask _layerMask;

    // hitしたObjのリスト
    private List<GameObject> _hitObjectList = new List<GameObject>(32); 

    private void Reset()
    {
        if (gameObject.layer == LayerMask.NameToLayer("PlayerSide"))
        {
            _hasPlayer = true;
        }
        _weaponCollier = GetComponent<BoxCollider>();
        _complementCollier = GetComponent<ComplementCollider>();
    }

    private void Start()
    {
        // Playerが持っているなら
        if (_hasPlayer)
        {
            PlayerAction playerAction = gameObject.transform.root.GetComponent<PlayerAction>();
            Assert.IsNotNull(playerAction, "PlayerActionがNullです");
            _healMagicPoint = playerAction.AttackHealMagicPoint;

            gameObject.transform.root.TryGetComponent(out _playerStats);
            //_layerMask = LayerMask.NameToLayer("EnemySide");
        }

        // AudioSourceの取得
        TryGetComponent(out _seAudioSource);
        
        //攻撃力を最大にする
        ChangePower(_maxPower);

        //武器を有無を決める
        WeaponActivate(_weaponStartActive);

        // 当たった時 OnTrigger
        this.OnTriggerEnterAsObservable()
                .Where(other => 
                {
                    bool isFirstHit = false;
                    // ヒットリストに無ければリストに入れる
                    if (!_hitObjectList.Contains(other.gameObject))
                    {
                        isFirstHit = true;
                        _hitObjectList.Add(other.gameObject);
                    }
                    // UnitStatsがあるか
                    bool hasUnitStats = other.GetComponent<UnitStats>() != null;
                    
                    return isFirstHit && hasUnitStats;//TODO: UnitStatsにするとDragonの尻尾に当たらないのでHitZoneなどのスクリプトをColliderに取り付けるようにしよう（時間あれば）
                })
                .Subscribe(other => 
                {
                    // ダメージ処理
                    other.gameObject.GetComponent<UnitStats>().OnDamage(_power);

                    if (_audioClip != null)
                    {
                        _seAudioSource.PlayOneShot(_audioClip);
                    }

                    // プレイヤーが持っているなら
                    if (_hasPlayer)
                    {
                        // Mp回復
                        _playerStats.ChangeMagicPoint(_healMagicPoint);
                        Debug.Log("Playerの攻撃");

                        // エフェクト発生
                    }
                });
    }

    /// <summary>
    /// 攻撃力の増減処理
    /// </summary>
    /// <param name="Value"></param>
    public void ChangePower(int Value)
    {
        Debug.Log($"{ this.gameObject} + {_power}");
        _power += Value;
        Debug.Log("パワー"+_power);
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
        if (active)
        {
            _hitObjectList.Clear();
        }
        _weaponCollier.enabled = active;
    }
    /// <summary>
    /// プレイヤーの武器の有効無効処理
    /// </summary>
    /// <param name="active"></param>
    public void PlayerWeaponActivate(bool active)
    {
        // 当たり判定の補完　ONOFF
        _complementCollier.isAttack = active;
        WeaponActivate(active);
    }
#endregion
}
