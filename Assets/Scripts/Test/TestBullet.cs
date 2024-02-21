using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameInput;
using UnityEngine.UIElements;
using UnityEngine.Assertions;

public class TestBullet : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField, Tooltip("�o���ʒu�����␳")] private float _popPositionHeightCorrection;
    [SerializeField]private TestTargetDetermination _targetDetermination;
    public GameObject _bulletPrefab;
    private Vector3 _bulletPopPos;

    private void Reset()
    {
        //_player = GetComponent<Transform>();
    }
    private void Start()
    {
        Assert.IsNotNull(_bulletPrefab, "_bulletPrefab��Null�ł�");
    }
    /*
    void Update()
    {
        /*
        if (ConfirmAction.s_Instance.InputAction.Player.Magic.WasPressedThisFrame())
        {
            //GenerateBullet();
        }
        
    }
    */
    /// <summary>
    /// �e�̐���
    /// </summary>
    public void GenerateBullet(Transform player)
    {
        _bulletPopPos = player.position + player.forward * 2;// �J�����Ɍ�����ꏊ�Ő���������
        _bulletPopPos += Vector3.up * _popPositionHeightCorrection;
        GameObject bullet = Instantiate(_bulletPrefab, _bulletPopPos
            ,Quaternion.FromToRotation(Vector3.forward, player.forward));
        bullet.GetComponent<TestShoot>().Shoot(_targetDetermination.Target.Value);
    }

}