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
        private Image _imgMagicPoint; // ヘルスバー
        private Text _txtMagicPoint; // ヘルス文字

        void Start()
        {
            TryGetComponent(out _unitStats);
            Assert.IsNotNull(_unitStats, $"{this}の_unitStatsがNullです。");
            _unitStats.MyCanvas.transform.Find("imgMagicPoint").TryGetComponent(out _imgMagicPoint);
            _unitStats.MyCanvas.transform.Find("txtMagicPoint").TryGetComponent(out _txtMagicPoint);

            // MP初期化
            _magicPoint.Value = _maxMagicPoint;

            _magicPoint.AddTo(this);
            // MPBarを作り　MPが減るとそれが反映される
            _magicPoint.Subscribe(magicPoint =>
                {
                    // MPBar変更
                    UpdateMagicPointBar(magicPoint);
                    // MPテキストを変更
                    UpdateMagicPointText(magicPoint);
                });
        }

        public void Ready()
        {
            _magicPoint.Value = _maxMagicPoint;
            _imgMagicPoint.fillAmount = 1;
        }

        /// <summary>
        /// MP変動時に最大値と最小値を超えないようにする。
        /// </summary>
        /// <param name="Value"></param>
        public void ChangeMagicPoint(int Value)
        {
            _magicPoint.Value =
                Mathf.Clamp(_magicPoint.Value + Value, 0, _maxMagicPoint);// 0以上かつ、_maxHealthより上にならないように
        }
        /// <summary>
        /// MPが十分かどうか
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
        /// MagicPointバーの更新
        /// </summary>
        /// <param name="magicPoint"></param>
        /// <param name="maxMagicPoint"></param>
        private void UpdateMagicPointBar(int magicPoint)
        {
            _imgMagicPoint.fillAmount = magicPoint / (float)_maxMagicPoint;
        }
        /// <summary>
        /// MagicPointテキストの更新
        /// </summary>
        /// <param name="magicPoint"></param>
        /// <param name="maxMagicPoint"></param>
        private void UpdateMagicPointText(int magicPoint)
        {
            _txtMagicPoint.text = magicPoint.ToString("f0") + "/" + _maxMagicPoint.ToString("f0");
        }
    }
}
