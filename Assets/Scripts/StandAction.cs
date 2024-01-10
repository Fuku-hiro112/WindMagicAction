using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; //新Inputシステムの利用に必要

public class StandAction : MonoBehaviour
{
    [SerializeField] private float _bias = 8.0f;
    [SerializeField] private float _rotBias = 120.0f;
    [SerializeField] private Vector3 _camDir = new Vector3(0.0f, 4.0f, -4.0f);
    [SerializeField] private float _closeBias = 0.75f; //近寄る割合
    [SerializeField] private float _closeTime = 5.0f; //近寄る時間
    [SerializeField] private float _minCameraRotate;
    [SerializeField] private float _maxCameraRotate;
    private GameObject _player; //プレイヤー
    private float _closeRatio = 1.0f; //距離の倍率
    private float _elapsed = 0.0f; //経過時間

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player"); //プレイヤーを取得
    }
    public void Attack()
    {
        //_elapsed = _closeTime;
    }
    private void Update()
    {
        /*
        _elapsed -= Time.deltaTime;
        _elapsed = Mathf.Clamp(_elapsed, 0.0f, _closeTime);
        _closeRatio = Mathf.Lerp(_closeRatio, //現在の倍率
                                 (_elapsed == 0.0f) ? 1.0f : _closeBias, //目指す倍率
                                 Time.deltaTime);

        Camera.main.gameObject.transform.localPosition = _camDir * _closeRatio;
        Camera.main.gameObject.transform.LookAt(transform.position + Vector3.up);
        */
    }
    private void FixedUpdate()
    {
        /*
        if (transform.rotation.x < _minCameraRotate) 
            transform.rotation = Quaternion.Euler(_minCameraRotate, transform.rotation.y, transform.rotation.z);
        if (transform.rotation.x > _maxCameraRotate) 
            transform.rotation = Quaternion.Euler(_maxCameraRotate, transform.rotation.y, transform.rotation.z);
        */
        transform.position = Vector3.Lerp(
            transform.position, //現在の位置
            _player.transform.position, //到着したい位置
            _bias * Time.fixedDeltaTime); //マイルドな考慮バイアス
        /*
        if (Gamepad.current == null || Mathf.Abs(Gamepad.current.rightStick.ReadValue().x) < 0.05f)
        {
            return; //十分にジョイスティックが倒れていない判定
        }
        transform.Rotate(
        -Gamepad.current.rightStick.ReadValue().y * _rotBias * Time.fixedDeltaTime,
        Gamepad.current.rightStick.ReadValue().x * _rotBias * Time.fixedDeltaTime, 
        0);
        */
    }
}
