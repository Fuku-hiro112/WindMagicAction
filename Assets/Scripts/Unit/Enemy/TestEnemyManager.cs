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
        // Update��Find�͂������Ȃ��I�I�@��Ώd���I�I
    }*/

    // �V�����G�����X�g�ɒǉ����郁�\�b�h
    public void AddEnemy(GameObject enemy)
    {
        EnemyList.Add(enemy);
    }

    // �G�����X�g����폜���郁�\�b�h
    public void RemoveEnemy(GameObject enemy)
    {
        EnemyList.Remove(enemy);
    }
}
