using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unit
{
    [RequireComponent(typeof(UnitStats))]
    public class UnitBase : MonoBehaviour, IAnimationAttackable// AnimationEvent�֐��𔭐�������ɂ͌p�����Ɋ֐����K�v���ۂ��̂ŕK�{�ɂ���
    {
        [SerializeField] protected GameObject[] _weapons = new GameObject[0]; // ����I�u�W�F�N�g

        protected WeaponAction[] _weaponActions;
        protected CancellationToken token;

        public virtual void OnDamage() { }
        public virtual async UniTaskVoid OnDeath() { }

        protected void Start()// �e��Start�͎����I��Override����邽�ߎq����base.Start�ŌĂяo�����K�v
        {
            token = this.GetCancellationTokenOnDestroy();
            GetWeapon();
        }
        /// <summary>
        /// ����̎擾
        /// </summary>
        private void GetWeapon()
        {
            _weaponActions = new WeaponAction[_weapons.Length];

            for (int i = 0; i < _weapons.Length; i++)
            {
                _weapons[i].TryGetComponent(out _weaponActions[i]);
                Assert.IsNotNull(_weaponActions[i], $"{this.gameObject.name}_weaponActions[{i}]��null�ł�");
            }
        }

        public virtual void AttackStart() { }
        public virtual void AttackFinish() { }
    }

    //HACK: AnimationEvent�֐��𔭐�������ɂ͌p�����Ɋ֐����K�v���ۂ�
    public interface IAnimationEventer { }
    public interface IAnimationAttackable : IAnimationEventer
    {
        /// <summary>
        /// �U���L����
        /// </summary>
        void AttackStart();
        /// <summary>
        /// �U��������
        /// </summary>
        void AttackFinish();
    }
}