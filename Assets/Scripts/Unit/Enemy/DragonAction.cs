using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;

namespace Unit
{
    public class DragonAction : EnemyActionBase
    {
        [SerializeField] Material _material;
        [SerializeField] float _canvasDisplayDistance = 30;

        private GameObject _canvas;

        private void Reset()
        {
            _fireDistance = 5;
            _searchRange = 10;
            _deathTime = 3;
            _damagePos = new Vector3(0, 1.5f, 0);
        }
        protected new void Start()
        {
            base.Start();
            _canvas = GetComponent<UnitStats>().MyCanvas;
        }
        private void Update()
        {
            ActionEnemy();
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
        /// 死亡処理
        /// </summary>
        public override async UniTaskVoid OnDeath()
        {
            Debug.Log("ドラゴン死亡");
            base.OnDeath().Forget();
            DeathPerformance();// 演出 回転しながら小さくなる
        }
        /// <summary>
        /// 死亡演出
        /// </summary>
        /// <param name="obj"></param>
        protected override void DeathPerformance()
        {
            SmallingWhileRotating();
            Assert.IsNotNull(_material, $"_materialがnullです");
            //_material.DOFade(0, _deathTime);//HACK: 出来ない
        }

        #region AnimationEvent

        /// <summary>
        /// 攻撃有効化
        /// </summary>
        public override void AttackStart()
        {
            _weaponActions[0].WeaponActivate(true);
            IsAttacking = true;
        }
        /// <summary>
        /// 攻撃無効化
        /// </summary>
        public override void AttackFinish()
        {
            _weaponActions[0].WeaponActivate(false);
            IsAttacking = false;
        }

        #endregion
    }
}
