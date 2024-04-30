public class Flame : MagicBase //HACK: å¿ÇËÇ»Ç≠éGÇ»åpè≥ â¥Ç≈Ç»Ç´Ç·å©ì¶ÇµÇøÇ·Ç§ÇÀ
{
    protected override void OnHit() 
    {
        _effectShoot.HitFlameEffect(gameObject.transform.position).Forget();
    }
}
