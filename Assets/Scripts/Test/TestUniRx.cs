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
    //�C�x���g�𔭍s����j�ƂȂ�C���X�^���X
    private Subject<int> timerSubject = new Subject<int>();

    //�C�x���g�̍w�Ǒ����������J
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
        //100����J�E���g�_�E��
        var time = 100;
        while (time > 0)
        {
            time--;

            //�C�x���g�𔭍s
            timerSubject.OnNext(time);

            //1�b�҂�
            yield return new WaitForSeconds(1);
        }
    }
}
public class TimerView : MonoBehaviour
{
    //���ꂼ��C���X�^���X�̓C���X�y�N�^�r���[����ݒ�

    [SerializeField] private TestUniRx testUniRx;
    [SerializeField] private Text counterText; //uGUI��Text

    void Start()
    {
        //�^�C�}�̃J�E���^���ω������C�x���g���󂯂�uGUI Text���X�V����
        testUniRx?.OnTimeChanged.Subscribe(time =>
        {
            //���݂̃^�C�}�l��UI�ɔ��f����
            counterText.text = time.ToString();
        });

        // time��0�ɂȂ�����subscribe�̒��g�i�|�W�V������000�ɂȂ�j�����s�@�X�g���[������gameobject��������ƍw�ǒ��~
        testUniRx.OnTimeChanged
            .Where(time => time == 0)
            .CatchIgnore()
            .Subscribe(_ => 
            { 
                transform.position = Vector3.zero; 
            },
            () => Debug.Log("OnCompleted")
            )
            .AddTo(gameObject);// gameObject���폜���ꂽ��X�g���[���̍w�ǂ����~�����
    }
}