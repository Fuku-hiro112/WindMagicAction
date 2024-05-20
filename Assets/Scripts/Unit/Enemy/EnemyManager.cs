using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemyManager : MonoBehaviour
{
    public List<GameObject> EnemyList { get; private set; } 

    void Start()
    {
        var enemys = GameObject.FindGameObjectsWithTag("Enemy");
        EnemyList = new List<GameObject>(enemys);
    }

    /*void Update()
    {
        //NOTE: UpdateでFindはしたくない！！　絶対重い！！
    }*/

    // 新しい敵をリストに追加するメソッド
    public void AddEnemy(GameObject enemy)
    {
        EnemyList.Add(enemy);
    }

    // 敵をリストから削除するメソッド
    public void RemoveEnemy(GameObject enemy)
    {
        EnemyList.Remove(enemy);
    }

    /// <summary>
    /// 敵が存在しているか
    /// </summary>
    /// <returns></returns>
    public bool HasExistsEnemy() => EnemyList.Count > 0;
    /// <summary>
    /// オブジェクトから一番近くの敵を返す
    /// </summary>
    /// <returns>近くの敵のTransformを返す</returns>
    public Transform NearestEnemy(Transform objTransform)
    {
        Transform minEnemy = null;

        // 最小距離を記録
        float minDistance = 999999;

        foreach (var enemy in EnemyList)
        {
            // Objと敵の距離
            float distance = Vector3.Distance(enemy.transform.position, objTransform.position);

            // 距離が短いなら
            if (minDistance > distance)
            {
                // 最小距離,Transformの上書き
                minDistance = distance;
                minEnemy = enemy.transform;
            }
        }

        Assert.IsNotNull(minEnemy, $"{this}のminEnemyがNUllです。");
        return minEnemy;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="objTransform">何から近いか</param>
    /// <param name="range">範囲</param>
    /// <returns>範囲内の敵のTransformのリストを返す</returns>
    public List<Transform> GetAllNearbyEnemies(Transform objTransform, float range)
    {
        List<Transform> returnList = new List<Transform>();

        foreach (var enemy in EnemyList)
        {
            //NOTE: 自分自身をリストに入れないために確認
            if (enemy.transform == objTransform) continue;
            
            // Objと敵の距離
            float distance = Vector3.Distance(enemy.transform.position, objTransform.position);

            // 範囲内ならリストに追加
            if (distance <= range)
            {
                returnList.Add(enemy.transform);
            }
        }

        return returnList;
    }
}
