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

    /// <summary>
    /// 弾の初期設定
    /// </summary>
    /// <param name="target"></param>
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
            transform.position += _InjectionDirection * _speed * Time.deltaTime;// Camera角度にするとおかしい
        }
        else
        {
            Vector3 targetPosition = _target.position;
            _targetHitPos = new Vector3(targetPosition.x, targetPosition.y + _targetY, targetPosition.z);

            Vector3 acceleration = Vector3.zero;
            // 運動方程式の実装（等加速度直線運動）
            Vector3 diff = _targetHitPos - _position;
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
        }
    }
}