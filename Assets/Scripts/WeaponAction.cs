using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAction : MonoBehaviour
{
    [SerializeField] private int _maxPower; //最大攻撃力
    [System.NonSerialized] public int Power; //現在の攻撃力
    [SerializeField] private Collider _weaponCollier;
    [SerializeField] private bool _weaponStartActive = false;
    //private string _escTag; //タグの退避エリア

    private void Reset()
    {
        _weaponCollier = GetComponent<Collider>();
    }

    private void Start()
    {
        //_escTag = gameObject.tag; //開始時のタグを退避
        Power = _maxPower; //攻撃力を最大にする
        WeaponActivate(_weaponStartActive); //武器の無効化
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
}
