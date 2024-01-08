using UnityEngine;
using UnityEngine.UI;

public class CombatAction : MonoBehaviour
{
    GameObject _myCanvas; // ���g��Canvas
    Image _imgHealth; // �w���X�o�[
    Text _txtHealth; // �w���X����
    [SerializeField] GameObject _healthCanvasPrefab;
    [SerializeField] Vector3 _canvasPos = new Vector3(0, 2, 0); // Canvas �̈ʒu
    [System.NonSerialized] public bool IsDead; // ���S�̐^�U�l
    [System.NonSerialized] int _health; // ���݂̃w���X�l
    [SerializeField] int _maxHealth; // ���g�̍ő�w���X�l

    void Start()
    {
        // ���g�̃w���X��\��
        _myCanvas = Instantiate(_healthCanvasPrefab);
        _myCanvas.transform.SetParent(gameObject.transform); // Canvas�����g�̎q�\����
        _myCanvas.transform.position = transform.position + _canvasPos; // �L�����o�X�̈ʒu�␳
        _myCanvas.transform.Find("imgHealth").TryGetComponent(out _imgHealth);
        _myCanvas.transform.Find("txtHealth").TryGetComponent(out _txtHealth);
        Ready();
    }
    void Update()
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

    public void Ready()
    {
        IsDead = false; // ����ł��Ȃ�
        _health = _maxHealth; // �w���X�l���ő�ɂ���
        _imgHealth.fillAmount = 1;
    }
    public void ChangeHealth(int Value)
    {
        _health = Mathf.Clamp(_health + Value, 0, _maxHealth);
    }

    void OnTriggerEnter(Collider other)
    {
        // ����łȂ��āA�^�OWeapon���N��������
        if (other.gameObject.tag == "Weapon" && !IsDead)
        {
            // ����̌��݂̍U����Power���Ɖ�A���g�̃w���X�l�����炷
            _health -= other.gameObject.GetComponent<WeaponAction>().Power;
            _health = Mathf.Clamp(_health, 0, _maxHealth); // �w���X�l�����͈͓��Ɏ��߂�
            if (_health <= 0.0f)
            { // ���S����
                IsDead = true; // ���S���w�肷��
                SendMessage("OnDeath", SendMessageOptions.DontRequireReceiver);
            }
            //�_���[�W�G�t�F�N�g����
            SendMessage("OnDamage", SendMessageOptions.DontRequireReceiver);
        }
    }
}
