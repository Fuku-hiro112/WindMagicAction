using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TargetTracking : MonoBehaviour
{
    [SerializeField]private TestTargetDetermination _targetDetermination;
    Image _image;

    [SerializeField] private float _animationTakesSeconds;
    [SerializeField] private float _animationRotateNumber;

    private Sequence _sequence;

    void Start()
    {
        TryGetComponent(out _image);
        _targetDetermination.Target.Subscribe(Observer.Create<Transform>(obj =>
        {
            if (obj == null)
            {
                _image.enabled = false;
            }
            else
            {
                _sequence.Kill();
                _image.enabled = true;
                transform.localScale = Vector3.zero;

                _sequence = DOTween.Sequence();
                _sequence.Append(transform.DOLocalRotate(Vector3.forward * 360 * _animationRotateNumber, _animationTakesSeconds, RotateMode.FastBeyond360))
                         .Join  (transform.DOScale (Vector3.one, _animationTakesSeconds)).Play();
            }
        }));
    }

    void Update()
    {
        if (_targetDetermination.Target.Value == null) return;
        transform.position = 
            RectTransformUtility.WorldToScreenPoint(Camera.main, _targetDetermination.Target.Value.position + Vector3.up);
    }
}
