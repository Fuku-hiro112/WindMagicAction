using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

namespace Unit
{
    public enum EnemyState
    {
        Idle,      // 待機
        Wandering, // 徘徊
        Separation,// 分散
        Chase,     // 敵を追いかけ
        Attack, 　 // 攻撃
    }

    [RequireComponent(typeof(Animator), typeof(NavMeshAgent))]// AnimatorとNavMeshを必須に
    public class EnemyControllerBase : UnitBase
    {
        [Header("範囲")]
        [SerializeField, Tooltip("攻撃開始距離")] protected float FireDistance = 1.0f;
        [SerializeField, Tooltip("索敵範囲")] protected float SearchRange = 5.0f;
        [Space(20)]
        [SerializeField] protected float DeathTime = 3.0f; // 死亡後に消えるまでの時間
        [SerializeField] protected int TurningSpeed = 5;
        [SerializeField] private GameObject _patDamage; // ダメージエフェクト
        [SerializeField] protected Vector3 DamagePos = new Vector3(0, 1.5f, 0); // ダメージエフェクトの位置
        [Space(20)]
        [SerializeField] private float _waitIdleMin = 1;
        [SerializeField] private float _waitIdleMax = 2;
        private float _waitIdleStateTimer = 0f;
        // 徘徊ポジションに着いたとする　到達しきい値
        [SerializeField] private float _arrivalThreshold = 0.1f;

        protected NavMeshAgent MyNavi; // 自身のナビメッシュ
        protected Animator MyAnim; // 自身のアニメーター
        protected UnitStats MyStats;   // 自身のStats
        protected EnemyManager EnemyManager;
        protected WanderingManager WanderingManager = null;
        protected Vector3 TargetWanderingPoint;
        protected EnemyState State;
        protected bool IsAttacking = false;


        private GameObject _player; // プレイヤー
        private UnitStats _playerStats; // プレイヤーのCombatAction

        // 徘徊する
        private Wandering _wandering;
        private float _wanderingStateDuration;

        private Vector3 _targetPos;


        protected new void Start()
        {
            base.Start();

            TryGetComponent(out MyAnim); // 自身のアニメーターを取得
            TryGetComponent(out MyNavi); // 自身のナビメッシュを取得
            TryGetComponent(out MyStats);  // 自身のCombatActionを取得
            GameObject.FindWithTag("EnemyManager").TryGetComponent(out EnemyManager);
            _player = GameObject.FindWithTag("Player"); // プレイヤーを取得

            Assert.IsNotNull(EnemyManager, $"{this}の_enemyManagerがNullです。EnemyManager配下に敵オブジェクトを生成するようにしてください。");
            Assert.IsNotNull(_player, $"{this}の_playerがNullです");
            
            _player?.TryGetComponent(out _playerStats);// プレイヤーからCombatActionを取得
            State = EnemyState.Idle;
            _wanderingStateDuration = RandomSetDuration(_waitIdleMin, _waitIdleMax);

            if (WanderingManager != null)
            {
                _wandering = WanderingManager.AssignNotUseWandering(null);
            }
        }
        private void Update()
        {
            OnUpdate();
            ActionEnemy();
        }
        protected virtual void OnUpdate(){}

