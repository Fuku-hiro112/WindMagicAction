using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookPosition : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _heightM = 1.2f; // íçéãì_ÇÃçÇÇ≥[m]

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
