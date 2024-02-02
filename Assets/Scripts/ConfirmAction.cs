using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem; //新Inputシステムの利用に必要
using UnityEngine.InputSystem.UI;
using UnityEngine.Windows;

namespace GameInput
{
    public class ConfirmAction : MonoBehaviour
    {
        [Header("スティックの入力値")]
        [SerializeField] private Vector2 _stickL;
        [SerializeField] private Vector2 _stickR;
        [Header("方向パッドの入力値")]
        [SerializeField] private Vector2 _pad;
        [Header("トリガーの入力値")]
        [SerializeField] private Vector2 _trigger;
        [Header("ボタン押下")]
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
        [Header("ボタンとしての方向パッド")]
        [SerializeField] private bool[] _btnPad = new bool[4];

        public static ConfirmAction s_Instance;// 使いやすいように

        //TODO: 移動値　視点操作値　といった変数名にしよう
        public Vector3 MoveDirection { get; private set; }
        public Vector2 LookDirection { get; private set; }

        public PlayerControls InputAction;
        public InputSystemUIInputModule InputModuleUI;// UI操作になった場合はenable=falseにする

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
            else
            {
                Destroy(this);// ２つ以上あると動かない？らしいので１つに
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
            GameObject.FindGameObjectsWithTag("GameController")? // tagを検索しその中から名前を見つけ指定の型を渡す
                .FirstOrDefault(obj => obj.name == "EventSystem")?.TryGetComponent(out InputModuleUI);

        }
        /// <summary>
        /// 移動ボタン
        /// </summary>
        /// <param name="context"></param>
        private void OnMove(InputAction.CallbackContext context)
        {
            //方向を取る　Vecter2
            Vector2 direction2 = context.ReadValue<Vector2>().normalized;// 斜め移動が早くならないよう正規化している
            MoveDirection = new Vector3(direction2.x, 0, direction2.y);
            //Debug.Log(MoveDirection);
        }
        private void OnMoveStop(InputAction.CallbackContext context) => MoveDirection = Vector3.zero;

        /// <summary>
        /// 視点移動ボタン
        /// </summary>
        /// <param name="context"></param>
        private void OnLook(InputAction.CallbackContext context)
        {
            //方向を取る　Vecter2
            LookDirection = context.ReadValue<Vector2>();
        }
        private void OnLookStop(InputAction.CallbackContext context) => LookDirection = Vector2.zero;
        /// <summary>
        /// 攻撃ボタン
        /// </summary>
        /// <param name="context"></param>
        private void OnFire(InputAction.CallbackContext context)
        {
            //Boolで取る
        }
        /// <summary>
        /// 魔法使用ボタン
        /// </summary>
        /// <param name="context"></param>
        private void OnMagic(InputAction.CallbackContext context)
        {
            //Boolで取る
        }
        /// <summary>
        /// 回避ボタン
        /// </summary>
        /// <param name="context"></param>
        private void OnAvoid(InputAction.CallbackContext context)
        {
            //Boolで取る
        }

        private void Update()
        {
            if (Gamepad.current == null) return; //必ずGamepadが有効かどうかを確認してから値を参照すること

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

            //Gamepad.current.SetMotorSpeeds(Trigger.x, Trigger.y); //トリガーをバイブレーションに転用
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
            _btnPad[0] = Gamepad.current.dpad.right.isPressed; //方向パッドをボタンとして検出可能
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