        /// <summary>
        /// 敵の行動
        /// </summary>
        protected void ActionEnemy()
        {
            if (!_player || MyStats.IsDead) return;// プレイヤー未発見時
                                                    // プレイヤー死亡時の対応
            if (_playerStats.IsDead)
            {
                SomeAnimationsStopped();
                return;
            }

            // プレイヤーとの距離を求める
            float distance = Vector3.Distance(transform.position, _player.transform.position);

            // プレイヤーの方向を向く
            if (distance <= SearchRange)// 探索範囲内なら
            {
                if (State != EnemyState.Attack)// 攻撃State以外なら
                {
                    // 攻撃前のPlayerの位置を保存
                    _targetPos = _player.transform.position;
                }// 攻撃State
                else if (!IsAttacking)// 攻撃してないなら
                {
                    _targetPos = _player.transform.position;
                }
                else // 攻撃中なら
                {
                    //Debug.Log($"{this.gameObject.name}攻撃中");
                }
                
                // 敵からPlayerのベクトル
                var moveVec = _targetPos - transform.position;
                moveVec.Normalize();

                // 回転実行
                TrunDirection(moveVec);
            }

            UpdateSwitchState(distance);

            switch (State)
            {
                case EnemyState.Idle:// 探索範囲外なら
                     // 移動停止
                    MyNavi.enabled = false; // ナビメッシュ切る
                    MyAnim.SetFloat("Speed", 0); //移動はしない
                    MyAnim.SetBool("Attack", false); //攻撃停止
                    IsAttacking = false;

                    break;
                    
                case EnemyState.Wandering:// 探索範囲外の時　一定時間（ランダム）経ったら　徘徊
                    //徘徊地点まで移動
                    MyNavi.enabled = true; 
                    MyNavi.destination = TargetWanderingPoint; // ターゲットを指示
                    MyAnim.SetFloat("Speed", MyNavi.velocity.magnitude); //移動モーション
                    
                    //攻撃停止
                    MyAnim.SetBool("Attack", false); 
                    IsAttacking = false;
                    break;

                case EnemyState.Chase: // 探索範囲内なら
                    // プレイヤーを追従
                    MyNavi.enabled = true; // ナビメッシュをオン
                    MyNavi.destination = _player.transform.position; // ターゲットを指示
                    MyAnim.SetFloat("Speed", MyNavi.velocity.magnitude); //移動モーション

                    //攻撃停止
                    MyAnim.SetBool("Attack", false);
                    IsAttacking = false;
                    break;

                case EnemyState.Attack:// 攻撃中なら
                    // 立ち止まって攻撃
                    MyNavi.enabled = false; // ナビメッシュ切る
                    MyAnim.SetFloat("Speed", 0); //移動はしない
                    MyAnim.SetBool("Attack", true); //攻撃開始
                    break;

                default:

                    break;
            }
        }

        /// <summary>
        /// 状態の更新をする
        /// </summary>
        /// <param name="distance"></param>
        private void UpdateSwitchState(float distance)
        {
            if(distance < SearchRange) _waitIdleStateTimer = 0f;

            if (State == EnemyState.Wandering || State == EnemyState.Separation)//NOTE: 一番前にしないと攻撃範囲内にいるのに待機状態とかになるかも？
            {
                // 目標地点　0.1m以内に着たら
                if (HasDestinationArrived(TargetWanderingPoint, _arrivalThreshold))
                {
                    State = EnemyState.Idle;
                }
            }// 探索範囲外なら
            else if (distance > SearchRange && State != EnemyState.Separation)
            {
                State = EnemyState.Idle;
                // タイマーを進める
                _waitIdleStateTimer += Time.deltaTime;

                // 探索範囲外の時、一定時間（ランダム）経ったら徘徊状態へ　WanderingManagerがない場合は徘徊しない
                if (_waitIdleStateTimer >= _wanderingStateDuration && WanderingManager != null) 
                {
                    // 徘徊ポイントを再設定
                    _wandering = WanderingManager.AssignNotUseWandering(_wandering.Transform);
                    TargetWanderingPoint = WanderingManager.CircleRandomPoint.GetRandomPointInCircle(_wandering.Transform.position, this.transform);
                    // 新しい待機時間を設定
                    _wanderingStateDuration = RandomSetDuration(_waitIdleMin, _waitIdleMax);

                    // 状態を変更
                    State = EnemyState.Wandering;
                    // タイマーリセット
                    _waitIdleStateTimer = 0f;
                }
            }

            SwitchStateNoticePlayer(distance);
        }
        /// <summary>
        /// Playerに気が付いている状態変化
        /// </summary>
        /// <param name="distance">Playerとの距離</param>
        protected virtual void SwitchStateNoticePlayer(float distance)
        {
            // 攻撃範囲内
            if (distance <= FireDistance)
            {
                State = EnemyState.Attack;
            }// 攻撃範囲外
            else if (distance <= SearchRange)// プレイヤーとの距離が索敵範囲内なら
            {
                if (!IsAttacking)
                {
                    State = EnemyState.Chase;// 攻撃中でないなら追いかける
                }
            }
        }
        /// <summary>
        /// 間隔をランダムに返す
        /// </summary>
        /// <param name="min">最小</param>
        /// <param name="max">最大</param>
        /// <returns></returns>
        private float RandomSetDuration(float min, float max) => UnityEngine.Random.Range(min, max);
        protected void TrunDirection(Vector3 direction)
        {
            // 回転実行
            transform.rotation = Quaternion.Slerp
                (
                       transform.rotation,
                       Quaternion.LookRotation(direction),
                       Time.deltaTime * TurningSpeed // 振り向き速度
                );
        }

