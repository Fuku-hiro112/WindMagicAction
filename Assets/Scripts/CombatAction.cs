using UnityEngine;
using UnityEngine.UI;

public class CombatAction : MonoBehaviour
{
    [System.NonSerialized] public bool IsDead; // ���S�̐^�U�l
    [System.NonSerialized] public int _health; // ���݂̃w���X�l
    [Header("Canvas�ݒ�")]
    [SerializeField] private GameObject _healthCanvasPrefab;
    [SerializeField] private int _magnificationCanvasScale = 1;// Canvas�̑傫���@���{���邩
    [SerializeField] private Vector3 _canvasPos = new Vector3(0, 2, 0); // Canvas �̈ʒu
    [SerializeField] private int _maxHealth; // ���g�̍ő�w���X�l
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
        Ready();
    }
    private void Update()
    {
        // �w���X�o�[�̑����ƍʐF
        _imgHealth.fillAmount = _health / (float)_maxHealth;
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
        _txtHealth.text = _health.ToString("f0") + "/" + _maxHealth.ToString("f0");
        // �L�����o�X���J�����Ɍ�����
        _myCanvas.transform.forward = Camera.main.transform.forward;
    }
    private void OnTriggerEnter(Collider other)
    {
        // ����łȂ��āA�^�OWeapon���N��������
        if (other.gameObject.CompareTag("Weapon") && !IsDead)
        {
            // ����̌��݂̍U����Power���Ɖ�A���g�̃w���X�l�����炷
            _health -= other.gameObject.GetComponent<WeaponAction>().Power;
            Debug.Log($"{this.gameObject.name}�́A{other.gameObject.transform.root.name}����{other.gameObject.GetComponent<WeaponAction>().Power}�_���[�W���󂯂�");
            _health = Mathf.Clamp(_health, 0, _maxHealth); // �w���X�l�����͈͓��Ɏ��߂�
            if (_health <= 0.0f)
            { // ���S����
                IsDead = true; // ���S���w�肷��
                _myUnit.StartCoroutine(_myUnit.OnDeath());
            }
            //�_���[�W�G�t�F�N�g����
            _myUnit.OnDamage();
        }
    }

    /// <summary>
    /// ������
    /// </summary>
    public void Ready()
    {
        IsDead = false; // ����ł��Ȃ�
        _health = _maxHealth; // �w���X�l���ő�ɂ���
        _imgHealth.fillAmount = 1;
    }
    /// <summary>
    /// HP�ϓ����ɍő�l�ƍŏ��l�𒴂��Ȃ��悤�ɂ���B
    /// </summary>
    /// <param name="Value"></param>
    public void ChangeHealth(int Value)
    {
        _health = Mathf.Clamp(_health + Value, 0, _maxHealth);
    }
}
