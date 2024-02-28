using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using DG.Tweening;
using UniRx;

public class ShakeCamera : MonoBehaviour
{
    private GameObject _shakeCameraObj;
    private Camera _shakeCamera;
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
        _shakeCameraObj = this.gameObject;
        TryGetComponent(out _shakeCamera);
        Assert.IsNotNull(_shakeCamera, $"{this}��_shakeCamera��������܂���ł����I");
        _shakeCamera.enabled = false;// �J�����I�t
        
        // ���C���̃J�����Ŏ���p�̕ύX����������Shake�J�����ɂ����f����@�Ȃ��Ǝv�����ǈꉞ��
        var fieldOfView = new ReactiveProperty<float>(Camera.main.fieldOfView);
        fieldOfView.DistinctUntilChanged()
            .Subscribe(value =>
            {
                _shakeCamera.fieldOfView = value;
            }).AddTo(this);
    }
    /// <summary>
    /// �J������h�炷
    /// </summary>
    /// <param name="positionStrength"></param>
    /// <param name="rotationStrength"></param>
    /// <param name="shakeDuration"></param>
    public void Shake(Vector3 positionStrength, Vector3 rotationStrength, float shakeDuration)
    {
        Transform camera = _shakeCameraObj.transform;
        
        // �O��Tween���܂����s���̏ꍇ�ɁA����𑦊�����ԂɈȍ~����B
        camera.DOComplete();//NOTE: Kill���ƒ��f���Ă��܂����߃J���������̈ʒu�ɖ߂�Ȃ�
        camera.DOShakeRotation(shakeDuration, rotationStrength);// �h�炷����,�p�x�h��̋��x
        camera.DOShakePosition(shakeDuration, positionStrength) // �h�炷����,�|�W�V�����̗h��̋��x
            .OnStart   (() => SwicthShakeCamera(true))  // �J�n�� Shake�J�����؂�ւ�
            .OnComplete(() => SwicthShakeCamera(false));// �I���� Main�J�����؂�ւ�
    }

    /// <summary>
    /// �V�F�C�N�J�����ɐ؂�ւ�
    /// </summary>
    /// <param name="shake">�V�F�C�N�J�����ɐ؂�ւ��邩�ǂ���</param>
    private void SwicthShakeCamera(bool shake)
    {
        _mainCamera.enabled  = !shake;
        _shakeCamera.enabled = shake;
    }
}
