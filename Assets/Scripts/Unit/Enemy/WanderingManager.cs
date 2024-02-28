using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Must;
using Unit;

public class WanderingManager : MonoBehaviour
{
    [SerializeField] private GameObject _wanderingPosStorage;
    [SerializeField] private EnemyActionBase[] _enemyActions;
    public RandomPointInCircle CircleRandomPoint;

    private Wandering[] _wanderings;

    private void Reset()
    {

    }
    private void Awake()
    {
        CircleRandomPoint = new RandomPointInCircle();
    }
    void Start()
    {
        _wanderings = new Wandering[_wanderingPosStorage.transform.childCount];
        for (int i = 0; i < _wanderings.Length; i++)
        {
            _wanderings[i] = new Wandering(_wanderingPosStorage.transform.GetChild(i));
            Assert.IsNotNull(_wanderings[i].Transform, $"_wandering[{i}]��Null�ł��B");
        }
    }
    void Update()
    {
        // _enemyAction��_wanderingTransform�̒l�����蓖�Ă�

        // ����
        // �����ꏊ�𕡐���Enemy�Ɋ��蓖�ĂȂ��悤�ɂ���
        // �ҋ@���Ԃ̍�(0.5�`�Q�b)�������_���ō��
    }
    /// <summary>
    /// �g�p���Ă��炸�A���Ɠ����o�Ȃ��ꍇ��Wandering�������_���ɐݒ肷��
    /// </summary>
    /// <returns>�g�p���Ă��Ȃ�Wandering��Ԃ�</returns>
    public Wandering AssignNotUseWandering(Transform currentWanderingTrans = null)
    {
        List<Wandering> NotUsedwanderingList = new List<Wandering>(_wanderings.Length);
        for (int index = 0; index < _wanderings.Length; index++)
        {
            // ���݂�Wandering�Ɠ����ꍇ
            if (currentWanderingTrans == _wanderings[index].Transform)
            {
                _wanderings[index].InUse = false;
            }
            // �g�p���Ă��Ȃ����A���݂�Wandering�Ɠ����łȂ��ꍇ
            else if (_wanderings[index].InUse == false)
            {
                NotUsedwanderingList.Add(_wanderings[index]);
            }
        }

        // �g�p���Ă��Ȃ��ʒu���烉���_���őI��
        Wandering returnWandering =
            NotUsedwanderingList[UnityEngine.Random.Range(0, NotUsedwanderingList.Count)];
        // �g�p����
        returnWandering.InUse = true;

        return returnWandering;
    }

    [Serializable]
    public class RandomPointInCircle
    {
        [SerializeField]
        private float _radius = 5f; // �~�̔��a

        // �~�����烉���_���ȍ��W���擾����֐�
        public Vector2 GetRandomPointInCircle(Vector3 centerPoint)
        {
            // �~���̃����_���ȍ��W���v�Z
            float x = centerPoint.x + Mathf.Cos(RandomAngle()) * _radius;
            float z = centerPoint.z + Mathf.Sin(RandomAngle()) * _radius;

            return new Vector3(x, 0, z);
        }
        /// <summary>
        /// 0����2��(360��)�܂ł̊p�x�������_���Ɏ擾
        /// </summary>
        /// <returns>�p�x�����W�A���œn��</returns>
        private float RandomAngle() => UnityEngine.Random.Range(0f, Mathf.PI * 2f);
    }
}
[Serializable]
public struct Wandering
{
    public Wandering(Transform transform)
    {
        Transform = transform;
        InUse = false;
    }
    public Transform Transform;
    [NonSerialized]
    public bool InUse;
}
