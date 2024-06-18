using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unit;

public class WargAction : EnemyControllerBase
{
    private List<Transform> _nearbyEnemieList = new List<Transform>();
    [SerializeField] private float _targetPriority = 2;

    private void Reset()
    {
        FireDistance = 2;
        SearchRange = 10;
        DeathTime = 3;
        DamagePos = new Vector3(0, 1.5f, 0);
        _targetPriority = 2;
    }
    protected new void Start()
    {
        transform.root.TryGetComponent(out WanderingManager);
        Assert.IsNotNull(WanderingManager, $"{this}‚ÌWanderingManager‚ªNull‚Å‚·");
        base.Start();
    }

    protected override void OnUpdate() 
    {
        WanderingStateSwitch();
        UpdateSeparation();
    }

    /// <summary>
    /// œpœjƒXƒe[ƒg“ü‚ê‘Ö‚¦
    /// </summary>
    private void WanderingStateSwitch()
    {
        if (State == EnemyState.Wandering || State == EnemyState.Separation)
        {
            _nearbyEnemieList.Clear();

            // ‹ß‚­‚É‚¢‚é“G‚ğŠi”[
            _nearbyEnemieList = EnemyManager.GetAllNearbyEnemies(this.transform, 20);

            // ‹ß‚­‚É“G‚ª‚¢‚é‚©
            if (_nearbyEnemieList.Count > 0)
            {
                State = EnemyState.Separation;
            }
            else
            {
                State = EnemyState.Wandering;
            }
        }
    }
    /// <summary>
    /// —£‚ê‚éó‘Ô‚Ìˆ—
    /// </summary>
    private void UpdateSeparation()
    {
        if (State == EnemyState.Separation)
        {
            Vector3 force = new();
            // “G‚Ì•ûŒü‚Æ‚Í‹t‚ÌƒxƒNƒgƒ‹‚ğ•Û‘¶
            foreach (Transform enemyTransform in _nearbyEnemieList)
            {
                force += (transform.position - enemyTransform.position).normalized;
            }

            MyAnim.SetFloat("Speed", MyNavi.velocity.magnitude); //ˆÚ“®ƒ‚[ƒVƒ‡ƒ“ ƒIƒ“
            MyNavi.enabled = true;
            // –Ú•W’n“_‚Ì•ûŒü
            Vector3 directionOfWanderingPoint = (TargetWanderingPoint - transform.position).normalized;
            MyNavi.destination = // ˆÚ“®‚·‚é–Ú“IêŠ
                transform.position + force.normalized + (directionOfWanderingPoint * _targetPriority); 
        }
    }

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
    }

#region AnimationEvent

    /// <summary>
    /// UŒ‚—LŒø‰»
    /// </summary>
    public override void AttackStart()
    {
        _weaponActions[0].WeaponActivate(true);
    }
    /// <summary>
    /// UŒ‚–³Œø‰»
    /// </summary>
    public override void AttackFinish()
    {
        _weaponActions[0].WeaponActivate(false);
    }

#endregion
}
