using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TestShoot : MonoBehaviour
{
    enum State
    {
        Move,
        Stop
    }
    [SerializeField] float _period = 1f;// 残り時間
    [SerializeField] private float _targetY = 1f;
    [SerializeField] float _accelerationUpperLimit = 100f;
    State _bulletState;
    Vector3 _velocity;
    Vector3 _position;
    Vector3 _stopPos;
    Vector3 _targetHitPos;
    private Transform _target = null;
    private Vector3 _cameraForward;

    public void Shoot(Transform target)
    {
        _cameraForward = Camera.main.transform.forward;
        _target = target;
        _bulletState = State.Move;
    }
    /*
    void OnCollisionEnter(Collision other)
    {
        igaguriState = State.Stop;
        if (other.gameObject.CompareTag("Target"))
        {
            //transform.parent = other.transform;
            igaguriState = State.Stop;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            GetComponent<ParticleSystem>().Play();
        }
        else if (other.gameObject.CompareTag("Bullet") || other.gameObject.CompareTag("Player"))
        {

        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    */
    void Start()
    {
        _position = transform.localPosition;
        /*GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
        float minimumDistance = Mathf.Infinity;
        foreach (GameObject target in targets)
        {
            float distanece = (target.transform.position - transform.position).sqrMagnitude;

            if (minimumDistance > distanece)
            {
                minimumDistance = distanece;
                _target = target.transform;
            }
        }*/
        _velocity = Vector3.zero;
    }

    void Update()
    {
        switch (_bulletState)
        {
            case State.Move:
                if (_target == null)
                {
                    float speed = 0.3f / _period;
                    transform.position += _cameraForward * speed;// Camera角度にするとおかしい
                }
                else
                {
                    var targetPosition = _target.position;
                    _targetHitPos = new Vector3(targetPosition.x, targetPosition.y + _targetY, targetPosition.z);

                    var acceleration = Vector3.zero;
                    var diff = _targetHitPos - _position;
                    acceleration += (diff - _velocity * _period) * 2f // d = vt + 1/2at^2 を　a= 2(d-vt) / t^2 に
                                     / (_period * _period);

                    _period -= Time.deltaTime;

                    // 加速度の上限値を設定　これにより必ず当たらなくなる
                    if (acceleration.magnitude > _accelerationUpperLimit)
                    {
                        acceleration = acceleration.normalized * _accelerationUpperLimit;
                    }

                    // 運動方程式
                    _velocity += acceleration * Time.deltaTime;
                    _position += _velocity * Time.deltaTime;

                    transform.position = _position;
                    //Debug.Log(transform.position + " " + _position);
                    //Debug.Log("加速度 "+ Pythagorean(acceleration)+" ");
                }
                break;
            case State.Stop:
                break;
        }
    }
    float Pythagorean(Vector3 vec)
    {
        float v = Mathf.Sqrt
            (
            Mathf.Pow(vec.x, 2) + Mathf.Pow(vec.x, 2) + Mathf.Pow(vec.x, 2)
            );
        return v;
    }
    void StopIgaguri()
    {
        _bulletState = State.Stop;
        _stopPos = transform.position;
    }
}