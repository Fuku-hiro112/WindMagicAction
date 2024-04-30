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
        Debug.Log(other.gameObject.name);
        // Player以外に当たったなら
        if (!other.gameObject.CompareTag("Player"))
        {
            Debug.Log(transform.position);

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
