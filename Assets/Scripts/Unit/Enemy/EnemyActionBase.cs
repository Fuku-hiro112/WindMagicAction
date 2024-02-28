using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

namespace Unit
{
    public enum EnemyState
    {
        Idle,     // �ҋ@
        Wandering,// �p�j
        Chase,    // �G��ǂ�����
        Attacking,// �U��
    }

    [RequireComponent(typeof(Animator), typeof(NavMeshAgent))]// Animator��NavMesh��K�{��
    public class EnemyActionBase : UnitBase
    {
        [Header("�͈�")]
        [SerializeField, Tooltip("�U���J�n����")] protected float _fireDistance = 1.0f;
        [SerializeField, Tooltip("���G�͈�")] protected float _searchRange = 5.0f;
        [Space(20)]
        [SerializeField] protected float _deathTime = 3.0f; // ���S��ɏ�����܂ł̎���
        [SerializeField] protected int _turningSpeed = 5;
        [SerializeField] private GameObject _patDamage; // �_���[�W�G�t�F�N�g
        [SerializeField] protected Vector3 _damagePos = new Vector3(0, 1.5f, 0); // �_���[�W�G�t�F�N�g�̈ʒu
        [Space(20)]
        [SerializeField] private float _WaitIdleMin;
        [SerializeField] private float _WaitIdleMax;
        private float WaitIdle;

        protected NavMeshAgent _myNavi; // ���g�̃i�r���b�V��
        protected Animator _myAnim; // ���g�̃A�j���[�^�[
        protected TestEnemyManager _enemyManager;
        private UnitStats _myStats;   // ���g��CombatAction
        private GameObject _player; // �v���C���[
        private UnitStats _playerStats; // �v���C���[��CombatAction

        protected WanderingManager WanderingManager;
        // �p�j����
        private Wandering _wandering;

        protected EnemyState EnemyState;

        protected bool IsAttacking = false;
        private Vector3 _targetPos;

        protected new void Start()
        {
            base.Start();
            TryGetComponent(out _myAnim); // ���g�̃A�j���[�^�[���擾
            TryGetComponent(out _myNavi); // ���g�̃i�r���b�V�����擾
            TryGetComponent(out _myStats);  // ���g��CombatAction���擾
            transform.parent.TryGetComponent(out _enemyManager);
            _player = GameObject.FindGameObjectWithTag("Player"); // �v���C���[���擾

            Assert.IsNotNull(_enemyManager, $"{this}��_enemyManager��Null�ł��BEnemyManager�z���ɓG�I�u�W�F�N�g�𐶐�����悤�ɂ��Ă��������B");
            Assert.IsNotNull(_player, $"{this}��_player��Null�ł�");
            _player?.TryGetComponent(out _playerStats);// �v���C���[����CombatAction���擾
            EnemyState = EnemyState.Idle;

            if (WanderingManager != null)
            {
                _wandering = WanderingManager.AssignNotUseWandering();
            }
        }
        private void Update()
        {
            ActionEnemy();
        }
        protected void ActionEnemy()
        {
            if (!_player || _myStats.IsDead) return;// �v���C���[��������
                                                    // �v���C���[���S���̑Ή�
            if (_playerStats.IsDead)
            {
                SomeAnimationsStopped();
                return;
            }

            // �v���C���[�Ƃ̋��������߂�
            float distance = Vector3.Distance(transform.position, _player.transform.position);

            // �v���C���[�̕���������
            if (distance <= _searchRange)// �T���͈͓��Ȃ�
            {
                if (!IsAttacking/**/)// �U���O��Player�̈ʒu������
                {
                    _targetPos = _player.transform.position;
                }
                // �G����Player�̃x�N�g��
                var moveVec = _targetPos - transform.position;
                moveVec.Normalize();

                // ��]���s
                transform.rotation = Quaternion.Slerp(
                           transform.rotation,
                           Quaternion.LookRotation(moveVec),
                           Time.deltaTime * _turningSpeed // �U��������x
                );
            }

            if (distance > _searchRange)// �T���͈͊O�Ȃ�
            {
                // �ړ���~
                _myNavi.enabled = false; // �i�r���b�V���؂�
                _myAnim.SetFloat("Speed", 0); //�ړ��͂��Ȃ�
                _myAnim.SetBool("Attack", false); //�U����~
                IsAttacking = false;
            }// �T���͈͓��Ȃ�
            else if (distance <= _fireDistance)// �v���C���[�Ƃ̋�����_fireDistance�ȉ��A
            {
                // �����~�܂��čU��
                _myNavi.enabled = false; // �i�r���b�V���؂�
                _myAnim.SetFloat("Speed", 0); //�ړ��͂��Ȃ�
                _myAnim.SetBool("Attack", true); //�U���J�n
                IsAttacking = true;
            }
            else if (distance > _fireDistance && !IsAttacking)// �v���C���[�Ƃ̋�����_fireDistance�`_searchRange�Ȃ�
            {
                // �v���C���[��Ǐ]
                _myNavi.enabled = true; // �i�r���b�V�����I��
                _myNavi.destination = _player.transform.position; // �^�[�Q�b�g���w��
                _myAnim.SetFloat("Speed", _myNavi.velocity.magnitude); //�ړ����[�V����
                _myAnim.SetBool("Attack", false); //�U����~
            }
            else if (distance > _fireDistance)
            {
                IsAttacking = false;
            }

            switch (EnemyState)
            {
                case EnemyState.Idle:// �T���͈͊O�Ȃ�
                     // �ړ���~
                     /*
                    _myNavi.enabled = false; // �i�r���b�V���؂�
                    _myAnim.SetFloat("Speed", 0); //�ړ��͂��Ȃ�
                    _myAnim.SetBool("Attack", false); //�U����~
                    IsAttacking = false;*/
                    //TODO: ��莞�Ԍo������Wandering�Ɉȍ~ && WanderingManager != null
                    break;

                case EnemyState.Wandering:// �T���͈͊O�̎��@��莞�ԁi�����_���j�o������@�p�j
                    // 
                    break;

                case EnemyState.Chase: // �T���͈͓��Ȃ�
                    // 
                    break;

                case EnemyState.Attacking:// �U�����Ȃ�
                    // 
                    break;
            }
        }

