using System;
using UnityEngine;

public class MagicBase : MonoBehaviour
{
    protected MagicShoot _effectShoot;//WARNIG: 循環参照している
    protected IDisposable _disposable;

    public void OnStart(MagicShoot effectShoot, IDisposable subscription)
    {
        _effectShoot = effectShoot;
        _disposable = subscription;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Player以外に当たったなら
        if (!other.gameObject.CompareTag("Player"))
        {
            OnHit();
            Destroy(gameObject);
            _disposable.Dispose();
        }
    }

    /// <summary>
    /// ヒット時処理
    /// </summary>
    protected virtual void OnHit(){ }
}
