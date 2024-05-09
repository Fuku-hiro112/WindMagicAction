using UnityEngine;

public class HomingShoot : MonoBehaviour
{
    [SerializeField] float _period = 1f;// 目標位置に着弾までの残り時間
    [SerializeField] private float _targetY = 2f;
    [SerializeField] float _accelerationUpperLimit = 100f;

    [SerializeField] private float _speed = 2;
    [SerializeField] private Vector3 _velocity = new Vector3(0,10,-10);
    private Vector3 _position;
    private Vector3 _targetHitPos;
    private Transform _target = default;
    private Vector3 _InjectionDirection;
    private Camera _camera;//NOTE: Camera.mainで取るとShake中カメラの切り替えでバグるので

    private void Awake()
    {
        _camera = Camera.main;
    }
    void Start()
    {
        _position = transform.localPosition;
    }

    public void Initialize(Transform target)
    {
        _InjectionDirection = _camera.transform.forward;
        _target = target;
        _velocity = _camera.transform.TransformDirection(_velocity); 
    }

    void Update()
    {
        if (_target == default)
        {
            transform.position += _InjectionDirection * _speed;// Camera角度にするとおかしい
        }
        else
        {
            var targetPosition = _target.position;
            _targetHitPos = new Vector3(targetPosition.x, targetPosition.y + _targetY, targetPosition.z);

            var acceleration = Vector3.zero;
            // 運動方程式の実装（等加速度直線運動）
            var diff = _targetHitPos - _position;
            acceleration += (diff - _velocity * _period) * 2f // d = v0t + 1/2at^2(等加速度直線運動) を　a= 2(d-vt) / t^2 に
                             / (_period * _period);

            _period -= Time.deltaTime;

            // 回避可能  加速度の上限値を設定　これにより必ず当たらなくなる
            if (acceleration.magnitude > _accelerationUpperLimit)
            {
                acceleration = acceleration.normalized * _accelerationUpperLimit;
            }

            // 運動方程式の実装　
            _velocity += acceleration * Time.deltaTime;// 速度m/s = 加速度m/s^2 * deltaTime
            _position += _velocity * Time.deltaTime;   // 位置m   = 速度m/s　   * deltaTime

            transform.position = _position;
            //Debug.Log("加速度 "+ Pythagorean(acceleration)+" ");
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
}