using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unit;

public class WanderingManager : MonoBehaviour
{
    [SerializeField] private GameObject _wanderingPosStorage;
    [SerializeField] private EnemyControllerBase[] _enemyActions;
    public RandomPointInCircle CircleRandomPoint;

    private Wandering[] _wanderings;

    private void Awake()
    {
        CircleRandomPoint = new RandomPointInCircle();
        _wanderings = new Wandering[_wanderingPosStorage.transform.childCount];
        for (int i = 0; i < _wanderings.Length; i++)
        {
            _wanderings[i] = new Wandering(i, _wanderingPosStorage.transform.GetChild(i));
            Assert.IsNotNull(_wanderings[i].Transform, $"_wandering[{i}]がNullです。");
        }
    }
    /// <summary>
    /// 使用していない徘徊地点をランダムに返す
    /// </summary>
    /// <param name="currentWanderingTrans">現在使用中のWandering.Transformを入れる（初期化の場合はNullを入れる）</param>
    /// <returns>徘徊地点</returns>
    public Wandering AssignNotUseWandering(Transform currentWanderingTrans)
    {
        List<Wandering> NotUsedwanderingList = new List<Wandering>(_wanderings.Length);
        for (int index = 0; index < _wanderings.Length; index++)
        {
            // 現在のWanderingと同じ場合
            if (currentWanderingTrans == _wanderings[index].Transform)
            {
                _wanderings[index].InUse = false;
            }
            // 使用していない時、現在のWanderingと同じでない場合
            else if (_wanderings[index].InUse == false)
            {
                NotUsedwanderingList.Add(_wanderings[index]);
            }
        }

        // 使用していない位置からランダムで選ぶ
        Wandering returnWandering =
            NotUsedwanderingList[UnityEngine.Random.Range(0, NotUsedwanderingList.Count)];

        // 使用中にする
        _wanderings[returnWandering.ID].InUse = true;

        return returnWandering;
    }

    [Serializable]
    public class RandomPointInCircle
    {
        [SerializeField]
        private float _radius = 5f; // 円の半径

        // 円内からランダムな座標を取得する関数
        public Vector3 GetRandomPointInCircle(Vector3 centerPoint, Transform trans)
        {
            // 円内のランダムな座標を計算
            float x = centerPoint.x + Mathf.Cos(RandomAngle()) * _radius;
            float z = centerPoint.z + Mathf.Sin(RandomAngle()) * _radius;

            return new Vector3(x, trans.position.y, z);
        }

        /// <summary>
        /// 0から2π(360°)までの角度をランダムに取得
        /// </summary>
        /// <returns>角度をラジアンで渡す</returns>
        private float RandomAngle() => UnityEngine.Random.Range(0f, Mathf.PI * 2f);
    }
}
[Serializable]
public struct Wandering
{
    public Wandering(int index ,Transform transform)
    {
        ID = index;
        Transform = transform;
        InUse = false;
    }
    public int ID;
    // 位置
    public Transform Transform;
    
    // 使用中かどうか
    [NonSerialized]
    public bool InUse;
}
