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

        private UnitStats _unitStats;
        private Image _imgMagicPoint; // ヘルスバー
        private Image _imgMagicCost; // ヘルスバー
        private Text _txtMagicPoint; // ヘルス文字

        void Start()
        {
            TryGetComponent(out _unitStats);
            
            Assert.IsNotNull(_unitStats, $"{this}の_unitStatsがNullです。");
            
            _unitStats.MyCanvas.transform.Find("imgMagicPoint").TryGetComponent(out _imgMagicPoint);
            _unitStats.MyCanvas.transform.Find("imgMagicCost").TryGetComponent(out _imgMagicCost);
            _unitStats.MyCanvas.transform.Find("txtMagicPoint").TryGetComponent(out _txtMagicPoint);

            Assert.IsNotNull(_imgMagicPoint, $"{this}の_imgMagicPointがNullです。");
            Assert.IsNotNull(_imgMagicCost , $"{this}の_imgMagicCostがNullです。");
            Assert.IsNotNull(_txtMagicPoint, $"{this}の_txtMagicPointがNullです。");

            Ready();

            // MP初期化
            _magicPoint.Value = 0;

            _magicPoint.AddTo(this);
            // MPBarを作り　MPが減るとそれが反映される
            _magicPoint.Subscribe(magicPoint =>
                {
                    float currentFillAmount = magicPoint / (float)_maxMagicPoint;
                    // MPBar変更
                    _unitStats.UpdateBarFillAmount(currentFillAmount, _imgMagicPoint, _imgMagicCost);
                    //UpdateMagicPointBar(magicPoint);
                    // MPテキストを変更
                    _unitStats.UpdateBarText(_txtMagicPoint, _maxMagicPoint, magicPoint);
                    //UpdateMagicPointText(magicPoint);
                });
        }

        public void Ready()
        {
            //TODO: 本番はココを0に使用
            _magicPoint.Value = _maxMagicPoint;
            //_magicPoint.Value = 0;

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
    }
}
