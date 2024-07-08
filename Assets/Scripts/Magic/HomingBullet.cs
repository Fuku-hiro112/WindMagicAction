using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    [SerializeField, Tooltip("出現位置前方補正")] private float _popPositionFrontCorrection = 2.0f;
    [SerializeField, Tooltip("出現位置高さ補正")] private float _popPositionHeightCorrection;
    [SerializeField] private TargetDeterminationModel _targetDeterminationModel;
    public GameObject _bulletPrefab;
    private Vector3 _bulletPopPos;

    /// <summary>
    /// 弾の生成
    /// </summary>
    public void GenerateBullet()
    {
        Vector3 horizontalCorrection = transform.forward * _popPositionFrontCorrection;
        Vector3 verticalCorrection = Vector3.up * _popPositionHeightCorrection;        // 縦(高さ)補正
        _bulletPopPos = transform.position + verticalCorrection + horizontalCorrection;// 生成ポジション

        // 弾生成
        GameObject bullet = Instantiate(
            _bulletPrefab
            , _bulletPopPos
            ,Quaternion.FromToRotation(Vector3.forward, transform.forward));
        
        // 誘導弾の初期設定　targetを渡す
        //NOTE: nullの場合エラーが出るため、その場合はdefault値を渡すようにしている
        if(_targetDeterminationModel.TargetObj.Value == null) 
            bullet.GetComponent<HomingShoot>().Initialize(default);
        else 
            bullet.GetComponent<HomingShoot>().Initialize(_targetDeterminationModel.TargetObj.Value.transform);
    }

}