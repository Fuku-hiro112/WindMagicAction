using System.Linq;
using UniRx;
using Unit;
using UnityEngine;
using UnityEngine.Assertions;

public class TargetDeterminationModel : MonoBehaviour
{
    // 自分自身
    [SerializeField] private Transform _player;

    // 視界の最大距離
    [SerializeField] private float _maxDistance = float.PositiveInfinity;//WHY: 何故無限にしてる？

    // 視界の円錐の頂角
    [SerializeField, Tooltip("視界の円錐の頂角")] private float _viewConeApexAngle = 155f;

    // 敵
    [SerializeField] private EnemyManager _enemyManager;
    // 評価点の満点
    private const float c_maxPoint = 100;
    [SerializeField, Tooltip("照準からの距離点数の割合"), Range(0, c_maxPoint)] private int _aimDistanceRatio = 40;

    // 視野
    private const float c_cos153 = -0.89f;// 約cos153

    // ターゲット
    private ReactiveProperty<GameObject> _targetObj = new ReactiveProperty<GameObject>();
    private Camera _camera;//NOTE: Camera.mainで取るとShake中カメラの切り替えでバグるので
    public IReadOnlyReactiveProperty<GameObject> TargetObj => _targetObj;

    private void Awake()
    {
        _targetObj.AddTo(this);
    }
    private void Start()
    {
        //NOTE: 振動時カメラ切り替えが起こるため事前に取得していないとエラーが出る
        _camera = Camera.main;
    }
    /// <summary>
    /// ターゲットをNullにする
    /// </summary>
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

            // カメラの視界にいるかどうか
            if (Vector3.Dot(targetToCameraDirection, _camera.transform.forward.normalized) < c_cos153)//NOTE: .normalizedを付けることにより、内積の計算で|a||b|ベクトルが1になりcosθのみの計算で良くなる
            {
                float totalPoint = 0;

                #region 距離ポイント計算

                // 敵との距離から点数を出す
                float distanceMaxPoint = c_maxPoint - _aimDistanceRatio;
                Vector3 playerDistance = obj.transform.position - _player.transform.position;
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
                float screenPoint = proximityScoreFromSenter * (_aimDistanceRatio / maxDistance);// 近さスコア×（最大点数 / 最大距離）= 画面中央から近ければ点数が高い
                
                #endregion

                // 合計ポイント
                totalPoint = distancePoint + screenPoint;
                Debug.Assert(totalPoint <= 100, "トータルスコアが想定外の数値になっています。(totalPoint("+totalPoint+") = distancePoint("+distancePoint+") + screenPoint("+screenPoint+ "))");

                if (maxPoint < totalPoint)
                {
                    Transform targetTrans;
                    Transform root = obj.transform.root;
                    UnitStats unitStats;
                    // UnitStatsがあるなら
                    if (root.TryGetComponent(out unitStats))
                    {
                        // ターゲットの位置を渡す
                        targetTrans = unitStats.TargetDisplayPosition;
                    }
                    else
                    {
                        // 一番上の階層のオブジェクトを渡す
                        targetTrans = root;
                    }

                    if (!IsObjectsDuringObstacle(root, _camera.transform))// カメラとオブジェクトの間に障害物があるか
                    {
                        // 代入
                        maxPoint = totalPoint; // 合計ポイント
                        target = root.gameObject;// ターゲットオブジェクト
                    }
                }
            }
        });

        // ターゲットが変わった時
        // ターゲットがいなくなった時
        if (_targetObj.Value != target && target != null)
        {
            _targetObj.Value = target;
        }// ターゲットが現れた時
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
        {
            // オブジェクト以外に当たっていれば
            if (hit.collider.transform.root.gameObject != targetTransform.gameObject)
                result = true;
            else
                result = false;
        }

        // 結果を返す
        return result;
    }

    /// <summary>
    /// 敵が存在しているか
    /// </summary>
    /// <returns>敵が居ればTrue</returns>
    public bool HasExistsEnemy() => _enemyManager.EnemyList.Count > 0;
    /// <summary>
    /// プレイヤーから一番近くの敵を返す
    /// </summary>
    /// <returns>一番近くの敵のTransformを返す</returns>
    public Transform NearEnemy() 
    {
        Transform minEnemy = null;
        
        // 最小距離を記録
        float minDistance = 99999;

        // 全ての敵を見てプレイヤーとの距離が最小の敵を格納する
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
