using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookPosition : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _heightM = 1.2f; // 注視点の高さ[m]

    private void Reset()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Start()
    {
        
    }
    void Update()
    {
        
    }
}