        private void SwitchState()
        {

        }

        /// <summary>
        /// �v���C���[�Ƃ̋������w�肵�����������ǂ���
        /// </summary>
        /// <param name="specifiedDistance">�w�苗��</param>
        /// <returns>�͈͓����ǂ���</returns>
        protected bool IsPlayerWithinRange(float specifiedDistance)
        {
            float distance = Vector3.Distance(transform.position, _player.transform.position);

            if (distance < specifiedDistance) return true;
            else return false;
        }
        /// <summary>
        /// �A�j���[�V�������~�߂� (�ړ��ƍU��)
        /// </summary>
        protected virtual void SomeAnimationsStopped()
        {
            _myAnim.SetFloat("Speed", 0); // �ړ��͂��Ȃ�
            _myAnim.SetBool("Attack", false); // �U����~
        }
        /// <summary>
        /// �_���[�W���o����
        /// </summary>
        /// <returns></returns>
        public override void OnDamage()
        {
            GameObject Fx = Instantiate(_patDamage); // �_���[�W�G�t�F�N�g�𐶐�
            Fx.transform.position = transform.position + _damagePos; // �ʒu��␳
            Destroy(Fx, 1.0f); // �G�t�F�N�g��1.0�b��ɔj��
        }
        /// <summary>
        /// ���S����
        /// </summary>
        /// <returns></returns>
        public override async UniTaskVoid OnDeath()
        {
            Debug.Log($"{gameObject.name}�����S����");

            SomeAnimationsStopped();
            _myAnim.SetTrigger("Death"); // ���S���[�V��������
            _weaponActions[0].WeaponActivate(false);//NOTE: �U�����Ɏ��ʂƍU�������蔻�肪�c�����܂܎���Ń_���[�W���󂯂�̂ŏ����Ă���
            _myNavi.enabled = false; // �i�r���b�V���؂�
            gameObject.tag = "Untagged";
            _enemyManager.RemoveEnemy(this.gameObject);
        }
        /// <summary>
        /// ���S���o
        /// </summary>
        protected virtual void DeathPerformance()
        {
            SmallingWhileRotating();
        }
        /// <summary>
        /// ��]���Ȃ��珬�����Ȃ�
        /// </summary>
        protected void SmallingWhileRotating()
        {
            // ���񂾂񏬂����Ȃ�
            transform.DOScale(Vector3.zero, _deathTime)
                .SetEase(Ease.OutCirc)
                .OnUpdate(() =>
                {
                    transform.rotation *= Quaternion.Euler(Vector3.up * Time.deltaTime * 360);
                })
                .OnComplete(() => Destroy(gameObject))// �I����Ă��玀�S
                .Play();
        }

        #region AnimationEvent

        /// <summary>
        /// �U���L����
        /// </summary>
        public override void AttackStart()
        {
            _weaponActions[0].WeaponActivate(true);
            IsAttacking = true;
        }
        /// <summary>
        /// �U��������
        /// </summary>
        public override void AttackFinish()
        {
            _weaponActions[0].WeaponActivate(false);
            IsAttacking = false;
        }

        #endregion
    }
}