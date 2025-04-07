using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public enum Direction
{
    Left,
    Right
}
public enum PKMNMode
{
    Stationary,
    Roaming
}
[Serializable]
public class PKMNBrain : MonoBehaviour
{
    public PKMNSpecies Species;
    [SerializeField]
    public bool IsShiny => _isShiny;
    public bool _isShiny = false; // True if the PKMN is shiny, false if it's not

    private bool _isWild;
    public bool IsWild => _isWild; // True if the PKMN is wild, false if it's a trainer's PKMN

    private string _name = "PKMN"; // Default name
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value) // Optional check to only update if the value changes
            {
                _name = value;
                OnNameUpdate?.Invoke(value); // Notify of the update
            }
        }
    }

    private SpriteRenderer _pkmnSprite;
    [HideInInspector]
    public Rigidbody2D Rb;

    public PKMNMode Mode = PKMNMode.Roaming;

    public float
        Speed = 3f, 
        Activity = 1f; // "laziness" stat, 1 - 10, 1 being the laziest, 10 being the most active

    private IBehaviour _currentBehavior;
    public IMovement Movement;
    public PKMNState CurrentState => _currentBehavior.State;

    public Direction FacingDirection = Direction.Left;

    public Action OnHover;
    public Action OnHoverExit;
    public Action OnClick;
    public Action OnUnclick;

    public Action OnMove;
    public Action OnMoveCallback;

    public Action OnWallHit;
    public Action OnInteract;

    public Action OnHappy;
    private ParticleSystem _particles;

    public Action<string> OnNameUpdate;

    public void Initialize(PKMNSpecies species, string name, bool isShiny, bool isWild, Sprite pkmnSprite, float speed, float activity, IMovement movement)
    {
        Species = species;
        Name = name;
        _isShiny = isShiny;
        _isWild = isWild;
        PKMNSpriteAnimations anims = GetComponentInChildren<PKMNSpriteAnimations>();
        anims.SetSprite(pkmnSprite);

        Speed = speed;
        Activity = activity;
        Movement = movement;
        Debug.Log($"Initialized {species.Name} {name} with speed {speed} and activity {activity}");
    }

    private void OnValidate()
    {
        Activity = Mathf.Clamp(Activity, 1, 10);
        Speed = Mathf.Clamp(Speed, 1, 5);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("SideWall"))
        {
            OnWallHit?.Invoke();
        }
    }
    private void OnMouseDown()
    {
        OnClick?.Invoke();
    }
    private void OnMouseUp()
    {
        OnUnclick?.Invoke();
    }
    private void OnMouseEnter()
    {
        OnHover.Invoke();
    }
    private void OnMouseExit()
    {
        OnHoverExit.Invoke();
    }
    private void Start()
    {
        Rb = GetComponent<Rigidbody2D>();
        _pkmnSprite = GetComponentInChildren<SpriteRenderer>();
        ChangeBehaviour(new Waiting());

        if (Movement == null) Movement = new Walking(this);

        _particles = GetComponentInChildren<ParticleSystem>();

        OnHappy += () =>
        {
            _particles.Stop();
            _particles.Play();
        };
    }
    private void Update()
    {
        Movement.UpdateMovement();
        _currentBehavior.UpdateState();
        _currentBehavior.UpdateBehaviour();
    }

    public void ChangeBehaviour(IBehaviour newBehaviour)
    {

        if (_currentBehavior != null)
        {
            if(newBehaviour.State == CurrentState) return;
            _currentBehavior.Exit();
        }

        _currentBehavior = newBehaviour;
        _currentBehavior.Enter(this);
    }

    public void FlipSprite()
    {
        _pkmnSprite.flipX = !_pkmnSprite.flipX;
        FacingDirection = FacingDirection == Direction.Left ? Direction.Right : Direction.Left;
    }
    public void ChangeMode(PKMNMode newMode)
    {
        Mode = newMode;
    }
}
