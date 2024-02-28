using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unit;


public class WeaponAction : MonoBehaviour
{
    [SerializeField] private int _maxPower; //最大攻撃力
    [System.NonSerialized] public int Power; //現在の攻撃力
    [SerializeField] private Collider _weaponCollier;
    [SerializeField] private ComplementCollider _complementCollier;
    [SerializeField] private bool _weaponStartActive = false;

    [SerializeField] private bool _isPlayer;
    private LayerMask _layerMask;
    private PlayerStats _playerStats;
    private int _healMagicPoint;

    private void Reset()
    {
        if (gameObject.layer == LayerMask.NameToLayer("PlayerSide"))
        {
            _isPlayer = true;
        }
        _weaponCollier = GetComponent<BoxCollider>();
        _complementCollier = GetComponent<ComplementCollider>();
    }

    private void Start()
    {
        if (_isPlayer) 
        {
            _healMagicPoint = gameObject.transform.root.GetComponent<PlayerAction>().HealMagicPoint;
            gameObject.transform.root.TryGetComponent(out _playerStats);
            _layerMask = LayerMask.NameToLayer("EnemySide");
        }
        //_escTag = gameObject.tag; //開始時のタグを退避
        Power = _maxPower; //攻撃力を最大にする
        WeaponActivate(_weaponStartActive); //武器の無効化
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isPlayer)
        {
            if (other.gameObject.layer == _layerMask)
            {
                _playerStats.ChangeMagicPoint(_healMagicPoint);
            }
        }
    }

    /// <summary>
    /// 攻撃力の増減処理
    /// </summary>
    /// <param name="Value"></param>
    public void ChangePower(int Value)
    {
        Power += Value;
        if (Power < 0) Power = 0;
    }
    /// <summary>
    /// 武器の有効無効処理
    /// </summary>
    /// <param name="active"></param>
    public void WeaponActivate(bool active)
    {
        //gameObject.tag = active ? _escTag : "Untagged"; // TriggerEnter時に攻撃判定を行っているため、tagを切り替えるだけでは常に判定内にいた場合攻撃を受けなくなる
        _weaponCollier.enabled = active;
    }
    /// <summary>
    /// プレイヤーの武器の有効無効処理
    /// </summary>
    /// <param name="active"></param>
    public void PlayerWeaponActivate(bool active)
    {
        _complementCollier.isAttack = active;
        _weaponCollier.enabled = active;
    }
}
