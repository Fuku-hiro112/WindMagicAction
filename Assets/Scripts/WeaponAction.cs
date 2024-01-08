using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAction : MonoBehaviour
{
    [SerializeField] int _maxPower; //最大攻撃力
    [System.NonSerialized] public int Power; //現在の攻撃力
    string _escTag; //タグの退避エリア
    void Start()
    {
        Power = _maxPower; //攻撃力を最大にする
        _escTag = gameObject.tag; //開始時のタグを退避
        WeaponActivate(false); //武器の無効化
    }
    //攻撃力の増減処理
    public void ChangePower(int Value)
    {
        Power += Value;
        if (Power < 0)
        {
            Power = 0;
        }
    }
    //武器の有効無効処理
    public void WeaponActivate(bool active)
    {
        gameObject.tag = active ? _escTag : "Untagged";
    }
}
