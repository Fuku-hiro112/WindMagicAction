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

    [ExecuteAlways, RequireComponent(typeof(ConfirmAction))]//Play���[�h�łȂ��Ƃ�(==Edit���[�h)�ɃX�N���v�g�����s�ł���悤�ɂȂ�B
    public class CameraManager : MonoBehaviour //�Q�lURL�F https://qiita.com/flankids/items/0a4f70c9bfb6d20f20eb
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
            _currentParameter.TrackTarget = GameObject.FindWithTag("Player").transform;//FIXME: �Ȃ�������Ȃ�
            _aimParameter.TrackTarget �@�@= GameObject.FindWithTag("Player").transform;//FIXME: �Ȃ�������Ȃ�
            _aimParameter.OffsetPosition = new Vector3(0.3f, 1.4f, 6.5f);
        }
        private void Start()
        {
            //�Q�[���v���C������Ȃ���Έȉ��������s��Ȃ�
            if (!Application.IsPlaying(gameObject)) return;

            _confirmAction = ConfirmAction.s_Instance;
            if (!Application.IsPlaying(gameObject)) _confirmAction = new ConfirmAction();
            Assert.IsNotNull(_confirmAction, "_confirmAction��Null�ł�");
            Assert.IsNotNull(_cursor, "_cursor��Null�ł�");
        }
        void OnGUI()
        {
            //GUILayout.Label($"CameraMode : {CameraMode}");
        }
        private void Update()
        {
            // �p�x�̍�
            Vector3 differenceAngle;
            // �}�E�X�̓����̍������J�����̉�荞�݊p�x�ɔ��f
            if (Application.IsPlaying(gameObject))//NOTE: ConfirmAction���V���O���g���Ȃ̂ŁA�G�f�B�^���[�h����InputSystem�ɂ��Ȃ��ƃG���[���o��@TODO:�r���h���͕K�v�Ȃ��̂ō폜���悤
            {
                // �C���v�b�g�V�X�e���Œl���擾
                //NOTE: X��Y�AY��X�Ƃ�₱�����̂�:�Ŗ������Ă���
                differenceAngle = new Vector3(
                    x: -_confirmAction.LookDirection.y,
                    y: _confirmAction.LookDirection.x
                    ) * 10f;
            }
            else
            {
                // �C���v�b�g�}�l�[�W���[�Œl���擾
                differenceAngle = new Vector3(
                x: -Input.GetAxis("Mouse Y"),
                y: Input.GetAxis("Mouse X")
                ) * 10f;
            }

            // �p�x����
            if (_cameraMode != CameraMode.LookTarget)// LookTarget�̎��͉�]���Ȃ��悤��
            {
                if (_currentParameter.IsLimitAngleX)// X���̊p�x����������Ȃ�
                {
                    if (_currentParameter.Angles.x > _currentParameter.LimitAngleX.Big   && differenceAngle.x > 0 ||
                        _currentParameter.Angles.x < _currentParameter.LimitAngleX.Small && differenceAngle.x < 0)
                    {
                        differenceAngle = Vector3.Scale(differenceAngle, new Vector3(0, 1, 1));// X������]�����Ȃ�  Vecter3.Scale:�x�N�g���̊e��������Z����
                    }
                }
                if (_currentParameter.IsLimitAngleY)// Y���̊p�x����������Ȃ�
                {
                    if (_currentParameter.Angles.y > _currentParameter.LimitAngleY.Big   && differenceAngle.y > 0 ||
                        _currentParameter.Angles.y < _currentParameter.LimitAngleY.Small && differenceAngle.y < 0)
                    {
                        differenceAngle = Vector3.Scale(differenceAngle, new Vector3(1, 0, 1));// Y������]�����Ȃ�  Vecter3.Scale:�x�N�g���̊e��������Z����
                    }
                }
                
                _currentParameter.Angles += differenceAngle;
            }
        }
        // ��ʑ̂Ȃǂ̈ړ��X�V���ς񂾌�ɃJ�������X�V�������̂ŁALateUpdate���g��
        private void FixedUpdate()
        {
            if (_parent == null || _child == null || _camera == null)// ����炪null�̏ꍇ�ȍ~����������Ȃ��悤�ɂ���
            {
                //IsNull��null�ł��鎖���m�F����(null�łȂ���΃G���[���o��)
                Assert.IsNotNull(_parent, "_parent��null�ł��I");
                Assert.IsNotNull(_child, "_child��null�ł��I");
                Assert.IsNotNull(_camera, "_camera��null�ł��I");
                return;
            }
            if (_currentParameter.TrackTarget != null)
            {
                // ��ʑ̂�Transform�Ŏw�肳��Ă���ꍇ�Aposition�p�����[�^�ɍ��W���㏑��
                UpdateTrackTargetBlend(_currentParameter);
            }

            // �p�����[�^���e��I�u�W�F�N�g�ɔ��f
            // 
            _parent.position = _currentParameter.Position;
            _parent.eulerAngles = _currentParameter.Angles;

            // �q�̃|�W�V�����ɔ��f
            var childPos = _child.localPosition;
            childPos.z = -_currentParameter.Distance;
            _child.localPosition = childPos;

            // �J�����Ƀp�����[�^�[�𔽉f
            _camera.fieldOfView = _currentParameter.FieldOfView;// ����p���f
            _camera.transform.localPosition = _currentParameter.OffsetPosition;
            _camera.transform.localEulerAngles = _currentParameter.OffsetAngles;
        }
        /// <summary>
        /// �J�����̃p�����[�^�����炩�ɕύX����
        /// </summary>
        /// <param name="_parameter">�J�����̃p�����[�^�[</param>
        public static void UpdateTrackTargetBlend(Parameter _parameter)
        {
            _parameter.Position = Vector3.Lerp(
                            _parameter.Position,
                            _parameter.TrackTarget.position,
                            Time.deltaTime * 10f
                        );
        }
        /// <summary>
        /// �J�����̃��[�h��ς���
        /// </summary>
        /// <param name="mode">���̃��[�h�ɕς��邩</param>
        /// <param name="lookParameter">LookTarget���[�h�ɕς���ꍇ�͂��̃p�����[�^�[��</param>
        public void SwitchMode(CameraMode mode ,Parameter lookParameter = null)
        {
            float duration = _switchingSeconds; 
            // �G�C�����[�h�؂�ւ����͑f�����J������J�ڂ�����
            if (mode == CameraMode.Aim || _cameraMode == CameraMode.Aim) duration = _aimSwitchingSeconds;

            switch (mode)// �ύX��̃��[�h��
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

            _cameraMode = mode;// mode���f
            // �J�[�\���̕\����\��
            _cursor.enabled = _cameraMode == CameraMode.Aim;

            CurrentParameter.TrackTarget = null;

            Parameter startParameter = CurrentParameter;
            Parameter endParameter = GetParameter(lookParameter);

            // �V�[�P���X
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
        /// ���݂̃��[�h����p�����[�^�[���擾
        /// </summary>
        /// <param name="lookParameter">lookTarget�̃^�[�Q�b�g������΁A���̃p�����[�^�[</param>
        /// <returns>���[�h�ɂ��p�����[�^�[</returns>
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
                    Debug.Log("�z�肵�Ȃ�CameraMode���Ԃ��Ă��܂����B");
                    return null;
            }
        }

        // �J�����̃p�����[�^
        [Serializable]
        public class Parameter
        {
            public Transform TrackTarget;
            public Vector3 Position;
            public Vector3 Angles = new Vector3(20f, -90f, 0f);
            public float Distance = 7f;�@�@�@�@�@�@�@�@�@�@�@�@// Target����̋���
            public float FieldOfView = 45f;// ����p
            public Vector3 OffsetPosition = new Vector3(0f, 1f, 0f);
            public Vector3 OffsetAngles;
            [Header("�p�x����")]
            [Tooltip("X���̐�����t���邩")]
            public bool IsLimitAngleX;
            public LimitAngle LimitAngleX;
            [Tooltip("Y���̐�����t���邩")]
            public bool IsLimitAngleY;
            public LimitAngle LimitAngleY;

            public static Parameter Lerp(Parameter a, Parameter b, float t, Parameter ret)//TODO: �����K���߂���̂ŕς��܂��傤�@Vecter3.Lerp�Ɠ������ɂ��Ă�
            {
                ret.Position = Vector3.Lerp(a.Position, b.Position, t);
                ret.Angles = LerpAngles(a.Angles, b.Angles, t);
                ret.Distance = Mathf.Lerp(a.Distance, b.Distance, t);
                ret.FieldOfView = Mathf.Lerp(a.FieldOfView, b.FieldOfView, t);
                ret.OffsetPosition = Vector3.Lerp(a.OffsetPosition, b.OffsetPosition, t);
                ret.OffsetAngles = LerpAngles(a.OffsetAngles, b.OffsetAngles, t);

                // �p�x�����ނ̔��f
                ret.IsLimitAngleX = b.IsLimitAngleX;
                ret.LimitAngleX = b.LimitAngleX;
                ret.IsLimitAngleY = b.IsLimitAngleY;
                ret.LimitAngleY = b.LimitAngleY;

                return ret;
            }

            /// <summary>
            /// �p�x�����炩�ɕς���
            /// </summary>
            /// <param name="currentAngle">�p�xA</param>
            /// <param name="afterAngle">�p�xB</param>
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
