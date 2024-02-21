using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemyManager : MonoBehaviour
{
    public List<GameObject> EnemyList { get; private set; } 

    void Start()
    {
        var enemys = GameObject.FindGameObjectsWithTag("Enemy");
        EnemyList = new List<GameObject>(enemys);
    }

    /*void Update()
    {
        // UpdateでFindはしたくない！！　絶対重い！！
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
}
