using JetBrains.Annotations;
using UnityEngine;

public class PKMNSpriteAnimations : MonoBehaviour
{
    private Vector3 _defaultScale;
    private float _hoverScaleMultiplier = 1.1f, _hoverScaleDuration = 0.1f;
    private PKMNBrain _entity;

    private void Start()
    {
        _defaultScale = transform.localScale;
        _entity = GetComponentInParent<PKMNBrain>();

        _entity.OnHover += OnHover;
        _entity.OnHoverExit += OnHoverExit;
    }
    public void OnHover()
    {
      LeanTween.scale(gameObject, _defaultScale * _hoverScaleMultiplier, _hoverScaleDuration);
    }
    public void OnHoverExit()
    {
      LeanTween.scale(gameObject, _defaultScale, _hoverScaleDuration);
    }
}
