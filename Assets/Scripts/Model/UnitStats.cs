using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Unit
{
    public class UnitStats : MonoBehaviour
    {
        [NonSerialized] public bool IsDead; // ���S�̐^�U�l
        private ReactiveProperty<int> _health = new ReactiveProperty<int>(); // ���݂̃w���X�l
        public IReadOnlyReactiveProperty<int> Health => _health;


        [Header("Canvas�ݒ�")]
        [SerializeField] private GameObject _healthCanvasPrefab;
        [SerializeField] private int _magnificationCanvasScale = 1;// Canvas�̑傫���@���{���邩
        [SerializeField] private Vector3 _canvasPos = new Vector3(0, 2, 0); // Canvas �̈ʒu
        [SerializeField] private int _maxHealth; // ���g�̍ő�w���X�l
        [SerializeField, Tooltip("���G����(�b)")]
        private float _invincibilityTimeSeconds = 1;

        [NonSerialized] public GameObject MyCanvas; // ���g��Canvas
        private UnitBase _myUnit;
        private Image _imgHealth; // �w���X�o�[
        private Text _txtHealth; // �w���X����
        private Camera _camera;//NOTE: Camera.main�Ŏ���Shake���J�����̐؂�ւ��Ńo�O��̂�

        private void Awake()
        {
            MyCanvas = Instantiate(_healthCanvasPrefab);
        }
        private void Start()
        {
            // ���g�̃w���X��\��
            MyCanvas.transform.localScale *= _magnificationCanvasScale;
            MyCanvas.transform.SetParent(gameObject.transform); // Canvas�����g�̎q�\����
            MyCanvas.transform.position = transform.position + _canvasPos; // �L�����o�X�̈ʒu�␳
            MyCanvas.transform.Find("imgHealth").TryGetComponent(out _imgHealth);
            MyCanvas.transform.Find("txtHealth").TryGetComponent(out _txtHealth);
            TryGetComponent(out _myUnit);
            _camera = Camera.main;
            Ready();// ������


            // Weapon�^�O�ɓ���������_���[�W�����炤
            this.OnTriggerEnterAsObservable()
                .Where(other => other.gameObject.CompareTag("Weapon") && !IsDead)
                .ThrottleFirst(TimeSpan.FromSeconds(_invincibilityTimeSeconds)) // �Q��ڈȍ~,�ݒ�b�ԍU���������󂯕t���Ȃ� (�P��ڂ͒ʉ�)
                .Subscribe(other => TriggerEnter(other));

            // �I�u�W�F�N�g�X�g���[����~
            _health.AddTo(this);
            // View healthBar�̍X�V
            _health.Subscribe(health =>
                {
                    // HPBar�̕ύX
                    UpdateHealthBar(health);
                    // HP�e�L�X�g�̕ύX
                    UpdateHealthText(health);
                    if (gameObject.name != "Dragon")
                        // HPBar�̐F�ύX
                        ChangeHealthImageColor();
                });

            // Hp���Ȃ��Ȃ��������S�������s��
            _health.Where(helth => helth <= 0)// Hp��0�ȉ��ɂȂ�����
                .Subscribe(_ =>
                {
                    // ���S����
                    IsDead = true; // ���S���w�肷��
                    _myUnit.OnDeath().Forget();
                },
                er => { Debug.Log("�G���["); }
                );

        }
        private void Update()
        {
            // �L�����o�X���J�����Ɍ�����
            MyCanvas.transform.forward = _camera.transform.forward;
        }
        /// <summary>
        /// HP�摜�����݂�HP�����ŕύX����
        /// </summary>
        private void ChangeHealthImageColor()
        {
            if (_imgHealth.fillAmount > 0.5f) _imgHealth.color = Color.green;
            else if (_imgHealth.fillAmount > 0.2f) _imgHealth.color = Color.yellow;
            else _imgHealth.color = Color.red;
        }
        /// <summary>
        /// HPBar�̍X�V
        /// </summary>
        /// <param name="health">���݂�HP</param>
        private void UpdateHealthBar(int health)
        {
            _imgHealth.fillAmount = health / (float)_maxHealth;
        }
        /// <summary>
        /// HP�e�L�X�g�̍X�V
        /// </summary>
        /// <param name="health">���݂�HP</param>
        private void UpdateHealthText(int health)
        {
            _txtHealth.text = health.ToString("f0") + "/" + _maxHealth.ToString("f0");
        }

        /*
        private void OnTriggerEnter(Collider other)
        {
            // ����łȂ��āA�^�OWeapon���N��������
            if (other.gameObject.CompareTag("Weapon") && !IsDead)
            {
                // ����̌��݂̍U����Power���Ɖ�A���g�̃w���X�l�����炷
                ReactivePropertyHealth.Value -= other.gameObject.GetComponent<WeaponAction>().Power;
                Debug.Log($"{this.gameObject.name}�́A{other.gameObject.transform.root.name}����{other.gameObject.GetComponent<WeaponAction>().Power}�_���[�W���󂯂�");
                ReactivePropertyHealth.Value = Mathf.Clamp(ReactivePropertyHealth.Value, 0, _maxHealth); // �w���X�l�����͈͓��Ɏ��߂�
                //�_���[�W�G�t�F�N�g����
                _myUnit.OnDamage();

                if (ReactivePropertyHealth.Value <= 0.0f)
                { // ���S����
                    IsDead = true; // ���S���w�肷��
                    _myUnit.OnDeath().Forget();
                }
            }
        }
        */
        private void TriggerEnter(Collider other)
        {
            // ����̌��݂̍U����Power���Ɖ�A���g�̃w���X�l�����炷
            _health.Value -= other.gameObject.GetComponent<WeaponAction>().Power;
            Debug.Log($"{this.gameObject.name}�́A{other.gameObject.transform.root.name}����{other.gameObject.GetComponent<WeaponAction>().Power}�_���[�W���󂯂�");
            _health.Value = Mathf.Clamp(_health.Value, 0, _maxHealth); // �w���X�l�����͈͓��Ɏ��߂�

            //�_���[�W�����i�G�t�F�N�g��o�C�u�Ȃǁj�@HACK: �_�T������_�ł�����������
            _myUnit.OnDamage();
        }

        /// <summary>
        /// ������
        /// </summary>
        public void Ready()
        {
            IsDead = false; // ����ł��Ȃ�
            _health.Value = _maxHealth; // �w���X�l���ő�ɂ���
            _imgHealth.fillAmount = 1;
        }
        /// <summary>
        /// HP�ϓ����ɍő�l�ƍŏ��l�𒴂��Ȃ��悤�ɂ���B
        /// </summary>
        /// <param name="Value"></param>
        public void ChangeHealth(int Value)
        {
            _health.Value =
                Mathf.Clamp(_health.Value + Value, 0, _maxHealth);// 0�ȏォ�A_maxHealth����ɂȂ�Ȃ��悤��
        }
    }
}
