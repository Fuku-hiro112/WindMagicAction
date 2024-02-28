using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unit;


public class WeaponAction : MonoBehaviour
{
    [SerializeField] private int _maxPower; //�ő�U����
    [System.NonSerialized] public int Power; //���݂̍U����
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
        //_escTag = gameObject.tag; //�J�n���̃^�O��ޔ�
        Power = _maxPower; //�U���͂��ő�ɂ���
        WeaponActivate(_weaponStartActive); //����̖�����
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
    /// <summary>
    /// �v���C���[�̕���̗L����������
    /// </summary>
    /// <param name="active"></param>
    public void PlayerWeaponActivate(bool active)
    {
        _complementCollier.isAttack = active;
        _weaponCollier.enabled = active;
    }
}
