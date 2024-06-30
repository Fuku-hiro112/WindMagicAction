using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;
using UniRx.Triggers;
using UniRx;
using System;

namespace Unit
{
    enum DragonAttackType
    {
        Weak = 0,
        Jump = 1,
        Breath = 2,
        None,
    }

    public class DragonController : EnemyControllerBase, IMissingAttackCountReseter
    {
        [Header("速度設定")]
        [SerializeField] private float _defaultSpeed = 3.5f;// 移動速度
        [SerializeField] private float _runSpeed = 7.0f;    // 走る速度
        [Space(20)]
        [Header("時間設定")]
        [SerializeField] private float _wakeUpWateTime = 1.0f;// 起きるまでの待ち時間
        [Space(20)]
        [Header("距離設定")]
        [SerializeField] private float _fireMidDistance;    // 中距離攻撃の距離
        [SerializeField] private float _canvasDisplayDistance = 30;// キャンバス表示距離
        [Space(20)]
        [SerializeField] private Vector3 _backStepPower;// バックステップの力

        [SerializeField] private Material _material;

        [SerializeField] private Collider[] _colliders;
        private DragonAttackType _attackType = DragonAttackType.None;
        private Rigidbody _myRigidbody;
        private GameObject _canvas;
        private int _missingAttackCount = 0;// 攻撃弱が当たらなかった回数
        private int _minCount = 1;// 攻撃弱が当たらなかった回数の最大値
        private int _maxCount = 3;
        private int _missTolerance;
        private bool _isSleeping = true;

        private void Reset()
        {
            FireDistance = 5;
            _fireMidDistance = 15;
            SearchRange = 30;
            DeathTime = 3;
            DamagePos = new Vector3(0, 1.5f, 0);
            _backStepPower = new Vector3(0, 5, -5);
        }
        protected new void Start()
        {
            //WARNING: このコンポーネントをbase.Startより先に取得しておかないと、WanderingManagerが認識されない
            GameObject.FindWithTag("EnemyManager").TryGetComponent(out WanderingManager);
            Assert.IsNotNull(WanderingManager, $"{this}のWanderingManagerがNullです");
            TryGetComponent(out _myRigidbody);
            Assert.IsNotNull(_myRigidbody, $"{this}のRigidbodyがNullです");

            base.Start();

            _canvas = MyStats.MyCanvas;
            _isSleeping = true;
            MyAnim.SetTrigger("Sleep");
            MissAttackToleranceUpdate();

            // 寝てる状態でHPがMaxじゃなくなったら　起きる
            this.UpdateAsObservable()
                .First(_ => _isSleeping && MyStats.Health.Value != MyStats.MaxHealth)
                .Subscribe(_ => OnWakeUp().Forget());
            // 死亡したときアニメーションを再生
            this.UpdateAsObservable()
                .First(_ => MyStats.IsDead)
                .Subscribe(_ => MyAnim.SetTrigger("Death"));
            
        }
        private void Update()
        {
            if (MyStats.IsDead) return;

            // 寝ていないなら　行動する
            if (!_isSleeping) ActionEnemy();

            DisplayCanvas();
        }

        /// <summary>
        /// キャンバスの表示
        /// </summary>
        private void DisplayCanvas()
        {
            // プレイヤーが一定距離に来たら　キャンバスをON　
            if (IsPlayerWithinRange(_canvasDisplayDistance))
            {
                _canvas.SetActive(true);
            }
            else
            {
                _canvas.SetActive(false);
            }
        }

        /// <summary>
        /// 起きた時の処理
        /// </summary>
        /// <returns>UniTask</returns>
        private async UniTaskVoid OnWakeUp()
        {
            MyAnim.SetTrigger("WakeUp");

            await UniTask.Delay((int)(_wakeUpWateTime * 1000));//NOTE: ミリ秒なので1000倍して秒に変換している

            MyAnim.SetTrigger("Scream");
        }

