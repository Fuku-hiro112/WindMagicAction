using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAction : MonoBehaviour
{
    [SerializeField] int _maxPower; //�ő�U����
    [System.NonSerialized] public int Power; //���݂̍U����
    string _escTag; //�^�O�̑ޔ��G���A
    void Start()
    {
        Power = _maxPower; //�U���͂��ő�ɂ���
        _escTag = gameObject.tag; //�J�n���̃^�O��ޔ�
        WeaponActivate(false); //����̖�����
    }
    //�U���͂̑�������
    public void ChangePower(int Value)
    {
        Power += Value;
        if (Power < 0)
        {
            Power = 0;
        }
    }
    //����̗L����������
    public void WeaponActivate(bool active)
    {
        gameObject.tag = active ? _escTag : "Untagged";
    }
}