        /// <summary>
        /// 目的地に到着したか
        /// </summary>
        /// <param name="destination">目的地</param>
        /// <param name="distance">どこまで近づけばいいか</param>
        /// <returns></returns>
        private bool HasDestinationArrived(Vector3 destination, float distance) => (destination - transform.position).sqrMagnitude <= distance*distance;

        /// <summary>
        /// プレイヤーとの距離が指定した距離内かどうか
        /// </summary>
        /// <param name="specifiedDistance">指定距離</param>
        /// <returns>範囲内かどうか</returns>
        protected bool IsPlayerWithinRange(float specifiedDistance)
        {
            float distance = Vector3.Distance(transform.position, _player.transform.position);

            if (distance < specifiedDistance) return true;
            else return false;
        }

        /// <summary>
        /// アニメーションを止める (移動と攻撃)
        /// </summary>
        protected virtual void SomeAnimationsStopped()
        {
            MyAnim.SetFloat("Speed", 0); // 移動はしない
            MyAnim.SetBool("Attack", false); // 攻撃停止
            MyNavi.enabled = false; // ナビメッシュ切る
        }

        /// <summary>
        /// ダメージを受けた時の処理
        /// </summary>
        /// <param name="hitStopTime"></param>
        public void OnDamage(float hitStopTime)
        {
            // ヒットストップアニメーションを指定秒数止める
            MyAnim.speed = 0;
            var sequence = DOTween.Sequence();
            sequence.SetDelay(hitStopTime);
            sequence.AppendCallback(() => MyAnim.speed = 1);

            // ダメージを視覚化
            VisualizationDamege();
        }

        /// <summary>
        /// ダメージ視覚処理
        /// </summary>
        /// <returns></returns>
        public override void VisualizationDamege()
        {
            GameObject Fx = Instantiate(_patDamage); // ダメージエフェクトを生成
            Fx.transform.position = transform.position + DamagePos; // 位置を補正
            Destroy(Fx, 1.0f); // エフェクトを1.0秒後に破棄
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        /// <returns></returns>
        public override async UniTaskVoid OnDeathAsync()
        {
            Debug.Log($"{gameObject.name}が死亡した");
            // 当たり判定を消す
            GetComponent<Collider>().enabled = false; // コライダーを切る　NOTE:↓で攻撃判定を消していても残っていたので消すようにした
            _weaponActions[0].WeaponActivate(false);//NOTE: 攻撃中に死ぬと攻撃当たり判定が残ったまま死んでダメージを受けるので消しておる

            // 死亡時演出
            SomeAnimationsStopped();
            MyAnim.SetTrigger("Death"); // 死亡モーション発動

            // 死亡処理
            EnemyManager.RemoveEnemy(this.gameObject);
        }

        /// <summary>
        /// 死亡演出
        /// </summary>
        protected virtual void DeathPerformance()
        {
            SmallingWhileRotating();
        }

        /// <summary>
        /// 回転しながら小さくなる
        /// </summary>
        protected void SmallingWhileRotating()
        {
            // だんだん小さくなる
            transform.DOScale(Vector3.zero, DeathTime)
                .SetEase(Ease.OutCirc)
                .OnUpdate(() =>
                {
                    transform.rotation *= Quaternion.Euler(Vector3.up * Time.deltaTime * 360);
                })
                .OnComplete(() => Destroy(gameObject))// 終わってから死亡
                .Play();
        }

        #region AnimationEvent

        /// <summary>
        /// 攻撃有効化
        /// </summary>
        public override void Attack0Start()
        {
            _weaponActions[0].WeaponActivate(true);
        }
        /// <summary>
        /// 攻撃無効化
        /// </summary>
        public override void Attack0Finish()
        {
            _weaponActions[0].WeaponActivate(false);
        }

        /// <summary>
        /// 攻撃アニメーション開始
        /// </summary>
        public void AttackAnimationStart()
        {
            IsAttacking = true;
        }
        /// <summary>
        /// 攻撃アニメーション終了
        /// </summary>
        public virtual void AttackAnimationEnd()
        {
            IsAttacking = false;
        }

        #endregion
    }
}