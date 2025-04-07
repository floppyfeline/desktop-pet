using System;
using System.Data.Common;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

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
        if (_entity.Rb.linearVelocityY <= 0.1f) _entity.Rb.AddRelativeForce((Vector2.up), ForceMode2D.Impulse);
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
public class Flying : IMovement
{
    private PKMNBrain _entity;
    private bool _isBusy = false;
    private float _forceMultiplier = .3f;
    private float _nextBounceVelocity = 0.01f; // Minimum velocity to trigger a bounce

    private float _highestY = -4.5f; // under screen height  * _highestY the creature will flap
    private float _drag = 0.99f;
    private float _graceTime = -1f;

    private float _timeSinceGrounded = -1f;
    public Flying(PKMNBrain brain)
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
        //_entity.Rb.linearVelocity = new Vector2(_entity.Rb.linearVelocityX, 0);
        Vector2 direction = (_entity.FacingDirection == Direction.Left ? Vector2.left : Vector2.right) + Vector2.up;
        _entity.Rb.AddRelativeForce((Vector2.up + direction) * _entity.Speed * _forceMultiplier, ForceMode2D.Impulse);
        _graceTime = 0.4f;
        _nextBounceVelocity = UnityEngine.Random.Range(-0.5f, 0.06f); // Randomize the bounce velocity
    }
    private void Bounce()
    {
        if (_entity.Rb.linearVelocityY <= 0.1f) _entity.Rb.AddRelativeForce((Vector2.up), ForceMode2D.Impulse);
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
        if (Mathf.Abs(_entity.Rb.linearVelocity.x) > _entity.Speed / 2 )
        {
            _entity.Rb.linearVelocity = new Vector2(Mathf.Sign(_entity.Rb.linearVelocityX) * _entity.Speed / 2, _entity.Rb.linearVelocity.y);
        }
        if(_entity.transform.position.y > _highestY)
        {
            if(_entity.Rb.linearVelocityY > 0) _entity.Rb.linearVelocityY *= _drag;
        }
        else if (_entity.Rb.linearVelocity.y < _nextBounceVelocity)
        {
            // Reset _isBusy to allow the next move cycle
            _isBusy = false;
            _entity.OnMoveCallback?.Invoke();
            return;
        }
        if (Mathf.Approximately(_entity.Rb.linearVelocityY, 0))
        {
            _entity.Rb.linearVelocity *= _drag;
            _timeSinceGrounded += Time.deltaTime;
            if (_timeSinceGrounded >= 0.7f)
            {
                // Reset _isBusy to allow the next move cycle
                _isBusy = false;
                _entity.OnMoveCallback?.Invoke();
                return;
            }

        }
        else _timeSinceGrounded = -1f;
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