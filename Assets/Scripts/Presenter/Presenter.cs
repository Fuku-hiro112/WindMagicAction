using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using SettingCamera;

public class Presenter : MonoBehaviour
{
    [SerializeField] TargetDeterminationModel _targetDetermination;
    [SerializeField] TargetTrackingView _targetTracking;
    [SerializeField] CameraManager _cameraManager;
    /*
    [SerializeField] UnitStatsModel _statsModel;
    [SerializeField] UnitStatsView _statsView;
    */
    void Start()
    {
        var targetObj = _targetDetermination.TargetObj;
#region Model￫View[Target]
        // ターゲット画像の表示の切り替え
        _targetDetermination.TargetObj// Target切り替え時
            .Select(obj => _cameraManager.CameraModeType.Value == CameraMode.Aim 
                           && obj != null)
            .Subscribe(obj => _targetTracking.ToggleCursorVisibility(obj));
        _cameraManager.CameraModeType // カメラモード切り替え時
            .Select(_ => _cameraManager.CameraModeType.Value == CameraMode.Aim
                           && targetObj.Value != null)
            .Subscribe(isDisplay => _targetTracking.ToggleCursorVisibility(isDisplay));
        // ターゲット画像の位置調整
        this.UpdateAsObservable()
            .Select(_=> targetObj.Value == null ? default : targetObj.Value.transform.position)
            //.DistinctUntilChanged()// 同じ値を連続して流さない　NOTE:少しでも軽量化したかった
            .Subscribe(pos => _targetTracking.AdjustCursorPosition((Vector3)pos));
#endregion

#region Model￫View[Helth]
        // Healthのテキスト・Barの更新
        /*//TODO: わざわざ全ての敵にMVP全部付けるなら要らないのでは？ 
        _statsModel.Health
            .Subscribe(health => 
            {
                _statsView.UpdateHealthBar(health, _statsModel.MaxHealth);
                _statsView.UpdateHealthText(health, _statsModel.MaxHealth);
            });
        */

#endregion
    }
    void Update()
    {
        
    }
}
