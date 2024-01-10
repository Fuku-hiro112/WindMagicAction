using GameInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerMove : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 4.0f;
    private ConfirmAction _confirmAction;

    void Start()
    {
        _confirmAction = ConfirmAction.s_Instance;
    }

    void FixedUpdate()
    {
        Vector3 direction = _confirmAction.MoveDirection;
        var horizontalRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
        // “ü—Í•ûŒü‚ÖˆÚ“®‚·‚é
        var vec = horizontalRotation * direction;
        transform.position += vec * _moveSpeed * Time.fixedDeltaTime;
    }
}
