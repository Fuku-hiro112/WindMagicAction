using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SettingCamera
{
    [ExecuteInEditMode]//Playモードでないとき(==Editモード)にスクリプトを実行できるようになる。
    public class TestCameraManager : MonoBehaviour //参考URL： https://qiita.com/flankids/items/0a4f70c9bfb6d20f20eb
    {

        [SerializeField]
        private Transform _parent;

        [SerializeField]
        private Transform _child;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private Parameter _parameter;

        public Parameter Param => _parameter;

        private void Update()
        {
            // マウスの動きの差分をカメラの回り込み角度に反映
            Vector3 diffAngles = new Vector3(
                x: -Input.GetAxis("Mouse Y"),
                y: Input.GetAxis("Mouse X")
            ) * 10f;
            _parameter.angles += diffAngles;
        }
        // 被写体などの移動更新が済んだ後にカメラを更新したいので、LateUpdateを使う
        private void LateUpdate()
        {
            if (_parent == null || _child == null || _camera == null)// これらがnullの場合以降が処理されないようにする
            {
                //IsNullはnullである事を確認する(nullでなければエラーを出す)
                Assert.IsNotNull(_parent, "_parentはnullです！");
                Assert.IsNotNull(_child, "_childはnullです！");
                Assert.IsNotNull(_camera, "_cameraはnullです！");
                return;
            }
            if (_parameter.trackTarget != null)
            {
                // 被写体がTransformで指定されている場合、positionパラメータに座標を上書き
                UpdateTrackTargetBlend(_parameter);
            }

            // パラメータを各種オブジェクトに反映
            _parent.position = _parameter.position;
            _parent.eulerAngles = _parameter.angles;

            var childPos = _child.localPosition;
            childPos.z = -_parameter.distance;
            _child.localPosition = childPos;

            _camera.fieldOfView = _parameter.fieldOfView;
            _camera.transform.localPosition = _parameter.offsetPosition;
            _camera.transform.localEulerAngles = _parameter.offsetAngles;
        }
        // 
        public static void UpdateTrackTargetBlend(Parameter _parameter)
        {
            _parameter.position = Vector3.Lerp(
                            _parameter.position,
                            _parameter.trackTarget.position,
                            Time.deltaTime * 4f
                        );
        }

        // カメラのパラメータ
        [Serializable]
        public class Parameter
        {
            public Transform trackTarget;
            public Vector3 position;
            public Vector3 angles = new Vector3(20f, 0f, 0f);
            public float distance = 7f;
            public float fieldOfView = 45f;
            public Vector3 offsetPosition = new Vector3(0f, 1f, 0f);
            public Vector3 offsetAngles;


            public static Parameter Lerp(Parameter a, Parameter b, float t, Parameter ret)
            {
                ret.position = Vector3.Lerp(a.position, b.position, t);
                ret.angles = LerpAngles(a.angles, b.angles, t);
                ret.distance = Mathf.Lerp(a.distance, b.distance, t);
                ret.fieldOfView = Mathf.Lerp(a.fieldOfView, b.fieldOfView, t);
                ret.offsetPosition = Vector3.Lerp(a.offsetPosition, b.offsetPosition, t);
                ret.offsetAngles = LerpAngles(a.offsetAngles, b.offsetAngles, t);

                return ret;
            }

            private static Vector3 LerpAngles(Vector3 a, Vector3 b, float t)
            {
                Vector3 ret = Vector3.zero;
                ret.x = Mathf.LerpAngle(a.x, b.x, t);
                ret.y = Mathf.LerpAngle(a.y, b.y, t);
                ret.z = Mathf.LerpAngle(a.z, b.z, t);
                return ret;
            }
        }
    }
}
