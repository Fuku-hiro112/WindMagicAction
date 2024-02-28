using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TargetTrackingView : MonoBehaviour
{

    [SerializeField] private float _animationTakesSeconds;
    [SerializeField] private float _animationRotateNumber;

    private Image _targetImage;
    private Sequence _sequence;
    private Camera _camera;

    private void Awake()
    {
        TryGetComponent(out _targetImage);
        _camera = Camera.main;
        _targetImage.enabled = false;
    }

    void Update()
    {
        //if (_targetDetermination.TargetObj.Value == null) return;
        //transform.position = 
        //    RectTransformUtility.WorldToScreenPoint(Camera.main, _targetDetermination.TargetObj.Value.transform.position + Vector3.up);
    }
    /// <summary>
    /// �J�[�\���ʒu�̒���
    /// </summary>
    /// <param name="pos"></param>
    public void AdjustCursorPosition(Vector3 pos)
    {
        transform.position =
            RectTransformUtility.WorldToScreenPoint(_camera, pos + Vector3.up);//HACK: Vector3.up�͍����␳�ł��B�@
    }
    /// <summary>
    /// �^�[�Q�b�g�摜�̕\���̐؂�ւ�
    /// </summary>
    /// <param name="obj"></param>
    public void ToggleCursorVisibility(bool isDisplay)
    {
        _targetImage.enabled = isDisplay;
        if(isDisplay) DisplayAnimation();
    }
    /// <summary>
    /// �\���A�j���[�V�����@�傫���Ȃ�Ȃ����]����
    /// </summary>
    private void DisplayAnimation()
    {
        // �V�[�P���X�Ƒ傫����������
        _sequence.Kill();
        transform.localScale = Vector3.zero;

        // �V�[�P���X����
        _sequence = DOTween.Sequence();
        // �A�j���[�V�����Đ��@��]���Ȃ���傫���Ȃ�
        _sequence.Append(transform.DOLocalRotate(Vector3.forward * 360 * _animationRotateNumber, _animationTakesSeconds, RotateMode.FastBeyond360))
                 .Join(transform.DOScale(Vector3.one, _animationTakesSeconds)).Play();
    }
}
