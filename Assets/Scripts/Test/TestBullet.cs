using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameInput;
using UnityEngine.UIElements;

public class TestBullet : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField, Tooltip("èoåªà íuçÇÇ≥ï‚ê≥")] private float _popPositionHeightCorrection;
    public GameObject _bulletPrefab;
    private Vector3 _bulletPopPos;

    private void Reset()
    {
        _player = GetComponent<Transform>();
    }

    void Update()
    {
        if (ConfirmAction.s_Instance.InputAction.Player.Magic.WasPressedThisFrame())
        {
            _bulletPopPos = _player.position + _player.forward * 2;// ÉJÉÅÉâÇ…å©Ç¶ÇÈèÍèäÇ≈ê∂ê¨ÇµÇΩÇ¢ÇÃÇ≈ z-1
            _bulletPopPos += Vector3.up * _popPositionHeightCorrection;
            GameObject bullet = Instantiate(_bulletPrefab, _bulletPopPos, Quaternion.FromToRotation(Vector3.forward, _player.forward));
            bullet.GetComponent<TestShoot>().Shoot();
        }
    }
}