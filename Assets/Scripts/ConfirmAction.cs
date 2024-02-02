using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem; //�VInput�V�X�e���̗��p�ɕK�v
using UnityEngine.InputSystem.UI;
using UnityEngine.Windows;

namespace GameInput
{
    public class ConfirmAction : MonoBehaviour
    {
        [Header("�X�e�B�b�N�̓��͒l")]
        [SerializeField] private Vector2 _stickL;
        [SerializeField] private Vector2 _stickR;
        [Header("�����p�b�h�̓��͒l")]
        [SerializeField] private Vector2 _pad;
        [Header("�g���K�[�̓��͒l")]
        [SerializeField] private Vector2 _trigger;
        [Header("�{�^������")]
        [SerializeField] private bool _btnUp;
        [SerializeField] private bool _btnDown;
        [SerializeField] private bool _btnRight;
        [SerializeField] private bool _btnLeft;
        [SerializeField] private bool _bumperL;
        [SerializeField] private bool _bumperR;
        [SerializeField] private bool _btnBack;
        [SerializeField] private bool _btnStart;
        [SerializeField] private bool _btnJoyL;
        [SerializeField] private bool _btnJoyR;
        [Header("�{�^���Ƃ��Ă̕����p�b�h")]
        [SerializeField] private bool[] _btnPad = new bool[4];

        public static ConfirmAction s_Instance;// �g���₷���悤��

        //TODO: �ړ��l�@���_����l�@�Ƃ������ϐ����ɂ��悤
        public Vector3 MoveDirection { get; private set; }
        public Vector2 LookDirection { get; private set; }

        public PlayerControls InputAction;
        public InputSystemUIInputModule InputModuleUI;// UI����ɂȂ����ꍇ��enable=false�ɂ���

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
            else
            {
                Destroy(this);// �Q�ȏ゠��Ɠ����Ȃ��H�炵���̂łP��
            }
        }
        private void OnEnable()
        {
            InputAction = new PlayerControls();

            InputAction.Player.Move.performed += OnMove;
            InputAction.Player.Move.canceled += OnMoveStop;
            InputAction.Player.Look.performed += OnLook;
            InputAction.Player.Look.canceled += OnLook;
            InputAction.Player.Fire.started += OnFire;
            InputAction.Player.Magic.started += OnMagic;
            InputAction.Player.Avoid.started += OnAvoid;

            InputAction.Enable();
        }
        private void OnDisable()
        {
            InputAction.Player.Move.performed -= OnMove;
            InputAction.Player.Move.canceled -= OnMoveStop;
            InputAction.Player.Look.performed -= OnLook;
            InputAction.Player.Look.canceled -= OnLookStop;
            InputAction.Player.Fire.started -= OnFire;
            InputAction.Player.Magic.started -= OnMagic;

            InputAction.Player.Avoid.started -= OnAvoid;

            InputAction.Dispose();
        }
        private void Start()
        {
            GameObject.FindGameObjectsWithTag("GameController")? // tag�����������̒����疼�O�������w��̌^��n��
                .FirstOrDefault(obj => obj.name == "EventSystem")?.TryGetComponent(out InputModuleUI);

        }
        /// <summary>
        /// �ړ��{�^��
        /// </summary>
        /// <param name="context"></param>
        private void OnMove(InputAction.CallbackContext context)
        {
            //���������@Vecter2
            Vector2 direction2 = context.ReadValue<Vector2>().normalized;// �΂߈ړ��������Ȃ�Ȃ��悤���K�����Ă���
            MoveDirection = new Vector3(direction2.x, 0, direction2.y);
            //Debug.Log(MoveDirection);
        }
        private void OnMoveStop(InputAction.CallbackContext context) => MoveDirection = Vector3.zero;

