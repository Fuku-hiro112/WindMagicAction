using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class StartFade : MonoBehaviour
{
    [SerializeField] private int _duration = 1;
    void Start()
    {
        Image image = GetComponent<Image>();
        image.color = Color.black;
        image.DOColor(Color.clear, _duration);
    }
}
