using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameInput;

[RequireComponent(typeof(Camera))]// Cameraコンポーネントが必要　ない場合は自動的に追加
public class TestCameraMove2 : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _distanceToPlayerM = 2f;    // カメラとプレイヤーとの距離[m]
    [SerializeField] private float _slideDistanceM = 0f;       // カメラを横にスライドさせる；プラスの時右へ，マイナスの時左へ[m]
    [SerializeField] private float _heightM = 1.2f;            // 注視点の高さ[m]
    [SerializeField] private float _rotationSensitivity = 100f;// 感度
    private ConfirmAction _confirmAction;

    private void Reset()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Start()
    {
        if (_target == null)
        {
            Debug.LogError("ターゲットが設定されていない");
            Application.Quit();
        }
        _confirmAction = ConfirmAction.s_Instance;
    }

    void FixedUpdate()
    {
        /*
        var rotX = Input.GetAxis("Mouse X") * Time.fixedDeltaTime * RotationSensitivity;
        var rotY = Input.GetAxis("Mouse Y") * Time.fixedDeltaTime * RotationSensitivity;
        */
        Vector2 lookDerection = _confirmAction.LookDirection;
        var rotX = lookDerection.x * Time.fixedDeltaTime * _rotationSensitivity;
        var rotY = -lookDerection.y * Time.fixedDeltaTime * _rotationSensitivity;// 上入力した時、上を向きたい為-(マイナスを付けている)

        var lookAt = _target.position + Vector3.up * _heightM;

        // 回転
        transform.RotateAround(lookAt, Vector3.up, rotX);// 横回転
        // カメラがプレイヤーの真上や真下にあるときにそれ以上回転させないようにする
        if (transform.forward.y > 0.9f && rotY < 0 || transform.forward.y < -0.9f && rotY > 0)
        {
            rotY = 0;
        }
        transform.RotateAround(lookAt, transform.right, rotY);// 縦回転

        // カメラとプレイヤーとの間の距離を調整
        transform.position = lookAt - transform.forward * _distanceToPlayerM;

        // 注視点の設定
        transform.LookAt(lookAt);

        // カメラを横にずらして中央を開ける
        transform.position +=  transform.right * _slideDistanceM;
    }
}