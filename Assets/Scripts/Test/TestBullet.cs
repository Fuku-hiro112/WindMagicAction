using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameInput;
using UnityEngine.UIElements;
using UnityEngine.Assertions;

public class TestBullet : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField, Tooltip("出現位置高さ補正")] private float _popPositionHeightCorrection;
    [SerializeField]private TargetDeterminationModel _targetDeterminationModel;
    public GameObject _bulletPrefab;
    private Vector3 _bulletPopPos;

    private void Reset()
    {
        //_player = GetComponent<Transform>();
    }
    private void Start()
    {
        Assert.IsNotNull(_bulletPrefab, $"{this}の_bulletPrefabがNullです");
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
    /// 弾の生成
    /// </summary>
    public void GenerateBullet(Transform player)
    {
        // カメラに見える場所で生成する
        Vector3 verticalCorrection = player.forward * 2;　　　　　　　　　　　　 // 縦(高さ)補正
        Vector3 horizontalCorrection = Vector3.up * _popPositionHeightCorrection;// 横(正面に)補正
        _bulletPopPos = player.position + verticalCorrection + horizontalCorrection;// 生成ポジション

        // 弾生成
        GameObject bullet = Instantiate(
            _bulletPrefab
            , _bulletPopPos
            ,Quaternion.FromToRotation(Vector3.forward, player.forward));

        //NOTE: nullの場合エラーが出るため、その場合はdefault値を渡すようにしている
        if(_targetDeterminationModel.TargetObj.Value == null) 
             bullet.GetComponent<TestShoot>().Shoot(default);
        else bullet.GetComponent<TestShoot>().Shoot(_targetDeterminationModel.TargetObj.Value.transform);
    }

}