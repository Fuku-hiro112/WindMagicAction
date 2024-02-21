using DG.Tweening;
using GameInput;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace SettingCamera
{
    public enum CameraMode
    {
        Default,
        Aim,
        LookTarget
    }

    [ExecuteAlways, RequireComponent(typeof(ConfirmAction))]//Playモードでないとき(==Editモード)にスクリプトを実行できるようになる。
    public class CameraManager : MonoBehaviour //参考URL： https://qiita.com/flankids/items/0a4f70c9bfb6d20f20eb
    {

        [SerializeField]
        private Transform _parent;

        [SerializeField]
        private Transform _child;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private Image _cursor;
        [SerializeField]
        private float _switchingSeconds = 2.0f;
        [SerializeField]
        private float _aimSwitchingSeconds = 0.3f;

        [SerializeField]
        private Parameter _currentParameter;

        [SerializeField]
        private Parameter _defaultParameter;
        [SerializeField]
        private Parameter _aimParameter;

        private Sequence _cameraSequence;
        private ConfirmAction _confirmAction;
        private CameraMode _cameraMode = CameraMode.Default;

        public CameraMode CameraMode => _cameraMode;
        public Parameter CurrentParameter => _currentParameter;

        private void Reset()
        {
            _parent = this.transform;
            _child = this.transform.GetChild(0);
            _camera = Camera.main;
            _currentParameter.TrackTarget = GameObject.FindWithTag("Player").transform;//FIXME: なぜか入らない
            _aimParameter.TrackTarget 　　= GameObject.FindWithTag("Player").transform;//FIXME: なぜか入らない
            _aimParameter.OffsetPosition = new Vector3(0.3f, 1.4f, 6.5f);
        }
        private void Start()
        {
            //ゲームプレイ中じゃなければ以下処理を行わない
            if (!Application.IsPlaying(gameObject)) return;

            _confirmAction = ConfirmAction.s_Instance;
            if (!Application.IsPlaying(gameObject)) _confirmAction = new ConfirmAction();
            Assert.IsNotNull(_confirmAction, "_confirmActionがNullです");
            Assert.IsNotNull(_cursor, "_cursorがNullです");
        }
        void OnGUI()
        {
            //GUILayout.Label($"CameraMode : {CameraMode}");
        }
        private void Update()
        {
            // 角度の差
            Vector3 differenceAngle;
            // マウスの動きの差分をカメラの回り込み角度に反映
            if (Application.IsPlaying(gameObject))//NOTE: ConfirmActionがシングルトンなので、エディタモード中はInputSystemにしないとエラーが出る　TODO:ビルド時は必要ないので削除しよう
            {
                // インプットシステムで値を取得
                //NOTE: XがY、YがXとややこしいので:で明示している
                differenceAngle = new Vector3(
                    x: -_confirmAction.LookDirection.y,
                    y: _confirmAction.LookDirection.x
                    ) * 10f;
            }
            else
            {
                // インプットマネージャーで値を取得
                differenceAngle = new Vector3(
                x: -Input.GetAxis("Mouse Y"),
                y: Input.GetAxis("Mouse X")
                ) * 10f;
            }

            // 角度制限
            if (_cameraMode != CameraMode.LookTarget)// LookTargetの時は回転しないように
            {
                if (_currentParameter.IsLimitAngleX)// X軸の角度制限があるなら
                {
                    if (_currentParameter.Angles.x > _currentParameter.LimitAngleX.Big   && differenceAngle.x > 0 ||
                        _currentParameter.Angles.x < _currentParameter.LimitAngleX.Small && differenceAngle.x < 0)
                    {
                        differenceAngle = Vector3.Scale(differenceAngle, new Vector3(0, 1, 1));// X軸を回転させない  Vecter3.Scale:ベクトルの各成分を乗算する
                    }
                }
                if (_currentParameter.IsLimitAngleY)// Y軸の角度制限があるなら
                {
                    if (_currentParameter.Angles.y > _currentParameter.LimitAngleY.Big   && differenceAngle.y > 0 ||
                        _currentParameter.Angles.y < _currentParameter.LimitAngleY.Small && differenceAngle.y < 0)
                    {
                        differenceAngle = Vector3.Scale(differenceAngle, new Vector3(1, 0, 1));// Y軸を回転させない  Vecter3.Scale:ベクトルの各成分を乗算する
                    }
                }
                
                _currentParameter.Angles += differenceAngle;
            }
        }
        // 被写体などの移動更新が済んだ後にカメラを更新したいので、LateUpdateを使う
        private void FixedUpdate()
        {
            if (_parent == null || _child == null || _camera == null)// これらがnullの場合以降が処理されないようにする
            {
                //IsNullはnullである事を確認する(nullでなければエラーを出す)
                Assert.IsNotNull(_parent, "_parentはnullです！");
                Assert.IsNotNull(_child, "_childはnullです！");
                Assert.IsNotNull(_camera, "_cameraはnullです！");
                return;
            }
            if (_currentParameter.TrackTarget != null)
            {
                // 被写体がTransformで指定されている場合、positionパラメータに座標を上書き
                UpdateTrackTargetBlend(_currentParameter);
            }

            // パラメータを各種オブジェクトに反映
            // 
            _parent.position = _currentParameter.Position;
            _parent.eulerAngles = _currentParameter.Angles;

            // 子のポジションに反映
            var childPos = _child.localPosition;
            childPos.z = -_currentParameter.Distance;
            _child.localPosition = childPos;

            // カメラにパラメーターを反映
            _camera.fieldOfView = _currentParameter.FieldOfView;// 視野角反映
            _camera.transform.localPosition = _currentParameter.OffsetPosition;
            _camera.transform.localEulerAngles = _currentParameter.OffsetAngles;
        }
        /// <summary>
        /// カメラのパラメータを滑らかに変更する
        /// </summary>
        /// <param name="_parameter">カメラのパラメーター</param>
        public static void UpdateTrackTargetBlend(Parameter _parameter)
        {
            _parameter.Position = Vector3.Lerp(
                            _parameter.Position,
                            _parameter.TrackTarget.position,
                            Time.deltaTime * 10f
                        );
        }
        /// <summary>
        /// カメラのモードを変える
        /// </summary>
        /// <param name="mode">何のモードに変えるか</param>
        /// <param name="lookParameter">LookTargetモードに変える場合はそのパラメーターを</param>
        public void SwitchMode(CameraMode mode ,Parameter lookParameter = null)
        {
            float duration = _switchingSeconds; 
            // エイムモード切り替え時は素早くカメラを遷移させる
            if (mode == CameraMode.Aim || _cameraMode == CameraMode.Aim) duration = _aimSwitchingSeconds;

            switch (mode)// 変更後のモードが
            {
                case CameraMode.Default:
                    _defaultParameter.Position = _defaultParameter.TrackTarget.position;
                    switch (_cameraMode)
                    {
                        case CameraMode.LookTarget:
                            _defaultParameter.Angles = new Vector3(15f, transform.eulerAngles.y, 0f);
                            break;
                        default:
                            _defaultParameter.Angles = CurrentParameter.Angles;
                            break;
                    }
                    break;
                case CameraMode.Aim:
                    _aimParameter.Position = _aimParameter.TrackTarget.position;
                    _aimParameter.Angles = CurrentParameter.Angles;
                    transform.eulerAngles = new Vector3(0f, CurrentParameter.Angles.y, 0f);
                    break;
            }

            _cameraMode = mode;// mode反映
            // カーソルの表示非表示
            _cursor.enabled = _cameraMode == CameraMode.Aim;

            CurrentParameter.TrackTarget = null;

            Parameter startParameter = CurrentParameter;
            Parameter endParameter = GetParameter(lookParameter);

            // シーケンス
            _cameraSequence?.Kill();
            _cameraSequence = DOTween.Sequence();
            _cameraSequence.Append(DOTween
                .To(() => 0f
                   , t => Parameter.Lerp(startParameter, endParameter, t, CurrentParameter)
                   , 1f, duration)
                .SetEase(Ease.OutQuart));
            switch (_cameraMode) 
            {
                case CameraMode.Default:
                    _cameraSequence.OnUpdate(()=> UpdateTrackTargetBlend(_defaultParameter));
                    break;
                case CameraMode.Aim:
                    _cameraSequence.OnUpdate(()=> _aimParameter.Position = _aimParameter.TrackTarget.position);
                    break;
            }
            _cameraSequence.AppendCallback(() => CurrentParameter.TrackTarget = endParameter.TrackTarget);
        }
        /// <summary>
        /// 現在のモードからパラメーターを取得
        /// </summary>
        /// <param name="lookParameter">lookTargetのターゲットがあれば、そのパラメーター</param>
        /// <returns>モードによるパラメーター</returns>
        private Parameter GetParameter(Parameter lookParameter)
        {
            switch (_cameraMode)
            {
                case CameraMode.Default:
                    return _defaultParameter;
                case CameraMode.Aim:
                    return _aimParameter;
                case CameraMode.LookTarget:
                    return lookParameter;
                default:
                    Debug.Log("想定しないCameraModeが返ってきました。");
                    return null;
            }
        }

        // カメラのパラメータ
        [Serializable]
        public class Parameter
        {
            public Transform TrackTarget;
            public Vector3 Position;
            public Vector3 Angles = new Vector3(20f, -90f, 0f);
            public float Distance = 7f;　　　　　　　　　　　　// Targetからの距離
            public float FieldOfView = 45f;// 視野角
            public Vector3 OffsetPosition = new Vector3(0f, 1f, 0f);
            public Vector3 OffsetAngles;
            [Header("角度制限")]
            [Tooltip("X軸の制限を付けるか")]
            public bool IsLimitAngleX;
            public LimitAngle LimitAngleX;
            [Tooltip("Y軸の制限を付けるか")]
            public bool IsLimitAngleY;
            public LimitAngle LimitAngleY;

            public static Parameter Lerp(Parameter a, Parameter b, float t, Parameter ret)//TODO: 引数適当過ぎるので変えましょう　Vecter3.Lerpと同じ風にしてる
            {
                ret.Position = Vector3.Lerp(a.Position, b.Position, t);
                ret.Angles = LerpAngles(a.Angles, b.Angles, t);
                ret.Distance = Mathf.Lerp(a.Distance, b.Distance, t);
                ret.FieldOfView = Mathf.Lerp(a.FieldOfView, b.FieldOfView, t);
                ret.OffsetPosition = Vector3.Lerp(a.OffsetPosition, b.OffsetPosition, t);
                ret.OffsetAngles = LerpAngles(a.OffsetAngles, b.OffsetAngles, t);

                // 角度制限類の反映
                ret.IsLimitAngleX = b.IsLimitAngleX;
                ret.LimitAngleX = b.LimitAngleX;
                ret.IsLimitAngleY = b.IsLimitAngleY;
                ret.LimitAngleY = b.LimitAngleY;

                return ret;
            }

            /// <summary>
            /// 角度を滑らかに変える
            /// </summary>
            /// <param name="currentAngle">角度A</param>
            /// <param name="afterAngle">角度B</param>
            /// <param name="t"></param>
            /// <returns></returns>
            private static Vector3 LerpAngles(Vector3 currentAngle, Vector3 afterAngle, float t)
            {
                Vector3 ret = Vector3.zero;
                ret.x = Mathf.LerpAngle(currentAngle.x, afterAngle.x, t);
                ret.y = Mathf.LerpAngle(currentAngle.y, afterAngle.y, t);
                ret.z = Mathf.LerpAngle(currentAngle.z, afterAngle.z, t);
                return ret;
            }
        }
        [Serializable]
        public class LimitAngle
        {
            public float Big;
            public float Small;
        }
    }
}
