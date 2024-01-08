using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAction : MonoBehaviour
{
    GameObject Player;
    [SerializeField] Vector3 CamDir = new Vector3(0.0f, 3.0f, -2.5f);
    [SerializeField] Vector3 Offset = new Vector3(0.0f, 1.5f, 0.0f);
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }
    void FixedUpdate()
    {
        transform.position = Player.transform.position + CamDir;
        transform.LookAt(Player.transform.position + Offset);
    }
}
