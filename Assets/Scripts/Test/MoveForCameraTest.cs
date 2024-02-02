using UnityEngine;

public class MoveForCameraTest : MonoBehaviour
{
    // �ړ��X�s�[�h
    [SerializeField]
    [Min(0.0f)]
    private float speed = 10.0f;
    // ���W�b�h�{�f�B
    private Rigidbody _rb = null;
    // ����
    private Vector2 m_inputMove = Vector2.zero;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        // �ړ�����
        float hori = Input.GetAxis("Horizontal");
        float var = Input.GetAxis("Vertical");

        m_inputMove = new Vector2(hori, var).normalized * Time.deltaTime * 100;
        Vector3 movingVelocity = new Vector3(m_inputMove.x * speed, 0, m_inputMove.y * speed);
        //_rb.AddForce(movingVelocity);
        _rb.AddForce(movingVelocity, ForceMode.Force);
    }
    private void FixedUpdate()
    {
        //Vector3 movingVelocity = new Vector3(m_inputMove.x * speed, 0, m_inputMove.y * speed);
        //_rb.AddForce(movingVelocity);
        //_rb.AddForce(movingVelocity, ForceMode.Force);
    }
}