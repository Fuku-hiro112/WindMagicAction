using GameInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        if (ConfirmAction.s_Instance.InputAction.Player.Decision.WasPerformedThisFrame())
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}
