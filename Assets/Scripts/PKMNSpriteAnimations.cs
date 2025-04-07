using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

public class PKMNSpriteAnimations : MonoBehaviour
{
    private Vector3 _defaultScale;
    private float _hoverScaleMultiplier = 1.1f, _hoverScaleDuration = 0.1f;
    private PKMNBrain _entity;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _defaultScale = transform.localScale;
        _entity = GetComponentInParent<PKMNBrain>();

        _entity.OnHover += OnHover;
        _entity.OnHoverExit += OnHoverExit;

        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void OnHover()
    {
      LeanTween.scale(gameObject, _defaultScale * _hoverScaleMultiplier, _hoverScaleDuration);
    }
    public void OnHoverExit()
    {
      LeanTween.scale(gameObject, _defaultScale, _hoverScaleDuration);
    }

    public void SetSprite(Sprite newSprite)
    {
        if(_spriteRenderer ==  null) _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = newSprite;
    }
}
