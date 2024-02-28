using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using DG.Tweening;
using UniRx;

public class ShakeCamera : MonoBehaviour
{
    private GameObject _shakeCameraObj;
    private Camera _shakeCamera;
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
        _shakeCameraObj = this.gameObject;
        TryGetComponent(out _shakeCamera);
        Assert.IsNotNull(_shakeCamera, $"{this}の_shakeCameraが見つかりませんでした！");
        _shakeCamera.enabled = false;// カメラオフ
        
        // メインのカメラで視野角の変更があった時Shakeカメラにも反映する　ないと思うけど一応ね
        var fieldOfView = new ReactiveProperty<float>(Camera.main.fieldOfView);
        fieldOfView.DistinctUntilChanged()
            .Subscribe(value =>
            {
                _shakeCamera.fieldOfView = value;
            }).AddTo(this);
    }
    /// <summary>
    /// カメラを揺らす
    /// </summary>
    /// <param name="positionStrength"></param>
    /// <param name="rotationStrength"></param>
    /// <param name="shakeDuration"></param>
    public void Shake(Vector3 positionStrength, Vector3 rotationStrength, float shakeDuration)
    {
        Transform camera = _shakeCameraObj.transform;
        
        // 前のTweenがまだ実行中の場合に、それを即完了状態に以降する。
        camera.DOComplete();//NOTE: Killだと中断してしまうためカメラが元の位置に戻らない
        camera.DOShakeRotation(shakeDuration, rotationStrength);// 揺らす時間,角度揺れの強度
        camera.DOShakePosition(shakeDuration, positionStrength) // 揺らす時間,ポジションの揺れの強度
            .OnStart   (() => SwicthShakeCamera(true))  // 開始時 Shakeカメラ切り替え
            .OnComplete(() => SwicthShakeCamera(false));// 終了時 Mainカメラ切り替え
    }

    /// <summary>
    /// シェイクカメラに切り替え
    /// </summary>
    /// <param name="shake">シェイクカメラに切り替えるかどうか</param>
    private void SwicthShakeCamera(bool shake)
    {
        _mainCamera.enabled  = !shake;
        _shakeCamera.enabled = shake;
    }
}
