using System;
using System.Data.Common;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public interface IMovement
{
    void UpdateMovement();
}
public class Walking : IMovement
{
    private PKMNBrain _entity;
    private bool _isBusy = false;
    private float _forceMultiplier = .5f;

    private float _graceTime = -1f;
    public Walking(PKMNBrain brain)
    {
        _entity = brain;
        _isBusy = false;
        brain.OnMove += OnMove;
        brain.OnWallHit += OnWallCollision;
        brain.OnInteract += Bounce;
        brain.OnClick += GetDragged;
    }
    private void OnWallCollision()
    {
        _entity.Rb.linearVelocity = new Vector2(-_entity.Rb.linearVelocity.x, _entity.Rb.linearVelocity.y);
        _entity.FlipSprite();
    }
    public void OnMove()
    {
        _isBusy = true;
        Vector2 direction = _entity.FacingDirection == Direction.Left ? Vector2.left : Vector2.right;
        _entity.Rb.AddRelativeForce((Vector2.up + direction) * _entity.Speed * _forceMultiplier, ForceMode2D.Impulse);
        _graceTime = 0.4f;
    }
    private void Bounce()
    {
        if(_entity.Rb.linearVelocityY <= 0.1f)_entity.Rb.AddRelativeForce((Vector2.up), ForceMode2D.Impulse);
    }

    public void UpdateMovement()
    {
        if (_graceTime >= 0)
        {
            _graceTime -= Time.deltaTime;
            return;
        }
        if (!_isBusy)
        {
            return;
        };


        // Less strict velocity check (avoids rounding issues)
        if (_entity.Rb.linearVelocity.magnitude < 0.1f)
        {
            // Reset _isBusy to allow the next move cycle
            _isBusy = false;
            _entity.OnMoveCallback?.Invoke();

        }
    }

    public void GetDragged()
    {
        _entity.Movement = new Dragged(this, _entity);
    }
}
public class Dragged : IMovement
{
    IMovement _prev;
    PKMNBrain _entity;
    private Vector2 _offset = Vector2.zero;
    public Dragged(IMovement prev, PKMNBrain brain)
    {
        _prev = prev;
        _entity = brain;

        // Get world position of entity and convert it to screen space
        Vector2 entityScreenPos = Camera.main.WorldToScreenPoint(_entity.transform.position);

        // Compute offset correctly
        Vector2 mousePos = GameManager.GetMousePosition();
        _offset = mousePos - entityScreenPos;
        if (mousePos == Vector2.zero) _offset = Vector2.zero; 
        Debug.Log($"Entity Screen Position: {entityScreenPos}, Mouse Position: {GameManager.MousePosition}, Offset: {_offset}");

        _entity.Rb.simulated = false;
        _entity.OnUnclick += Exit;
    }
    public void UpdateMovement()
    {
        Vector3 newPos = Camera.main.ScreenToWorldPoint(GameManager.MousePosition - _offset);
        newPos.z = _entity.transform.position.z; // Keep original Z position
        _entity.transform.position = newPos;
    }
    public void Exit()
    {
        _entity.Rb.simulated = true;
        _entity.Movement = _prev;
    }
}