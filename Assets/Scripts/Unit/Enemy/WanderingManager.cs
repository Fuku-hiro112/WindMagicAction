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
            Assert.IsNotNull(_wanderings[i].Transform, $"_wandering[{i}]がNullです。");
        }
    }
    void Update()
    {
        // _enemyActionに_wanderingTransformの値を割り当てる

        // 条件
        // 同じ場所を複数のEnemyに割り当てないようにする
        // 待機時間の差(0.5～２秒)をランダムで作る
    }
    /// <summary>
    /// 使用しておらず、今と同じ出ない場合のWanderingをランダムに設定する
    /// </summary>
    /// <returns>使用していないWanderingを返す</returns>
    public Wandering AssignNotUseWandering(Transform currentWanderingTrans = null)
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
        // 使用中に
        returnWandering.InUse = true;

        return returnWandering;
    }

    [Serializable]
    public class RandomPointInCircle
    {
        [SerializeField]
        private float _radius = 5f; // 円の半径

        // 円内からランダムな座標を取得する関数
        public Vector2 GetRandomPointInCircle(Vector3 centerPoint)
        {
            // 円内のランダムな座標を計算
            float x = centerPoint.x + Mathf.Cos(RandomAngle()) * _radius;
            float z = centerPoint.z + Mathf.Sin(RandomAngle()) * _radius;

            return new Vector3(x, 0, z);
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
    public Wandering(Transform transform)
    {
        Transform = transform;
        InUse = false;
    }
    public Transform Transform;
    [NonSerialized]
    public bool InUse;
}
