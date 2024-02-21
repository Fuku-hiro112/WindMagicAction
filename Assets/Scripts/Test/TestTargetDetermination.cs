using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class TestTargetDetermination : MonoBehaviour
{
    // �������g
    [SerializeField] private Transform _player;

    // �^�[�Q�b�g
    private List<Transform> _targetPositionList;

    // ����p�i�x���@�j
    [SerializeField] private float _sightAngle;

    // ���E�̍ő勗��
    [SerializeField] private float _maxDistance = float.PositiveInfinity;

    // ���E�̉~���̒��p
    [SerializeField, Tooltip("���E�̉~���̒��p")] private float _viewConeApexAngle = 155f;

    // �G
    [SerializeField] private TestEnemyManager _enemyManager;
    // �]���_�̖��_
    private const float c_maxPoint = 100;
    [SerializeField,Tooltip("�W������̋����_���̊���"), Range(0, c_maxPoint)] private int _aimDistanceRatio;

    // �^�[�Q�b�g
    public ReactiveProperty<Transform> Target { get; private set; } = new ReactiveProperty<Transform>();
    
    // �����Ă��邩
    private bool isVisible = false;

    // �m�F�p�ϐ�
    private float _maxPoint = 100;
    private float _maxdis = 100;
    private float _maxscr = 100;

    private void Start()
    {
        Target.Subscribe(obj =>
        {
            // �J�[�\���̕\��
            /*
            Debug.Log(obj?.name + "���^�[�Q�b�g�ɂȂ�܂����B");
            Debug.Log($"dis:{_maxdis}+ screen:{_maxscr} = {_maxPoint}");
            Material material = new Material(Shader.Find("Standard"));
            material.color = Color.blue;
            if(obj != null)
            obj.GetComponent<Renderer>().material = material;
            */
        });
    }

    private void Update()
    {
        //�S�Ă̓G�̃��X�g����^�[�Q�b�g�����߂�
        Transform target = null;
        float maxPoint = 0;

        _enemyManager.EnemyList.Where(obj =>
        {
            // ����ő勗���͈͓��ɂ��邩�ǂ���
            var distance = _player.position - obj.transform.position;
            return distance.sqrMagnitude < _maxDistance * _maxDistance;//NOTE: magnitude���Ə捪�̌v�Z������A���x�Ƒ��x���������ߏ捪���g�킸sqrMag�E2����g���Čv�Z���Ă���
        }).ToList()
        .ForEach(obj =>
        {
            //�^�[�Q�b�g����J�����̕����֐��K�������x�N�g�����쐬
            Vector3 targetToCameraDirection = (Camera.main.transform.position - obj.transform.position).normalized;
            float cos153 = -0.89f;// ��cos153��

            // �J�����̎��E�ɂ��邩�ǂ���
            if (Vector3.Dot(targetToCameraDirection, Camera.main.transform.forward.normalized) < cos153)//NOTE: .normalized��t���邱�Ƃɂ��A���ς̌v�Z��|a||b|�x�N�g����1�ɂȂ�cos�Ƃ݂̂̌v�Z�ŗǂ��Ȃ�
            {//TODO: �{�Ԃ̓^�[�Q�b�gCanvas��\���ɂ���
                //Debug.Log("������");
                isVisible = true;

                float point = 0;
                // �G�Ƃ̋�������_�����o��
                float distanceMaxPoint = c_maxPoint - _aimDistanceRatio;
                var cameraDistance = obj.transform.position - Camera.main.transform.position;
                // �����|�C���g���v
                float distancePoint = distanceMaxPoint - cameraDistance.magnitude * (distanceMaxPoint / _maxDistance); // �ő�_�� - �J�����Ƃ̋����~(�ő�_��/�ő压�싗��) = �߂���Γ_������

                // �X�N���[�����W
                Vector3 objScreen = new Vector3(
                    Camera.main.WorldToViewportPoint(obj.transform.position).x
                  , Camera.main.WorldToViewportPoint(obj.transform.position).y * 2 - 0.5f // �����Z���̂ŕ␳�@NOTE:�Q�{(�c�����̂Q�{������)����ƃZ���^�[�ɏƏ�������Ȃ��Ȃ�̂�2�{���Ă���-0.5f���Ă���
                  , 1f);
                // �X�N���[�����W��ʒ���
                Vector3 senter = new Vector3(0.5f, 0.5f, 1f);
                // �X�N���[���|�C���g���v
                float screenPoint = _aimDistanceRatio - (senter - objScreen).magnitude * 100;
                // ���v�|�C���g
                point = distancePoint + screenPoint;

                if (maxPoint < point)
                {
                    if (!IsObjectsDuringObstacle(obj.transform,Camera.main.transform))// �J�����ƃI�u�W�F�N�g�̊Ԃɏ�Q�������邩
                    {
                        // ���
                        maxPoint = point;      // ���v�|�C���g
                        target = obj.transform;// �^�[�Q�b�g�I�u�W�F�N�g

                        //NOTE: �m�F�p����������
                        Debug.Log(obj.name);
                        _maxPoint = maxPoint;
                        _maxdis = distancePoint;
                        _maxscr = screenPoint;
                    }
                }
            }
            else isVisible = false;
        });

        // �����ڊm�F�p�@target�͐@����ȊO��
        if (Target.Value != target && target != null)
        {
            Target.Value = target;
            /*
            _enemyManager.EnemyList.ForEach(obj =>
            {
                Material material = new Material(Shader.Find("Standard"));
                // �}�e���A���̐F�́Atarget�Ɠ����ł���΁@�F�@�Ⴄ�ꍇ�́@���F
                material.color = (obj.transform == target) ? Color.blue : Color.white;
                obj.GetComponent<Renderer>().material = material;
            });*/
        }// target���f
        else if (Target.Value != null && target == null)
        {
            Target.Value = target;
        }
    }
    /// <summary>
    /// �I�u�W�F�N�g�Ԃɏ�Q�������邩�ǂ���
    /// </summary>
    /// <param name="targetTransform">����Obj��</param>
    /// <param name="startTransform">�ǂ̃I�u�W�F�N�g����</param>
    /// <returns>��Q���������ture</returns>
    private bool IsObjectsDuringObstacle(Transform targetTransform, Transform startTransform )
    {
        bool result = true;
        // Ray���΂����p
        Vector3 heightCorrection = Vector3.up * 0.5f;// Ray�̍����␳�l
        Vector3 targetPoint = targetTransform.position + heightCorrection; //NOTE: Ray���n�ʂɏՓ˂��邽�ߍ�����␳
        Vector3 objDirection = targetPoint - startTransform.position;
        RaycastHit hit;
        // PlayerSide�ȊO�ɓ�����LayerMask
        int layerMask = 1 << LayerMask.NameToLayer("PlayerSide");
        layerMask = ~layerMask;

        Debug.DrawLine(startTransform.position, targetPoint, Color.red, 0.1f);
        if (Physics.Raycast(startTransform.position, objDirection, out hit, _maxDistance,layerMask))// �J��������I�u�W�F�N�g��Ray���΂�
        {//TODO: ��Q�����C���[�݂̂ɓ�����悤�ɂ��悤

            // �I�u�W�F�N�g�ȊO�ɓ������Ă����
            if (hit.collider.gameObject.name != targetTransform.gameObject.name)
            {
                result = true;
                Debug.Log($"{hit.collider.gameObject.name}�ɓ������Ă���");
            }
            else 
            {
                Debug.Log("�����ƃ^�[�Q�b�g�ɓ������Ă�");
                result = false; 
            }
        }

        // ���ʂ�Ԃ�
        return result;
    }

    #region Debug

    // ���E����̌��ʂ�GUI�o��
    private void OnGUI()
    {
        // ���E����
        //var isVisible = IsVisible();

        // ���ʕ\��
        GUI.Box(new Rect(20, 20, 150, 23), $"isVisible = {isVisible}");
    }

    #endregion
}