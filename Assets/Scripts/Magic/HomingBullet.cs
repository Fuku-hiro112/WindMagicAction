using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameInput;
using UnityEngine.UIElements;
using UnityEngine.Assertions;
using SettingCamera;
using UnityEngine.InputSystem.Interactions;

public class HomingBullet : MonoBehaviour
{
    [SerializeField, Tooltip("出現位置高さ補正")] private float _popPositionHeightCorrection;
    [SerializeField] private TargetDeterminationModel _targetDeterminationModel;
    public GameObject _bulletPrefab;
    private Vector3 _bulletPopPos;

    private void Start()
    {
        
    }
    /// <summary>
    /// 弾の生成
    /// </summary>
    public void GenerateBullet()
    {
        /*
        Vector3 instanceDirection = default;   // 生成方向
        Vector3 horizontalCorrection = default;

        // カメラに見える場所で生成する
        switch (_cameraManager.CameraModeType.Value)
        {
            case CameraMode.Default:
                instanceDirection = transform.position - _camera.transform.position;
                break;
            case CameraMode.Aim:
                instanceDirection = Vector3.forward;
                break;
            default: 
                break;
        }
        horizontalCorrection = new Vector3(instanceDirection.x, 0, instanceDirection.z).normalized * 2;// 横(正面に)補正
        */
        Vector3 horizontalCorrection = transform.forward * 2;
        Vector3 verticalCorrection = Vector3.up * _popPositionHeightCorrection;        　　　　　　　　// 縦(高さ)補正
        _bulletPopPos = transform.position + verticalCorrection + horizontalCorrection;// 生成ポジション

        // 弾生成
        GameObject bullet = Instantiate(
            _bulletPrefab
            , _bulletPopPos
            ,Quaternion.FromToRotation(Vector3.forward, transform.forward));
        
        //NOTE: nullの場合エラーが出るため、その場合はdefault値を渡すようにしている
        if(_targetDeterminationModel.TargetObj.Value == null) 
             bullet.GetComponent<HomingShoot>().Initialize(default);
        else bullet.GetComponent<HomingShoot>().Initialize(_targetDeterminationModel.TargetObj.Value.transform);
    }

}