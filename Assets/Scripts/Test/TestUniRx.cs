using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.ComponentModel;
using System;
using System.Xml.Linq;

public class TestUniRx : MonoBehaviour
{
    //イベントを発行する核となるインスタンス
    private Subject<int> timerSubject = new Subject<int>();

    //イベントの購読側だけを公開
    public IObservable<int> OnTimeChanged
    {
        get { return timerSubject; }
    }

    void Start()
    {
        StartCoroutine(TimerCoroutine());
    }

    IEnumerator TimerCoroutine()
    {
        //100からカウントダウン
        var time = 100;
        while (time > 0)
        {
            time--;

            //イベントを発行
            timerSubject.OnNext(time);

            //1秒待つ
            yield return new WaitForSeconds(1);
        }
    }
}
public class TimerView : MonoBehaviour
{
    //それぞれインスタンスはインスペクタビューから設定

    [SerializeField] private TestUniRx testUniRx;
    [SerializeField] private Text counterText; //uGUIのText

    void Start()
    {
        //タイマのカウンタが変化したイベントを受けてuGUI Textを更新する
        testUniRx?.OnTimeChanged.Subscribe(time =>
        {
            //現在のタイマ値をUIに反映する
            counterText.text = time.ToString();
        });

        // timeが0になった時subscribeの中身（ポジションが000になる）が実行　ストリーム中にgameobjectが消えると購読中止
        testUniRx.OnTimeChanged
            .Where(time => time == 0)
            .CatchIgnore()
            .Subscribe(_ => 
            { 
                transform.position = Vector3.zero; 
            },
            () => Debug.Log("OnCompleted")
            )
            .AddTo(gameObject);// gameObjectが削除されたらストリームの購読も中止される
    }
}