        /// <summary>
        /// ���_�ړ��{�^��
        /// </summary>
        /// <param name="context"></param>
        private void OnLook(InputAction.CallbackContext context)
        {
            //���������@Vecter2
            LookDirection = context.ReadValue<Vector2>();
        }
        private void OnLookStop(InputAction.CallbackContext context) => LookDirection = Vector2.zero;
        /// <summary>
        /// �U���{�^��
        /// </summary>
        /// <param name="context"></param>
        private void OnFire(InputAction.CallbackContext context)
        {
            //Bool�Ŏ��
        }
        /// <summary>
        /// ���@�g�p�{�^��
        /// </summary>
        /// <param name="context"></param>
        private void OnMagic(InputAction.CallbackContext context)
        {
            //Bool�Ŏ��
        }
        /// <summary>
        /// ����{�^��
        /// </summary>
        /// <param name="context"></param>
        private void OnAvoid(InputAction.CallbackContext context)
        {
            //Bool�Ŏ��
        }

        private void Update()
        {
            if (Gamepad.current == null) return; //�K��Gamepad���L�����ǂ������m�F���Ă���l���Q�Ƃ��邱��

            _stickL = new Vector2(
            Gamepad.current.leftStick.ReadValue().x,
            Gamepad.current.leftStick.ReadValue().y);
            _stickR = new Vector2(
            Gamepad.current.rightStick.ReadValue().x,
            Gamepad.current.rightStick.ReadValue().y);
            _pad = new Vector2(
            Gamepad.current.dpad.ReadValue().x,
            Gamepad.current.dpad.ReadValue().y);
            _trigger = new Vector2(
            Gamepad.current.leftTrigger.ReadValue(),
            Gamepad.current.rightTrigger.ReadValue());

            //Gamepad.current.SetMotorSpeeds(Trigger.x, Trigger.y); //�g���K�[���o�C�u���[�V�����ɓ]�p
            _btnDown = Gamepad.current.buttonSouth.isPressed;
            _btnRight = Gamepad.current.buttonEast.isPressed;
            _btnLeft = Gamepad.current.buttonWest.isPressed;
            _btnUp = Gamepad.current.buttonNorth.isPressed;
            _bumperL = Gamepad.current.leftShoulder.isPressed;
            _bumperR = Gamepad.current.rightShoulder.isPressed;
            _btnBack = Gamepad.current.selectButton.isPressed;
            _btnStart = Gamepad.current.startButton.isPressed;
            _btnJoyL = Gamepad.current.leftStickButton.isPressed;
            _btnJoyR = Gamepad.current.rightStickButton.isPressed;
            _btnPad[0] = Gamepad.current.dpad.right.isPressed; //�����p�b�h���{�^���Ƃ��Č��o�\
            _btnPad[1] = Gamepad.current.dpad.down.isPressed;
            _btnPad[2] = Gamepad.current.dpad.left.isPressed;
            _btnPad[3] = Gamepad.current.dpad.up.isPressed;
        }
        /*
        void OnGUI()
        {
            if (Gamepad.current == null) return;

            GUILayout.Label($"Move: {MoveDirection}");
            GUILayout.Label($"Look: {LookDirection}");
            GUILayout.Label($"Fire: {InputAction.Player.Fire.IsPressed()}");
            GUILayout.Label($"Magic: {InputAction.Player.Magic.IsPressed()}");
            GUILayout.Label($"Avoid: {InputAction.Player.Avoid.IsPressed()}");
            /*
            GUILayout.Label($"leftStick: {_stickL}");
            GUILayout.Label($"RightStick: {_stickR}");
            GUILayout.Label($"Pad: {_pad}");
            GUILayout.Label($"Trigger: {_trigger}");
            GUILayout.Label($"ButtonA: {_btnDown}");
            GUILayout.Label($"ButtonB: {_btnRight}");
            GUILayout.Label($"ButtonX: {_btnLeft}");
            GUILayout.Label($"ButtonY: {_btnUp}");
            GUILayout.Label($"BumperL: {_bumperL}");
            GUILayout.Label($"BumperR: {_bumperR}");
            GUILayout.Label($"ButtonBack: {_btnBack}");
            GUILayout.Label($"ButtonStart: {_btnStart}");
            GUILayout.Label($"ButtonJoyL: {_btnJoyL}");
            GUILayout.Label($"ButtonJoyR: {_btnJoyR}");
            GUILayout.Label($"ButtonPad: {_btnPad[0]}, {_btnPad[1]}, {_btnPad[2]}, {_btnPad[3]}");
            *//*
        }
        */
    }
}