using GameInput;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    void Start()
    {
        this.UpdateAsObservable()
            .Where(_ => ConfirmAction.s_Instance.InputAction.Player.Decision.WasPerformedThisFrame())// 決定ボタンが押されたら
            .Subscribe(_ => SceneManager.LoadScene("GameScene"));// GameSceneへ
    }
}
