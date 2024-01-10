using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; //�VInput�V�X�e���̗��p�ɕK�v

public class StandAction : MonoBehaviour
{
    [SerializeField] private float _bias = 8.0f;
    [SerializeField] private float _rotBias = 120.0f;
    [SerializeField] private Vector3 _camDir = new Vector3(0.0f, 4.0f, -4.0f);
    [SerializeField] private float _closeBias = 0.75f; //�ߊ�銄��
    [SerializeField] private float _closeTime = 5.0f; //�ߊ�鎞��
    [SerializeField] private float _minCameraRotate;
    [SerializeField] private float _maxCameraRotate;
    private GameObject _player; //�v���C���[
    private float _closeRatio = 1.0f; //�����̔{��
    private float _elapsed = 0.0f; //�o�ߎ���

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player"); //�v���C���[���擾
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
        _closeRatio = Mathf.Lerp(_closeRatio, //���݂̔{��
                                 (_elapsed == 0.0f) ? 1.0f : _closeBias, //�ڎw���{��
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
            transform.position, //���݂̈ʒu
            _player.transform.position, //�����������ʒu
            _bias * Time.fixedDeltaTime); //�}�C���h�ȍl���o�C�A�X
        /*
        if (Gamepad.current == null || Mathf.Abs(Gamepad.current.rightStick.ReadValue().x) < 0.05f)
        {
            return; //�\���ɃW���C�X�e�B�b�N���|��Ă��Ȃ�����
        }
        transform.Rotate(
        -Gamepad.current.rightStick.ReadValue().y * _rotBias * Time.fixedDeltaTime,
        Gamepad.current.rightStick.ReadValue().x * _rotBias * Time.fixedDeltaTime, 
        0);
        */
    }
}
