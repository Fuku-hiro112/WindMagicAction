using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameInput;

[RequireComponent(typeof(Camera))]// Camera�R���|�[�l���g���K�v�@�Ȃ��ꍇ�͎����I�ɒǉ�
public class TestCameraMove2 : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _distanceToPlayerM = 2f;    // �J�����ƃv���C���[�Ƃ̋���[m]
    [SerializeField] private float _slideDistanceM = 0f;       // �J���������ɃX���C�h������G�v���X�̎��E�ցC�}�C�i�X�̎�����[m]
    [SerializeField] private float _heightM = 1.2f;            // �����_�̍���[m]
    [SerializeField] private float _rotationSensitivity = 100f;// ���x
    private ConfirmAction _confirmAction;

    private void Reset()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Start()
    {
        if (_target == null)
        {
            Debug.LogError("�^�[�Q�b�g���ݒ肳��Ă��Ȃ�");
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
        var rotY = -lookDerection.y * Time.fixedDeltaTime * _rotationSensitivity;// ����͂������A�������������-(�}�C�i�X��t���Ă���)

        var lookAt = _target.position + Vector3.up * _heightM;

        // ��]
        transform.RotateAround(lookAt, Vector3.up, rotX);// ����]
        // �J�������v���C���[�̐^���^���ɂ���Ƃ��ɂ���ȏ��]�����Ȃ��悤�ɂ���
        if (transform.forward.y > 0.9f && rotY < 0 || transform.forward.y < -0.9f && rotY > 0)
        {
            rotY = 0;
        }
        transform.RotateAround(lookAt, transform.right, rotY);// �c��]

        // �J�����ƃv���C���[�Ƃ̊Ԃ̋����𒲐�
        transform.position = lookAt - transform.forward * _distanceToPlayerM;

        // �����_�̐ݒ�
        transform.LookAt(lookAt);

        // �J���������ɂ��炵�Ē������J����
        transform.position +=  transform.right * _slideDistanceM;
    }
}