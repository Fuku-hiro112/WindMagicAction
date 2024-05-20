using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SocialPlatforms.Impl;

public class TargetDeterminationModel : MonoBehaviour
{
    // 自分自身
    [SerializeField] private Transform _player;

    /*
    // ターゲット
    //private List<Transform> _targetPositionList;

    // 視野角（度数法）
    //[SerializeField] private float _sightAngle;
    */

    // 視界の最大距離
    [SerializeField] private float _maxDistance = float.PositiveInfinity;

    // 視界の円錐の頂角
    [SerializeField, Tooltip("視界の円錐の頂角")] private float _viewConeApexAngle = 155f;

    // 敵
    [SerializeField] private EnemyManager _enemyManager;
    // 評価点の満点
    private const float c_maxPoint = 100;
    [SerializeField, Tooltip("標準からの距離点数の割合"), Range(0, c_maxPoint)] private int _aimDistanceRatio;

    // ターゲット
    private ReactiveProperty<GameObject> _targetObj = new ReactiveProperty<GameObject>();
    public IReadOnlyReactiveProperty<GameObject> TargetObj => _targetObj;
    private Camera _camera;//NOTE: Camera.mainで取るとShake中カメラの切り替えでバグるので

    private void Awake()
    {
        _targetObj.AddTo(this);
    }
    private void Start()
    {
        //NOTE: 振動時カメラ切り替えが起こるため事前に取得していないとエラーが出る
        _camera = Camera.main;
    }
    public void NullTarget()
    {
        _targetObj.Value = null;
    }
    public void OnUpdate()
    {
        if (!HasExistsEnemy()) return;// Listに要素がないなら以下を処理しない

        //全ての敵のリストからターゲットを決める
        GameObject target = null;
        float maxPoint = 0;

        _enemyManager.EnemyList.Where(obj =>
        {
            // 視野最大距離範囲内にいるかどうか
            var distance = _player.position - obj.transform.position;
            return distance.sqrMagnitude < _maxDistance * _maxDistance;//NOTE: magnitudeだと乗根の計算があり、精度と速度が悪いため乗根を使わずsqrMag・2乗を使って計算している
        }).ToList()
        .ForEach(obj =>
        {
            //ターゲットからカメラの方向へ正規化したベクトルを作成
            Vector3 targetToCameraDirection = (_camera.transform.position - obj.transform.position).normalized;
            float cos153 = -0.89f;// 約cos153

            // カメラの視界にいるかどうか
            if (Vector3.Dot(targetToCameraDirection, _camera.transform.forward.normalized) < cos153)//NOTE: .normalizedを付けることにより、内積の計算で|a||b|ベクトルが1になりcosθのみの計算で良くなる
            {
                // TODO: ポイント計算がおかしい　満点・最小の時の距離を出す必要がありそう
                float totalPoint = 0;

                #region 距離ポイント計算
                // 敵との距離から点数を出す TODO: Rayを使ってEnemyに当たった時にhit.distanceで距離を取って敵との距離を取った方が敵のモデルの大きさに左右されずに住むのでは？
                float distanceMaxPoint = c_maxPoint - _aimDistanceRatio;
                var playerDistance = obj.transform.position - _player.transform.position;
                float proximityScore = _maxDistance - playerDistance.magnitude;// 近いほど点数が高い　最高はMaxDistance値
                // 接近スコアが0未満の時エラーを出す。　マイナス値の場合得点計算がおかしくなるため
                Debug.Assert(proximityScore >= 0, "接近スコアが0未満になっています！(proximityScore : "+ proximityScore +")");

                // 距離ポイント合計
                float distancePoint = proximityScore * (distanceMaxPoint / _maxDistance); // 近さスコア×(最大点数/最大視野距離) = 近ければ点数高い
                #endregion


                #region スクリーンポイント計算
                // オブジェクトの位置をスクリーン座標へ
                Vector3 objToScreenPoint = _camera.WorldToScreenPoint(obj.transform.position); 
                // スクリーン座標座標 画面中央
                Vector3 senterToScreenPoint = new Vector3(Screen.width/2, Screen.height/2, 1);
                float maxDistance = Screen.width / 2;
                float proximityScoreFromSenter = maxDistance - (senterToScreenPoint - objToScreenPoint).magnitude;// 接近スコア
                
                // 接近スコアが0未満の時エラーを出す。　マイナス値の場合得点計算がおかしくなるため
                Debug.Assert(proximityScoreFromSenter >= 0, "接近スコアが0未満になっています！(proximityScoreFromSenter : " + proximityScore + ")");

                // スクリーンポイント合計
                float screenPoint = proximityScoreFromSenter * (_aimDistanceRatio / maxDistance);
                #endregion

                // 合計ポイント
                totalPoint = distancePoint + screenPoint;
                Debug.Assert(totalPoint <= 100, "トータルスコアが想定外の数値になっています。(totalPoint("+totalPoint+") = distancePoint("+distancePoint+") + screenPoint("+screenPoint+ "))");
                Debug.Log($"{totalPoint} = 距離{distancePoint} + スクリーン{screenPoint}");
                
                if (maxPoint < totalPoint)
                {
                    if (!IsObjectsDuringObstacle(obj.transform, _camera.transform))// カメラとオブジェクトの間に障害物があるか
                    {
                        // 代入
                        maxPoint = totalPoint; // 合計ポイント
                        target = obj;// ターゲットオブジェクト
                        /*
                        //Debug.Log(obj.name);
                        //NOTE: 確認用すぐ消そう
                        _maxPoint = maxPoint;
                        _maxdis = distancePoint;
                        _maxscr = screenPoint;
                        */
                    }
                }
            }
        });

        // 見た目確認用　targetは青　それ以外白
        if (_targetObj.Value != target && target != null)
        {
            _targetObj.Value = target;
        }// target反映
        else if (_targetObj.Value != null && target == null)
        {
            _targetObj.Value = target;
        }
    }
    /// <summary>
    /// オブジェクト間に障害物があるかどうか
    /// </summary>
    /// <param name="targetTransform">何のObjに</param>
    /// <param name="startTransform">どのオブジェクトから</param>
    /// <returns>障害物があればture</returns>
    private bool IsObjectsDuringObstacle(Transform targetTransform, Transform startTransform)
    {
        bool result = true;
        // Rayを飛ばす方角
        Vector3 heightCorrection = Vector3.up * 0.5f;// Rayの高さ補正値
        Vector3 targetPoint = targetTransform.position + heightCorrection; //NOTE: Rayが地面に衝突するため高さを補正
        Vector3 objDirection = targetPoint - startTransform.position;
        RaycastHit hit;

        // PlayerSide以外に当たるLayerMask
        int layerMask = 1 << LayerMask.NameToLayer("PlayerSide");
        layerMask = ~layerMask;

        Debug.DrawLine(startTransform.position, targetPoint, Color.red, 0.1f);
        if (Physics.Raycast(startTransform.position, objDirection, out hit, _maxDistance, layerMask))// カメラからオブジェクトにRayを飛ばす
        {//TODO: 障害物レイヤーのみに当たるようにしよう

            // オブジェクト以外に当たっていれば
            if (hit.collider.gameObject.name != targetTransform.gameObject.name)
            {
                result = true;
                Debug.Log($"{hit.collider.gameObject.name}に当たっている");
            }
            else
            {
                Debug.Log("ちゃんとターゲットに当たってる");
                result = false;
            }
        }

        // 結果を返す
        return result;
    }

    /// <summary>
    /// 敵が存在しているか
    /// </summary>
    /// <returns></returns>
    public bool HasExistsEnemy() => _enemyManager.EnemyList.Count > 0;
    /// <summary>
    /// プレイヤーから近くの敵を返す
    /// </summary>
    /// <returns>近くの敵のTransformを返す</returns>
    public Transform NearEnemy() 
    {
        Transform minEnemy = null;
        
        // 最小距離を記録
        float minDistance = 999999;

        foreach (var enemy in _enemyManager.EnemyList)
        {
            // プレイヤーと敵の距離
            float distance = Vector3.Distance(enemy.transform.position, _player.transform.position);
            
            // 距離が短いなら
            if (minDistance > distance)
            {
                // 最小距離,Transformの上書き
                minDistance = distance;
                minEnemy = enemy.transform;
            }
        }

        Assert.IsNotNull(minEnemy, $"{this}のminEnemyがNUllです。");
        return minEnemy;
    }
}
