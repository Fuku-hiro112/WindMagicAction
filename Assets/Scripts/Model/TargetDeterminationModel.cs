using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class TargetDeterminationModel : MonoBehaviour
{
    // 自分自身
    [SerializeField] private Transform _player;

    // ターゲット
    private List<Transform> _targetPositionList;

    // 視野角（度数法）
    //[SerializeField] private float _sightAngle;

    // 視界の最大距離
    [SerializeField] private float _maxDistance = float.PositiveInfinity;

    // 視界の円錐の頂角
    [SerializeField, Tooltip("視界の円錐の頂角")] private float _viewConeApexAngle = 155f;

    // 敵
    [SerializeField] private TestEnemyManager _enemyManager;
    // 評価点の満点
    private const float c_maxPoint = 100;
    [SerializeField, Tooltip("標準からの距離点数の割合"), Range(0, c_maxPoint)] private int _aimDistanceRatio;

    // ターゲット
    private ReactiveProperty<GameObject> _targetObj = new ReactiveProperty<GameObject>();
    public IReadOnlyReactiveProperty<GameObject> TargetObj => _targetObj;

    // 見えているか
    private bool isVisible = false;

    private Camera _camera;//NOTE: Camera.mainで取るとShake中カメラの切り替えでバグるので

    private void Awake()
    {
        _targetObj.AddTo(this);
    }
    private void Start()
    {
        _camera = Camera.main;
    }
    public void OnUpdate()
    {
        if (_enemyManager.EnemyList.Count <= 1) return;// Listに要素がないなら以下を処理しない

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
            float cos153 = -0.89f;// 約cos153°

            // カメラの視界にいるかどうか
            if (Vector3.Dot(targetToCameraDirection, _camera.transform.forward.normalized) < cos153)//NOTE: .normalizedを付けることにより、内積の計算で|a||b|ベクトルが1になりcosθのみの計算で良くなる
            {//TODO: 本番はターゲットCanvasを表示にする
                //Debug.Log("見えた");
                isVisible = true;

                float point = 0;
                // 敵との距離から点数を出す
                float distanceMaxPoint = c_maxPoint - _aimDistanceRatio;
                var cameraDistance = obj.transform.position - _camera.transform.position;
                // 距離ポイント合計
                float distancePoint = distanceMaxPoint - cameraDistance.magnitude * (distanceMaxPoint / _maxDistance); // 最大点数 - カメラとの距離×(最大点数/最大視野距離) = 近ければ点数高い

                // スクリーン座標
                Vector3 objScreen = new Vector3(
                    _camera.WorldToViewportPoint(obj.transform.position).x
                  , _camera.WorldToViewportPoint(obj.transform.position).y * 2 - 0.5f // 横が短いので補正　NOTE:２倍(縦横比大体２倍だから)するとセンターに照準が合わなくなるので2倍してから-0.5fしている
                  , 1f);
                // スクリーン座標画面中央
                Vector3 senter = new Vector3(0.5f, 0.5f, 1f);
                // スクリーンポイント合計
                float screenPoint = _aimDistanceRatio - (senter - objScreen).magnitude * 100;
                // 合計ポイント
                point = distancePoint + screenPoint;

                if (maxPoint < point)
                {
                    if (!IsObjectsDuringObstacle(obj.transform, _camera.transform))// カメラとオブジェクトの間に障害物があるか
                    {
                        // 代入
                        maxPoint = point;      // 合計ポイント
                        target = obj;// ターゲットオブジェクト

                        Debug.Log(obj.name);
                        //NOTE: 確認用すぐ消そう
                        /*
                        _maxPoint = maxPoint;
                        _maxdis = distancePoint;
                        _maxscr = screenPoint;
                        */
                    }
                }
            }
            else isVisible = false;
        });

        // 見た目確認用　targetは青　それ以外白
        if (_targetObj.Value != target && target != null)
        {
            _targetObj.Value = target;
            /*
            _enemyManager.EnemyList.ForEach(obj =>
            {
                Material material = new Material(Shader.Find("Standard"));
                // マテリアルの色は、targetと同じであれば　青色　違う場合は　白色
                material.color = (obj.transform == target) ? Color.blue : Color.white;
                obj.GetComponent<Renderer>().material = material;
            });*/
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

    #region Debug

    // 視界判定の結果をGUI出力
    private void OnGUI()
    {
        // 視界判定
        //var isVisible = IsVisible();

        // 結果表示
        GUI.Box(new Rect(20, 20, 150, 23), $"isVisible = {isVisible}");
    }

    #endregion
}
