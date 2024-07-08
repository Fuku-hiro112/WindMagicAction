using Cysharp.Threading.Tasks;
using System;
using UniRx;
using UnityEngine;

public class MagicShoot : MonoBehaviour
{
    // 0:Charge 1:Flame 2:Burst
    [SerializeField, Tooltip("0:Charge 1:Flame 2:Burst")] 
    private GameObject[] _burstEffectObjs = new GameObject[3];
    [SerializeField] private GameObject _slashObj;
    [SerializeField] private GameObject _hitEffectObj;

    [SerializeField] private float _burstDestroyTime = 3f;
    [SerializeField] private float _hitEffectDestroyTime = 1f;
    [SerializeField] private float _flameMoveSpeed = 10f;
    [SerializeField] private float _slashMoveSpeed = 30f;
    [SerializeField] private float _destroyTime = 3f;

    private Camera _camera;

    void Start()
    {
        _camera = Camera.main;
        OnReset();
    }
    /// <summary>
    /// 初期化
    /// </summary>
    private void OnReset()
    {
        _burstEffectObjs[0].SetActive(false);
    }

    /// <summary>
    /// 貯め
    /// </summary>
    public void Charge()
    {
        _burstEffectObjs[0].SetActive(true);
    }

    /// <summary>
    /// パーティクルをストップ
    /// </summary>
    public void StopParticle()
    {
        _burstEffectObjs[0].SetActive(false);
    }

    #region Shoot
    /// <summary>
    /// 火を生成
    /// </summary>
    /// <param name="targetTransform"></param>
    public void InstanceFlame(Transform targetTransform)
    {
        // 火の弾のインスタンスを生成
        float forwardCorrection = 1.5f;
        float heightCorrection = 1.5f;
        Vector3 instancePos = transform.position + new Vector3( -forwardCorrection, heightCorrection, 0);//HACK: アニメーションの関係でｘ軸に-forwardの補正をかけている
        
        GameObject flameObj = Instantiate(_burstEffectObjs[1], instancePos, Quaternion.identity);

        IDisposable disposable = ShootMagic(targetTransform, flameObj, _flameMoveSpeed);

        // 魔法にこのスクリプトを渡す
        flameObj.GetComponent<Flame>().OnStart(this, disposable);
    }
    /// <summary>
    /// 斬撃を生成
    /// </summary>
    /// <param name="targetTransform"></param>
    public void InstanceSlash(Transform targetTransform)
    {
        // 斬撃のインスタンスを生成
        float forwardCorrection = 1.85f;
        float heightCorrection = 1f;
        // プレイヤーの正面に生成
        Vector3 instancePos = transform.position + new Vector3(0, heightCorrection, forwardCorrection);//HACK: アニメーションの関係でｘ軸に-forwardの補正をかけている
        Quaternion instanceRotation = transform.rotation;

        GameObject slashObj = Instantiate(_slashObj, instancePos, instanceRotation);

        IDisposable disposable = ShootMagic(targetTransform, slashObj, _slashMoveSpeed);

        // 魔法にこのスクリプトを渡す
        slashObj.GetComponent<Slash>().OnStart(this, disposable);

        // 斬撃を2秒後に消す
        Destroy(slashObj, _destroyTime);
    }
    /// <summary>
    /// 魔法を放つ
    /// </summary>
    /// <param name="targetTransform">目標地点</param>
    /// <param name="magicObj">放つ魔法</param>
    /// <returns>IDisposable</returns>
    private IDisposable ShootMagic(Transform targetTransform, GameObject magicObj, float moveSpeed)
    {
        // 火の弾をターゲットまで移動
        IDisposable disposable = Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                // ターゲットの方向を求める
                Vector3 direction = targetTransform.position - magicObj.transform.position;

                // ターゲットに向かう移動量を求める
                Vector3 moveAmount = direction.normalized * moveSpeed * Time.deltaTime;

                // 移動
                magicObj.transform.position += moveAmount;

            }).AddTo(magicObj);

        return disposable;
    }
    #endregion

    /// <summary>
    /// 画面中央からRayを飛ばし当たった座標を返す
    /// </summary>
    /// <returns>画面中央からRayを飛ばして当たった座標</returns>
    public Vector3 ToScreenCenter()
    {
        Vector3 shootPosition;
        // カメラから直線的にRayを飛ばし、当たった位置に飛ばす
        // カメラの位置から画面中央に向かってレイを飛ばす
        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // Rayがオブジェクトに当たったかどうか
        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.layer != LayerMask.NameToLayer("PlayerSide"))
        {
            Debug.Log(hit.collider.gameObject.name);
            // Rayが当たった位置を取得して表示
            shootPosition = hit.point;
        }
        else
        {
            // レイの原点から方向に100m伸ばした座標
            shootPosition = ray.origin + ray.direction * 100f;
        }

        return shootPosition;
    }

    /// <summary>
    /// 火の玉着弾処理  バースト攻撃の着弾エフェクト
    /// </summary>
    /// <param name="bullet"></param>
    public async UniTaskVoid HitFlameEffect(Vector3 arrivedPos)
    {
        // 生成ポジション
        Vector3 instancePos = new Vector3(arrivedPos.x, -1, arrivedPos.z);

        // エフェクトのインスタンス生成
        GameObject burst = Instantiate(_burstEffectObjs[2], instancePos, Quaternion.identity);
        
        // burstDestroyTime秒後burstを破壊
        Destroy(burst, _burstDestroyTime);

        await UniTask.Delay(100);
        
        burst.GetComponent<Collider>().enabled = false;
    }

    /// <summary>
    /// 斬撃着弾処理
    /// </summary>
    public void HitSlashEffect(Vector3 impactPosition)
    {
        // ヒット時エフェクト発生
        GameObject slash = Instantiate(_hitEffectObj, impactPosition, Quaternion.identity);
        
        Destroy(slash, _hitEffectDestroyTime);
    }
}
