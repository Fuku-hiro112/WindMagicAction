using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameInput;
using UnityEngine.Assertions;
using Unit;

[RequireComponent(typeof(Camera))]// Cameraコンポーネントが必要　ない場合は自動的に追加
public class CameraMove : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _bias = 1.0f;
    [SerializeField] private float _distanceToPlayerM = 2f;    // カメラとプレイヤーとの距離[m]
    [SerializeField] private float _slideDistanceM = 0f;       // カメラを横にスライドさせる；プラスの時右へ，マイナスの時左へ[m]
    [SerializeField] private float _heightM = 1.2f;            // 注視点の高さ[m]
    [SerializeField] private float _rotationSensitivity = 50f;// 感度
    private ConfirmAction _confirmAction;
    private UnitStats _unitStats;
    private Vector3 _lookAt;

    private void Reset()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Start()
    {
        _unitStats = GameObject.FindGameObjectWithTag("Player")?.GetComponent<UnitStats>();
        _confirmAction = ConfirmAction.s_Instance;
        ResetPosition();// カメラ位置リセット

        Assert.IsNotNull(_target,    $"{this}のターゲットが設定されていない。");
        Assert.IsNotNull(_unitStats, $"{this}の_combatActionがNullです。");
    }
    /// <summary>
    /// カメラ位置リセット
    /// </summary>
    public void ResetPosition()
    {
        _lookAt = _target.position + Vector3.up * _heightM;
        transform.position = _lookAt - transform.forward * _distanceToPlayerM;
    }
    void FixedUpdate()
    {

        Vector2 lookDerection = _confirmAction.LookDirection;
        float rotX = lookDerection.x * Time.fixedDeltaTime * _rotationSensitivity;
        float rotY = -lookDerection.y * Time.fixedDeltaTime * _rotationSensitivity;// 上入力した時、上を向きたい為-(マイナスを付けている)

        _lookAt = _target.position + Vector3.up * _heightM;// カメラの注視点（中心点）

        // 回転
        transform.RotateAround(_lookAt, Vector3.up, rotX);// 横回転
        // カメラがプレイヤーの真上や真下にあるときにそれ以上回転させないようにする
        if (transform.forward.y > 0.9f && rotY < 0 /*真上*/|| 
            transform.forward.y < -0.9f && rotY > 0/*真下*/) 
            rotY = 0;
        transform.RotateAround(_lookAt, transform.right, rotY);// 縦回転 

        // カメラとプレイヤーとの間の距離を調整
        Vector3 targetPos = _lookAt - transform.forward * _distanceToPlayerM;
        //transform.position = Vector3.Lerp(transform.position, targetPos, _bias * Time.fixedDeltaTime);//HACK: 上下を向いた時歩くとだんだん初期位置に戻る
        
        // 注視点の設定
        transform.LookAt(_lookAt);

        // _slideDistanceM分、カメラを横にずらす
        transform.position = transform.position + transform.right * _slideDistanceM;
    }

    /* Assert.IsNotNullっていう便利機能があったのでいらなくなった（; ;）
    public static void ErrorLog<T>(T check, string message)
    {
        if (check == null)
        {
            Debug.LogError(message);
            Application.Quit();
        }
    }*/
}