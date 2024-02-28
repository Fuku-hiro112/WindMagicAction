using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unit
{
    [RequireComponent(typeof(UnitStats))]
    public class UnitBase : MonoBehaviour, IAnimationAttackable// AnimationEvent関数を発生させるには継承元に関数が必要っぽいので必須にした
    {
        [SerializeField] protected GameObject[] _weapons = new GameObject[0]; // 武器オブジェクト

        protected WeaponAction[] _weaponActions;
        protected CancellationToken token;

        public virtual void OnDamage() { }
        public virtual async UniTaskVoid OnDeath() { }

        protected void Start()// 親のStartは自動的にOverrideされるため子からbase.Startで呼び出しが必要
        {
            token = this.GetCancellationTokenOnDestroy();
            GetWeapon();
        }
        /// <summary>
        /// 武器の取得
        /// </summary>
        private void GetWeapon()
        {
            _weaponActions = new WeaponAction[_weapons.Length];

            for (int i = 0; i < _weapons.Length; i++)
            {
                _weapons[i].TryGetComponent(out _weaponActions[i]);
                Assert.IsNotNull(_weaponActions[i], $"{this.gameObject.name}_weaponActions[{i}]がnullです");
            }
        }

        public virtual void AttackStart() { }
        public virtual void AttackFinish() { }
    }

    //HACK: AnimationEvent関数を発生させるには継承元に関数が必要っぽい
    public interface IAnimationEventer { }
    public interface IAnimationAttackable : IAnimationEventer
    {
        /// <summary>
        /// 攻撃有効化
        /// </summary>
        void AttackStart();
        /// <summary>
        /// 攻撃無効化
        /// </summary>
        void AttackFinish();
    }
}