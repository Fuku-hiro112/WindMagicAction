using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Unit
{
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private int _maxMagicPoint = 100;
        private ReactiveProperty<int> _magicPoint = new ReactiveProperty<int>();
        //public IReactiveProperty<int> MagicPoint;

        private UnitStats _unitStats;
        private Image _imgMagicPoint; // �w���X�o�[
        private Text _txtMagicPoint; // �w���X����

        void Start()
        {
            TryGetComponent(out _unitStats);
            Assert.IsNotNull(_unitStats, $"{this}��_unitStats��Null�ł��B");
            _unitStats.MyCanvas.transform.Find("imgMagicPoint").TryGetComponent(out _imgMagicPoint);
            _unitStats.MyCanvas.transform.Find("txtMagicPoint").TryGetComponent(out _txtMagicPoint);

            // MP������
            _magicPoint.Value = _maxMagicPoint;

            _magicPoint.AddTo(this);
            // MPBar�����@MP������Ƃ��ꂪ���f�����
            _magicPoint.Subscribe(magicPoint =>
                {
                    // MPBar�ύX
                    UpdateMagicPointBar(magicPoint);
                    // MP�e�L�X�g��ύX
                    UpdateMagicPointText(magicPoint);
                });
        }

        public void Ready()
        {
            _magicPoint.Value = _maxMagicPoint;
            _imgMagicPoint.fillAmount = 1;
        }

        /// <summary>
        /// MP�ϓ����ɍő�l�ƍŏ��l�𒴂��Ȃ��悤�ɂ���B
        /// </summary>
        /// <param name="Value"></param>
        public void ChangeMagicPoint(int Value)
        {
            _magicPoint.Value =
                Mathf.Clamp(_magicPoint.Value + Value, 0, _maxMagicPoint);// 0�ȏォ�A_maxHealth����ɂȂ�Ȃ��悤��
        }
        /// <summary>
        /// MP���\�����ǂ���
        /// </summary>
        /// <param name="costMP"></param>
        /// <returns></returns>
        public bool IsMagicPointEnough(int costMP)
        {
            if (costMP <= _magicPoint.Value)
                return true;
            else
                return false;
        }

        /// <summary>
        /// MagicPoint�o�[�̍X�V
        /// </summary>
        /// <param name="magicPoint"></param>
        /// <param name="maxMagicPoint"></param>
        private void UpdateMagicPointBar(int magicPoint)
        {
            _imgMagicPoint.fillAmount = magicPoint / (float)_maxMagicPoint;
        }
        /// <summary>
        /// MagicPoint�e�L�X�g�̍X�V
        /// </summary>
        /// <param name="magicPoint"></param>
        /// <param name="maxMagicPoint"></param>
        private void UpdateMagicPointText(int magicPoint)
        {
            _txtMagicPoint.text = magicPoint.ToString("f0") + "/" + _maxMagicPoint.ToString("f0");
        }
    }
}
