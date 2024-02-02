using GameInput;
using UnityEngine;

public class TestCameraMove : MonoBehaviour
{
    [SerializeField] private float _bias = 8.0f;
    [SerializeField] private float _rotBias = 120.0f;
    [SerializeField] private Vector3 _camDir = new Vector3(0.0f, 4.0f, -4.0f);
    [SerializeField] private float _closeBias = 0.75f; //�ߊ�銄��
    [SerializeField] private float _closeTime = 5.0f; //�ߊ�鎞��
    [SerializeField] private GameObject _target; //�v���C���[
    private float _closeRatio = 1.0f; //�����̔{��
    private float _elapsed = 0.0f; //�o�ߎ���

    private void Start()
    {
        
    }
    public void Attack()
    {
        _elapsed = _closeTime;
    }
    private void Update()
    {
        _elapsed -= Time.deltaTime;
        _elapsed = Mathf.Clamp(_elapsed, 0.0f, _closeTime);
        _closeRatio = Mathf.Lerp(_closeRatio, //���݂̔{��
                                 (_elapsed == 0.0f) ? 1.0f : _closeBias, //�ڎw���{��
                                 Time.deltaTime);

        Camera.main.gameObject.transform.localPosition = _camDir * _closeRatio;
        Camera.main.gameObject.transform.LookAt(transform.position + Vector3.up);
    }
    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(
            transform.position, //���݂̈ʒu
            _target.transform.position, //�����������ʒu
            _bias * Time.fixedDeltaTime); //�}�C���h�ȍl���o�C�A�X
        if (Mathf.Abs(ConfirmAction.s_Instance.LookDirection.x) < 0.05f)
        {
            return; //�\���ɃW���C�X�e�B�b�N���|��Ă��Ȃ�����
        }
        transform.Rotate(
        0, ConfirmAction.s_Instance.LookDirection.x * _rotBias * Time.fixedDeltaTime, 0);
    }
}