        /// <summary>
        /// Playerに気が付いている状態変化
        /// </summary>
        /// <param name="distance">Playerとの距離</param>
        protected override void SwitchStateNoticePlayer(float distance)
        {
            if (IsAttacking) return;
            DragonAttackType currentType = _attackType;

            // 攻撃弱範囲内
            if (distance <= FireDistance)
            {
                // 攻撃弱が当たらなかった回数が3~5回以上なら
                if (_missingAttackCount >= _missTolerance && _attackType == DragonAttackType.None)
                {
                    //TODO: バックステップ　してAttackMid攻撃のどちらか
                    MyAnim.SetTrigger("BackStep");
                    MissAttackToleranceUpdate();
                    MissingAttackCountReset();
                }
                else
                {
                    State = EnemyState.Attack;

                    _attackType = DragonAttackType.Weak;
                    //StartAttackAnimation();
                }
            }//攻撃弱範囲内
            else if (distance <= _fireMidDistance)
            {
                State = EnemyState.Attack;
                OnAttackMid();

            }// 攻撃範囲外
            else if (distance <= SearchRange)// プレイヤーとの距離が索敵範囲内なら
            {
                State = EnemyState.Chase;// 攻撃中でないなら追いかける
            }

            // 攻撃範囲に入っている
            if (distance <= _fireMidDistance && currentType != _attackType)
            {
                // アニメーションスタート
                StartAttackAnimation();
            }
        }
        /// <summary>
        /// 中距離攻撃
        /// </summary>
        private void OnAttackMid()
        {
            bool halfPercent = UnityEngine.Random.Range(0, 2) == 0;

            // 体力が50％以下 かつ 50％の確率で
            if (MyStats.Health.Value <= MyStats.MaxHealth/2 && halfPercent)
            {
                // ブレス攻撃
                _attackType = DragonAttackType.Breath;
            }
            else
            {
                // ジャンプ攻撃
                _attackType = DragonAttackType.Jump;
            }

            /*
            #region ローカル関数
            /// <summary>
            /// ブレス攻撃
            /// </summary>
            void AttackBreath()
            {
                //TODO: 中身の実装 animationEvent
                _attackType = DragonAttackType.Breath;
            }
            /// <summary>
            /// ジャンプ攻撃
            /// </summary>
            void AttackJump()
            {
                //TODO: 中身の実装 animationEvent
                _attackType = DragonAttackType.Jump;
            }
            #endregion
            */
        }
        
        /// <summary>
        /// 指定されたタイプの攻撃アニメーションを開始
        /// </summary>
        private void StartAttackAnimation()
        {
            // WARNING: 一度だけ実行したい
            MyAnim.SetInteger("AttackType", (int)_attackType);

            if(_attackType == DragonAttackType.Weak)
                _missingAttackCount++;// 攻撃が外れた回数を増やす NOTE: 攻撃した瞬間にミスすると確定させて、当たった場合にミスした回数をリセットさせている
        }

        /// <summary>
        /// 攻撃ミス許容回数更新
        /// </summary>
        /// <returns>回数</returns>
        private void MissAttackToleranceUpdate() => _missTolerance = UnityEngine.Random.Range(_minCount, _maxCount);
        /// <summary>
        /// 攻撃ミス許容回数リセット
        /// </summary>
        public void MissingAttackCountReset() => _missingAttackCount = 0;


        #region AnimationEvent

        /// <summary>
        /// 攻撃飛びつき有効化　
        /// </summary>
        public void AttackJumpStart()
        {
            _weaponActions[1].WeaponActivate(true);
        }
        /// <summary>
        /// 攻撃飛びつき無効化　
        /// </summary>
        public void AttackJumpFinish()
        {
            _weaponActions[1].WeaponActivate(false);
        }

        //TODO: パーティクルの当たり判定をどうするかによる
        /// <summary>
        /// 攻撃ブレス有効化
        /// </summary>
        public void AttackBreathStart()
        {
            _weaponActions[2].WeaponActivate(true);
        }
        /// <summary>
        /// 攻撃ブレス無効化
        /// </summary>
        public void AttackBreathFinish()
        {
            _weaponActions[2].WeaponActivate(false);
        }

        /// <summary>
        /// 後ろ向きに飛ぶ　BackStepのAnimationEventで呼び出す
        /// </summary>
        public void JumpBack()
        {
            Debug.Log($"State:{State}");
            // 飛ぶ方向
            Vector3 jumpDirection = transform.forward + transform.up;
            // ベクトル
            Vector3 vector = Vector3.Scale(jumpDirection ,_backStepPower);
            // ベクトル分力を加える
            _myRigidbody.AddForce(vector, ForceMode.Impulse);
        }

        /// <summary>
        /// 起きた状態に切り替える　Screamの最後で呼び出す
        /// </summary>
        /// <returns></returns>
        public async UniTask SwithWakeUp()//TODO: AnimationEventのScreamの最後で呼び出す
        {
            await UniTask.Delay((int)(_wakeUpWateTime * 1000));//NOTE: ミリ秒なので1000倍して秒に変換している
            _isSleeping = false;
        }

        /// <summary>
        /// 死亡処理 DeathのAnimationEventの最後で呼び出す
        /// </summary>
        public override async UniTaskVoid OnDeathAsync()
        {
            Debug.Log("ドラゴン死亡");

            // 当たり判定を消す
            foreach (Collider collider in _colliders)
            {
                // コライダーを切る
                if (collider != null)
                collider.enabled = false;
            }
            foreach (WeaponAction weaponAction in _weaponActions)
            {
                //NOTE: 攻撃中に死ぬと攻撃当たり判定が残ったまま死んでダメージを受けるので消しておる
                weaponAction?.WeaponActivate(false);
            }

            // 死亡時演出
            SomeAnimationsStopped();

            // 死亡処理
            EnemyManager.RemoveEnemy(this.gameObject);
        }

        public override void AttackAnimationEnd()
        {
            _attackType = DragonAttackType.None;
            IsAttacking = false;
        }
        #endregion
    }
}
public interface IMissingAttackCountReseter
{
    void MissingAttackCountReset();
}