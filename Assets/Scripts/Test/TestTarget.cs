using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class TestTarget : MonoBehaviour
{
    [ReadOnly]public Vector3 TargetPosition;
    [ReadOnly]public Transform TargetGameObject;

    [SerializeField]private GameObject _targetPosObj;
    private RaycastHit hit;
    [SerializeField] float distance;
    [SerializeField] float duration;

    private Camera _camera;
    [SerializeField]private LayerMask _ignoreLayer;

    private void Reset()
    {
        _ignoreLayer = LayerMask.GetMask("PlayerSide");
    }
    void Start()
    {
        _camera = Camera.main;
    }
    void Update()
    {
        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);//�@�J��������L�т�Ray�𐶐�
        Debug.DrawRay(ray.origin, ray.direction * distance, UnityEngine.Color.red, duration, false);

        if (Physics.Raycast(ray, out hit, distance, _ignoreLayer)/*�ARay�𓊎˂��A�������炩�̃I�u�W�F�N�g�ƏՓ˂�����*/)
        {
            if (hit.collider.CompareTag("Enemy")/*�B�Փ˂�������̃^�O��"Enemy"��������*/)
            {
                //�C����I�u�W�F�N�g���擾
                SetTarget(obj: hit.collider.transform);
            }
            else
            {
                SetTarget(pos: hit.point);
            }

            _targetPosObj.transform.position = hit.point;
        }
        else
        {
            SetTarget();
        }

    }
    private void SetTarget(Vector3 pos = default, Transform obj = null)
    {
        TargetPosition = pos;
        TargetGameObject = obj;
    }
    private void OnGUI()
    {
        GUILayout.Label($"TargetPosition : {TargetPosition}");
        GUILayout.Label($"TargetGameObject : {TargetGameObject}");
    }
}
/*
public class Dot : MonoBehaviour
{
    [SerializeField]
    Transform targetTransform;

    Transform cameraDirection;

    void Start()
    {
        cameraDirection = this.gameObject.GetComponent<Transform>();
    }

    void Update()
    {
        //�^�[�Q�b�g����J�����̕����֐��K�������x�N�g�����쐬
        Vector3 targetToCameraDirection_N = (cameraDirection.position - targetTransform.position).normalized;

        //���K�������x�N�g���̓��ς����ȉ��Ȃ猩�����Ƃɂ���
        if (Vector3.Dot(targetToCameraDirection_N, cameraDirection.forward.normalized) < -0.9)
        {
            print("�����I");
        }
    }
}
*/
