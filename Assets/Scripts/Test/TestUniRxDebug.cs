using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class TestUniRxDebug : MonoBehaviour
{
    void Start()
    {
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonDown(0))  // ���N���b�N�������ꂽ��
            .Subscribe(_ => Debug.Log("�N���b�N"));
    }
}
