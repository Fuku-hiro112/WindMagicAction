using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IState
{
    private PlayerController _playerController;

    public IdleState(PlayerController playerController)
    {
        _playerController = playerController;
    }

    public void Enter()
    {
        
    }

    public void Update()
    {
        
    }

    public void Exit()
    {

    }
}
