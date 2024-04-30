using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unit;
using DG.Tweening;

public class SerectMagicView : MonoBehaviour
{
    //NOTE: SerectMagicの順番通りにImageを登録しよう
    // 0:誘導弾　1:範囲魔法 2:斬撃魔法 3:強化魔法
    [SerializeField] private Image[] _imgMagics = new Image[4];
    [SerializeField] private Text _txtRequiredMagicPoint;

    [SerializeField] private float _duration = 2f; // 点滅の1サイクルの時間（秒）
    [SerializeField] private float _minAlpha = 0.5f; // 最小のアルファ値
    [SerializeField] private float _maxAlpha = 1f; // 最大のアルファ値

    Sequence _sequence;
    private void Reset()
    {
        int imgMagicLange = 4;
        for (int i = 0; i < imgMagicLange; i++)
        {
            _imgMagics[i] = transform.GetChild(i).GetComponent<Image>();
        }
    }
    void Start()
    {
        TextFlashing();
    }

    /// <summary>
    /// 選択中の魔法を可視化
    /// </summary>
    public void VisualizeChoosingMagic(SerectMagic serectMagic, int requiredPoint)
    {
        for (int i = 0;i < _imgMagics.Length;i++)
        {
            // SerectMagicと同じ数字のImageの色を白に
            if (serectMagic == (SerectMagic)i)
            {
                // 白に　NOTE: 半透明になったものを白に戻す
                _imgMagics[i].color =  Color.white;

                _txtRequiredMagicPoint.text = requiredPoint.ToString();

                // Imageの上にテキストを配置
                Vector3 newPosition = _imgMagics[i].transform.localPosition + Vector3.up * 63f;
                _txtRequiredMagicPoint.transform.localPosition = newPosition;

                AudioManager.Instance.PlaySE(SESoundData.SE.Decision);
            }
            else
            {
                // 半透明(alpha値を半分)に
                Color color = _imgMagics[i].color;
                float alpha = 0.5f;
                color.a = alpha;
                _imgMagics[i].color = color;
            }
        }
    }

    /// <summary>
    /// テキストを点滅させる
    /// </summary>
    private void TextFlashing()
    {
        _sequence.Kill();//HACK: こっちは要らないかな？　OnDestroyが出ない場合があるかもしれないから一応付けている

        // ループするTweenを作成
        _sequence = DOTween.Sequence();
        _sequence.Append(_txtRequiredMagicPoint.DOFade(_minAlpha, _duration / 2)) // 最大のアルファ値に変更
                 .Append(_txtRequiredMagicPoint.DOFade(_maxAlpha, _duration / 2)) // 最小のアルファ値に変更
                 .SetLoops(-1); // 無限ループ

        // Tweenを再生
        _sequence.Play();
    }

    private void OnDestroy()
    {
        _sequence.Kill();
    }
}
