using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAction : MonoBehaviour
{
    [SerializeField] private int _maxPower; //�ő�U����
    [System.NonSerialized] public int Power; //���݂̍U����
    [SerializeField] private Collider _weaponCollier;
    [SerializeField] private bool _weaponStartActive = false;
    //private string _escTag; //�^�O�̑ޔ��G���A

    private void Reset()
    {
        _weaponCollier = GetComponent<Collider>();
    }

    private void Start()
    {
        //_escTag = gameObject.tag; //�J�n���̃^�O��ޔ�
        Power = _maxPower; //�U���͂��ő�ɂ���
        WeaponActivate(_weaponStartActive); //����̖�����
    }

    /// <summary>
    /// �U���͂̑�������
    /// </summary>
    /// <param name="Value"></param>
    public void ChangePower(int Value)
    {
        Power += Value;
        if (Power < 0) Power = 0;
    }
    /// <summary>
    /// ����̗L����������
    /// </summary>
    /// <param name="active"></param>
    public void WeaponActivate(bool active)
    {
        //gameObject.tag = active ? _escTag : "Untagged"; // TriggerEnter���ɍU��������s���Ă��邽�߁Atag��؂�ւ��邾���ł͏�ɔ�����ɂ����ꍇ�U�����󂯂Ȃ��Ȃ�
        _weaponCollier.enabled = active;
    }
}
