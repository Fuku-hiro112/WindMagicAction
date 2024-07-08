using DG.Tweening;
using GameInput;
using System;
using UniRx;
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

        [SerializeField, Tooltip("照準画像")]
        private Image _cursor;
        [SerializeField, Range(0,10), Tooltip("カメラ感度")]
        private float _cameraSensitivity = 5;
        [SerializeField, Tooltip("通常状態への切り替え秒数")]
        private float _switchingSeconds = 0.5f;
        [SerializeField, Tooltip("Aim状態への切り替え秒数")]
        private float _aimSwitchingSeconds = 0.3f;

        [SerializeField, Tooltip("現在のカメラ値")]
        private Parameter _currentParameter;

        [SerializeField, Tooltip("通常時のカメラ値")]
        private Parameter _defaultParameter;
        [SerializeField, Tooltip("Aim時のカメラ値")]
        private Parameter _aimParameter;

        private Sequence _cameraSequence;
        private ConfirmAction _confirmAction;
        private ReactiveProperty<CameraMode> _cameraModeType = new ReactiveProperty<CameraMode>();

        public IReactiveProperty<CameraMode> CameraModeType => _cameraModeType;
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
        private void Awake()
        {
            _cameraModeType.AddTo(this);
        }
        private void Start()
        {
            //ゲームプレイ中じゃなければ以下処理を行わない
            if (!Application.IsPlaying(gameObject)) return;

            _confirmAction = ConfirmAction.s_Instance;
            if (!Application.IsPlaying(gameObject)) _confirmAction = new ConfirmAction();

            Assert.IsNotNull(_confirmAction, $"{this}の_confirmActionがNullです");
            Assert.IsNotNull(_cursor,        $"{this}の_cursorがNullです");
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
                    ) * _cameraSensitivity;
            }
            else
            {
                // インプットマネージャーで値を取得
                differenceAngle = new Vector3(
                x: -Input.GetAxis("Mouse Y"),
                y: Input.GetAxis("Mouse X")
                ) * _cameraSensitivity;
            }

            // 角度制限
            if (_cameraModeType.Value != CameraMode.LookTarget)// LookTargetの時は回転しないように
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
        private void FixedUpdate()
        {
            if (_parent == null || _child == null || _camera == null)// これらがnullの場合以降が処理されないようにする
            {
                //IsNullはnullである事を確認する(nullでなければエラーを出す)
                Assert.IsNotNull(_parent, $"{this}の_parentはnullです！");
                Assert.IsNotNull(_child, $"{this}の_childはnullです！");
                Assert.IsNotNull(_camera, $"{this}の_cameraはnullです！");
                return;
            }
            if (_currentParameter.TrackTarget != null)
            {
                // 被写体がTransformで指定されている場合、positionパラメータに座標を上書き
                UpdateTrackTargetBlend(_currentParameter);
            }

            // パラメータを各種オブジェクトに反映
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
        /// <param name="modeChange">何のモードに変えるか</param>
        /// <param name="lookParameter">LookTargetモードに変える場合はそのパラメーターを</param>
        public void SwitchMode(CameraMode modeChange ,Parameter lookParameter = null)
        {
            float duration = _switchingSeconds; 
            // エイムモードが絡む切り替え時は素早くカメラを遷移させる
            if (modeChange == CameraMode.Aim || _cameraModeType.Value == CameraMode.Aim) duration = _aimSwitchingSeconds;

            // モードによってパラメーターを変更　ポジション、角度
            switch (modeChange)
            {
                // デフォルトモードに切り替え時
                case CameraMode.Default:
                    _defaultParameter.Position = _defaultParameter.TrackTarget.position;
                    
                    // 切り替え前のモードによって角度を変える
                    switch (_cameraModeType.Value)
                    {
                        case CameraMode.LookTarget:
                            _defaultParameter.Angles = new Vector3(15f, transform.eulerAngles.y, 0f);
                            break;
                        default:
                            _defaultParameter.Angles = _currentParameter.Angles;
                            break;
                    }
                    break;
                // エイムモードに切り替え時
                case CameraMode.Aim:
                    _aimParameter.Position = _aimParameter.TrackTarget.position;
                    _aimParameter.Angles = _currentParameter.Angles;
                    transform.eulerAngles = new Vector3(0f, _currentParameter.Angles.y, 0f);
                    break;
            }

            _cameraModeType.Value = modeChange;// mode反映
            // カーソルの表示非表示
            _cursor.enabled = _cameraModeType.Value == CameraMode.Aim;

            _currentParameter.TrackTarget = null;

            Parameter startParameter = _currentParameter.Clone();
            Parameter endParameter = GetParameter(lookParameter);//HACK: ここは参照渡し

            // シーケンス
            _cameraSequence?.Kill();
            _cameraSequence = DOTween.Sequence();
            // StartParameterからEndParameterに変更 //HACK: 角度も変更しているのでdurationの間は視点移動出来ない
            _cameraSequence.Append(DOTween
                .To(() => 0f
                   , t => Parameter.Lerp(startParameter, endParameter, t, _currentParameter)
                   , 1f
                   , duration)
                .SetEase(Ease.OutQuart));

            switch (_cameraModeType.Value) 
            {
                case CameraMode.Default:
                    _cameraSequence.OnUpdate(()=> UpdateTrackTargetBlend(_defaultParameter));
                    break;
                case CameraMode.Aim:
                    _cameraSequence.SetUpdate(UpdateType.Fixed, true).OnUpdate(()=> _aimParameter.Position = _aimParameter.TrackTarget.position);
                    break;
            }
            _cameraSequence.AppendCallback(() => _currentParameter.TrackTarget = endParameter.TrackTarget);
        }
        /// <summary>
        /// 現在のモードからパラメーターを取得
        /// </summary>
        /// <param name="lookParameter">lookTargetのターゲットがあれば、そのパラメーター</param>
        /// <returns>モードによるパラメーター</returns>
        private Parameter GetParameter(Parameter lookParameter)
        {
            switch (_cameraModeType.Value)
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
            public float Distance = 7f;　　// Targetからの距離
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

            // 参照しない場合のコピーを作成
            public Parameter Clone()
            {
                return (Parameter)MemberwiseClone();//NOTE: MemberwiseCloneはobject型を返すのでキャストが必要
            }

            public static Parameter Lerp(Parameter before, Parameter after, float t, Parameter ret)//TODO: 引数適当過ぎるので変えましょう　Vecter3.Lerpと同じ風にしてる
            {
                ret.Position = Vector3.Lerp(before.Position, after.Position, t);
                ret.Angles = LerpAngles(before.Angles, after.Angles, t);
                ret.Distance = Mathf.Lerp(before.Distance, after.Distance, t);
                ret.FieldOfView = Mathf.Lerp(before.FieldOfView, after.FieldOfView, t);
                ret.OffsetPosition = Vector3.Lerp(before.OffsetPosition, after.OffsetPosition, t);
                ret.OffsetAngles = LerpAngles(before.OffsetAngles, after.OffsetAngles, t);

                // 角度制限類の反映
                ret.IsLimitAngleX = after.IsLimitAngleX;
                ret.LimitAngleX = after.LimitAngleX;
                ret.IsLimitAngleY = after.IsLimitAngleY;
                ret.LimitAngleY = after.LimitAngleY;

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
