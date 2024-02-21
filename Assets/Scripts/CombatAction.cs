using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class CombatAction : MonoBehaviour
{
    [NonSerialized] public bool IsDead; // ���S�̐^�U�l
    [NonSerialized] public ReactiveProperty<int> ReactivePropertyHealth; // ���݂̃w���X�l

    [Header("Canvas�ݒ�")]
    [SerializeField] private GameObject _healthCanvasPrefab;
    [SerializeField] private int _magnificationCanvasScale = 1;// Canvas�̑傫���@���{���邩
    [SerializeField] private Vector3 _canvasPos = new Vector3(0, 2, 0); // Canvas �̈ʒu
    [SerializeField] private int _maxHealth; // ���g�̍ő�w���X�l
    [SerializeField, Tooltip("���G����(�b)")] 
                     private float _invincibilityTimeSeconds = 1;

    private GameObject _myCanvas; // ���g��Canvas
    private UnitBase _myUnit;
    private Image _imgHealth; // �w���X�o�[
    private Text _txtHealth; // �w���X����

    private void Start()
    {
        // ���g�̃w���X��\��
        _myCanvas = Instantiate(_healthCanvasPrefab);
        _myCanvas.transform.localScale *= _magnificationCanvasScale;
        _myCanvas.transform.SetParent(gameObject.transform); // Canvas�����g�̎q�\����
        _myCanvas.transform.position = transform.position + _canvasPos; // �L�����o�X�̈ʒu�␳
        _myCanvas.transform.Find("imgHealth").TryGetComponent(out _imgHealth);
        _myCanvas.transform.Find("txtHealth").TryGetComponent(out _txtHealth);
        TryGetComponent(out _myUnit);
        Ready();// ������


        // Weapon�^�O�ɓ���������_���[�W�����炤
        this.OnTriggerEnterAsObservable()
            .Where(other => other.gameObject.CompareTag("Weapon") && !IsDead)
            .ThrottleFirst(TimeSpan.FromSeconds(_invincibilityTimeSeconds)) // �Q��ڈȍ~,�ݒ�b�ԍU���������󂯕t���Ȃ� (�P��ڂ͒ʉ�)
            .Subscribe(other => TriggerEnter(other));

        ReactivePropertyHealth
            .Subscribe(health => 
            {
                _imgHealth.fillAmount = health / (float)_maxHealth;
            }).AddTo(this);
        // Hp���Ȃ��Ȃ��������S�������s��
        ReactivePropertyHealth.Where(helth => helth <= 0)// Hp��0�ȉ��ɂȂ�����
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
        // �w���X�o�[�̑����ƍʐF
        //_imgHealth.fillAmount = ReactivePropertyHealth.Value / (float)_maxHealth;
        if (_imgHealth.fillAmount > 0.5f)
        {
            _imgHealth.color = Color.green;
        }
        else if (_imgHealth.fillAmount > 0.2f)
        {
            _imgHealth.color = Color.yellow;
        }
        else
        {
            _imgHealth.color = Color.red;
        }
        // �w���X�l
        _txtHealth.text = ReactivePropertyHealth.Value.ToString("f0") + "/" + _maxHealth.ToString("f0");
        // �L�����o�X���J�����Ɍ�����
        _myCanvas.transform.forward = Camera.main.transform.forward;
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
        ReactivePropertyHealth.Value -= other.gameObject.GetComponent<WeaponAction>().Power;
        Debug.Log($"{this.gameObject.name}�́A{other.gameObject.transform.root.name}����{other.gameObject.GetComponent<WeaponAction>().Power}�_���[�W���󂯂�");
        ReactivePropertyHealth.Value = Mathf.Clamp(ReactivePropertyHealth.Value, 0, _maxHealth); // �w���X�l�����͈͓��Ɏ��߂�
        
        //�_���[�W�G�t�F�N�g�����@HACK: �_�T������_�ł�����������
        _myUnit.OnDamage();
    }

    /// <summary>
    /// ������
    /// </summary>
    public void Ready()
    {
        IsDead = false; // ����ł��Ȃ�
        ReactivePropertyHealth = new ReactiveProperty<int>(_maxHealth); // �w���X�l���ő�ɂ���
        _imgHealth.fillAmount = 1;
    }
    /// <summary>
    /// HP�ϓ����ɍő�l�ƍŏ��l�𒴂��Ȃ��悤�ɂ���B
    /// </summary>
    /// <param name="Value"></param>
    public void ChangeHealth(int Value)
    {
        ReactivePropertyHealth.Value = 
            Mathf.Clamp(ReactivePropertyHealth.Value + Value, 0, _maxHealth);// 0�ȏォ�A_maxHealth����ɂȂ�Ȃ��悤��
    }
}
