using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unit;

public class WargAction : EnemyActionBase
{
    private List<Transform> _nearbyEnemieList = new List<Transform>();

    private void Reset()
    {
        _fireDistance = 2;
        _searchRange = 10;
        _deathTime = 3;
        _damagePos = new Vector3(0, 1.5f, 0);
    }
    protected new void Start()
    {
        transform.root.TryGetComponent(out WanderingManager);
        Assert.IsNotNull(WanderingManager, $"{this}‚ÌWanderingManager‚ªNull‚Å‚·");
        base.Start();
    }

    protected override void OnUpdate() 
    {
        //WanderingStateSwitch();
        //UpdateSeparation();
    }

    #region ŠJ”­“r’†ˆ—
    /*
    private void WanderingStateSwitch()
    {
        if (State == EnemyState.Wandering)
        {
            // ‹ß‚­‚É‚¢‚é“G‚ğŠi”[
            _nearbyEnemieList = _enemyManager.GetAllNearbyEnemies(this.transform, 2);

            // ‹ß‚­‚É“G‚ª‚¢‚é‚©
            if (_nearbyEnemieList.Count > 0)
            {
                State = EnemyState.Separation;
            }
        }
        else
        {
            State = EnemyState.Separation;
        }
    }
    private void UpdateSeparation()
    {
        // —£‚ê‚éƒ‚[ƒh‚ğì‚é OK
        // “G‚Æˆê’è‹——£‚Ü‚Å‹ß‚Ã‚¢‚½‚ç—£‚ê‚éƒ‚[ƒh‚É
        if (State == EnemyState.Separation)
        {
            // –Ú“I’n‚Ì•ûŒü‚Æ“G‚ª‹‚éˆÊ’u‚Æ‹t‚Ì•ûŒü‚É‡¬
            // “G‚Æ‹t‚ÌˆÊ’u‚É‡¬‚·‚é‚©@‚Ç‚¿‚ç‚©
            _myNavi.enabled = true;

            Vector3 force = new();// ‚±‚ê‚Í‰½H
            // ‹tƒxƒNƒgƒ‹‚ğ•Û‘¶
            foreach (Transform enemyTransform in _nearbyEnemieList)
            {
                force += (transform.position - enemyTransform.position).normalized;
                         //vec / Mathf.Sqrt(vec.x * vec.x + vec.y * vec.y)
            }

            _myNavi.destination = default/*ˆÚ“®•ûŒü*//*; // ƒ^[ƒQƒbƒg‚ğw¦
        }

    }
    */
    #endregion

    /// <summary>
    /// €–Sˆ—
    /// </summary>
    public override async UniTaskVoid OnDeathAsync()
    {
        Debug.Log("˜T€–S");
        base.OnDeathAsync().Forget();
        DeathPerformance();// ‰‰o ‰ñ“]‚µ‚È‚ª‚ç¬‚³‚­‚È‚é
    }
    /// <summary>
    /// €–S‰‰o
    /// </summary>
    /// <param name="obj"></param>
    protected override void DeathPerformance()
    {
        SmallingWhileRotating();
        //Assert.IsNotNull(_material, $"_material‚ªnull‚Å‚·");
        //_material.DOFade(0, _deathTime);//HACK: o—ˆ‚È‚¢
    }

#region AnimationEvent

    /// <summary>
    /// UŒ‚—LŒø‰»
    /// </summary>
    public override void AttackStart()
    {
        _weaponActions[0].WeaponActivate(true);
        IsAttacking = true;
    }
    /// <summary>
    /// UŒ‚–³Œø‰»
    /// </summary>
    public override void AttackFinish()
    {
        _weaponActions[0].WeaponActivate(false);
        IsAttacking = false;
    }

#endregion
}
