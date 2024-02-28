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
    [SerializeField]private TargetDeterminationModel _targetDeterminationModel;
    public GameObject _bulletPrefab;
    private Vector3 _bulletPopPos;

    private void Reset()
    {
        //_player = GetComponent<Transform>();
    }
    private void Start()
    {
        Assert.IsNotNull(_bulletPrefab, $"{this}��_bulletPrefab��Null�ł�");
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
        // �J�����Ɍ�����ꏊ�Ő�������
        Vector3 verticalCorrection = player.forward * 2;�@�@�@�@�@�@�@�@�@�@�@�@ // �c(����)�␳
        Vector3 horizontalCorrection = Vector3.up * _popPositionHeightCorrection;// ��(���ʂ�)�␳
        _bulletPopPos = player.position + verticalCorrection + horizontalCorrection;// �����|�W�V����

        // �e����
        GameObject bullet = Instantiate(
            _bulletPrefab
            , _bulletPopPos
            ,Quaternion.FromToRotation(Vector3.forward, player.forward));

        //NOTE: null�̏ꍇ�G���[���o�邽�߁A���̏ꍇ��default�l��n���悤�ɂ��Ă���
        if(_targetDeterminationModel.TargetObj.Value == null) 
             bullet.GetComponent<TestShoot>().Shoot(default);
        else bullet.GetComponent<TestShoot>().Shoot(_targetDeterminationModel.TargetObj.Value.transform);
    }

}