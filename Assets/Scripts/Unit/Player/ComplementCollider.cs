using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ComplementCollider : MonoBehaviour // 参考URL: https://qiita.com/LuminousNeco/items/311b450cf5a45eab3f9e
{
    /// <summary>
    /// 切っ先部分の位置追跡用
    /// </summary>
    public Transform TipPosition;

    /// <summary>
    /// メッシュ生成用
    /// </summary>
    Mesh mesh;

    /// <summary>
    /// メッシュコライダ
    /// </summary>
    MeshCollider meshCollider;

    /// <summary>
    /// 前フレーム切っ先位置
    /// </summary>
    Vector3 lastTipPosition;

    /// <summary>
    /// 前フレーム
    /// </summary>
    Vector3 lastPosition;

    public bool isAttack;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();

        // トリガーとして設定する
        meshCollider.convex = true;
        meshCollider.isTrigger = true;
        
        // メッシュを指定
        meshCollider.sharedMesh = mesh;
        
        // 前回位置の初期化
        lastTipPosition = TipPosition.position;
        lastPosition = transform.position;

        meshCollider.enabled = false;
        isAttack = false;
    }

    void FixedUpdate()
    {
        if (!isAttack) 
        { 
            meshCollider.enabled = false;
            return;
        }
        else
        {
            meshCollider.enabled = true;
        }
        // 前回位置と同じ時は処理しない（エラーになる）
        if (lastPosition == transform.position)
            return;
        if (lastTipPosition == TipPosition.position)
            return;

        // 毎フレームメッシュを作り直す
        mesh.Clear();

        // 前回位置と今回位置の中間位置を補完する（こうしないと中間部分の判定が短くなる）
        var colliderLength = (TipPosition.position - transform.position).magnitude;
        var startMidPoint = Vector3.Lerp(transform.position, lastPosition, 0.5f);
        var endMidPoint = Vector3.Lerp(TipPosition.position, lastTipPosition, 0.5f);
        var assumedMidEndPoint = startMidPoint + (endMidPoint - startMidPoint).normalized * colliderLength;

        // 頂点（ローカル座標に変換して設定）
        mesh.vertices = new Vector3[] {
            transform.InverseTransformPoint(lastPosition), transform.InverseTransformPoint(lastTipPosition),
            transform.InverseTransformPoint(startMidPoint), transform.InverseTransformPoint(assumedMidEndPoint),
            transform.InverseTransformPoint(transform.position), transform.InverseTransformPoint(TipPosition.position),
        };

        // さっきの頂点を順に指定して三角形をつくる
        mesh.triangles = new int[]{
            0, 1, 2,
            1, 2, 3,
            2, 3, 4,
            3, 4, 5,
        };

        // 反映
        meshCollider.enabled = false;
        meshCollider.enabled = true;

        // 前回位置の記憶
        lastTipPosition = TipPosition.position;
        lastPosition = transform.position;
    }
}
