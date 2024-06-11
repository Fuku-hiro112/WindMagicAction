using UnityEngine;

public class StateMachine : MonoBehaviour
{
    // 現在のState
    public IState CurrentState { get; private set; }

    // State一覧
    public IState IdleState   { get; private set; } // 待機
    public IState WalkState   { get; private set; } // 移動
    public IState DashState   { get; private set; } // ダッシュ
    public IState AttackState { get; private set; } // 攻撃
    public IState MagicState  { get; private set; } // 魔法
    public IState AvoidState  { get; private set; } // 回避
    public IState DamageState { get; private set; } // ダメージ
    public IState DeathState  { get; private set; } // 死亡

    public StateMachine(PlayerController playerController)
    {
        IdleState = new IdleState(playerController);
        //WalkState = new WalkState(playerController);
        //DashState = new DashState(playerController);
        //AttackState = new AttackState(playerController);
        //MagicState = new MagicState(playerController);
        //AvoidState = new AvoidState(playerController);
        //DamageState = new DamageState(playerController);
        //DeathState = new DeathState(playerController);
    }

    /// <summary>
    /// Stateの初期化
    /// </summary>
    /// <param name="startingState">最初のState</param>
    public void Initialize(IState startingState)
    {
        // 初期Stateを設定
        CurrentState = startingState;

        // Stateを開始
        startingState.Enter();
    }

    /// <summary>
    /// 現在のStateを終了し、次のStateに移行する
    /// </summary>
    /// <param name="nextState"></param>
    public void TransitionTo(IState nextState)
    {
        // 過去のStateを終了
        CurrentState.Exit();

        // 現在のStateを更新
        CurrentState = nextState;

        // 新規Stateを開始
        nextState.Enter();
    }

    public void Update()
    {
        if (CurrentState != null)
        {
            // 状態を更新
            CurrentState.Update();
        }
    }
}

public interface IState
{
    /// <summary>
    /// State開始時に実行される
    /// </summary>
    public void Enter();
    /// <summary>
    /// フレーム単位で実行される、新しい状態に移行するための条件も書く
    /// </summary>
    public void Update();
    /// <summary>
    /// State終了時に実行される
    /// </summary>
    public void Exit();
}
