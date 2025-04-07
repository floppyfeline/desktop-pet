using Unity.VisualScripting;
using UnityEngine;


public enum PKMNState
{
    Roaming,
    Moving,
    Waiting,
    Following,
    Fleeing
}
public interface IBehaviour
{
    public PKMNState State { get; }
    public PKMNBrain Entity { get; set; }
    void Enter(PKMNBrain brain);
    void UpdateState(); // updates this state
    void UpdateBehaviour(); // initialises next state
    void Exit();
}

public class Waiting : IBehaviour
{
    public PKMNState State => PKMNState.Waiting;
    public PKMNBrain Entity { get; set; }

    private bool _shouldMove = true;

    protected float _timeUntilNextAction = -1f;
    private float _timeUntilNextIdle = -1f;
    private float _activityFactor;

    public Waiting()
    {

    }
    public Waiting(bool shouldMove)
    {
        _shouldMove = shouldMove;
    }

    public virtual void Enter(PKMNBrain brain)
    {
        Entity = brain;
        Entity.OnHover += OnHoveredOn;
        _activityFactor = Mathf.Lerp(0.5f, 2f, 1f - (Entity.Activity - 1) / 9f); // Maps activity (1-10) to a multiplier (2x for low, 0.5x for high)
        float baseTime = Random.Range(2f, 15f);
        _timeUntilNextAction = baseTime * _activityFactor;
        Debug.Log(_timeUntilNextAction);
        baseTime = Random.Range(1f, 20f);
        _timeUntilNextIdle = baseTime * _activityFactor;
    }

    public virtual void UpdateState()
    {
        if(_timeUntilNextIdle > 0)
        {
            _timeUntilNextIdle -= Time.deltaTime;
            if(_timeUntilNextIdle <= 0)
            {
                Entity.FlipSprite();
                _timeUntilNextIdle = Random.Range(1f, 20f);
                _timeUntilNextIdle *= _activityFactor;
            }
        }
    }
    public void OnHoveredOn()
    {
        _timeUntilNextAction = -1f;
        _timeUntilNextIdle = -1f;
        _shouldMove = false;
        Entity.OnHoverExit += ObservePlayer;
    }
    public void ObservePlayer()
    {
        Entity.ChangeBehaviour(new ObservingPlayer());
        Entity.OnHoverExit -= ObservePlayer;
    }

    public void UpdateBehaviour()
    {
        if (!_shouldMove || Entity.Mode == PKMNMode.Stationary) return;
        if (_timeUntilNextAction <= 0)
        {
            Entity.ChangeBehaviour(new Roaming());
        }
        else
        {
            _timeUntilNextAction -= Time.deltaTime;
        }
    }

    public void Exit()
    {
        Entity.OnHover -= OnHoveredOn;
        Entity.OnHoverExit -= ObservePlayer;
    }
}
public class Roaming : IBehaviour
{
    public PKMNState State => PKMNState.Roaming;
    private int _stridesUntilNextAction = -1;
    public PKMNBrain Entity { get; set; }

    public void Enter(PKMNBrain brain)
    {
        Entity = brain;
        _stridesUntilNextAction = Random.Range(1, 10);
        Entity.OnMoveCallback += OnMoveCallback;
        PerformStride();
        brain.OnHover += OnHoveredOn;
    }

    private void PerformStride()
    {
        Entity.OnMove.Invoke();
    }
    private void OnMoveCallback()
    {
        _stridesUntilNextAction--;
        if(_stridesUntilNextAction > 0) PerformStride();
    }
    private void OnHoveredOn()
    {
        Entity.ChangeBehaviour(new ObservingPlayer());
    }
    public void UpdateState()
    {
    }
    public void UpdateBehaviour()
    {
        if (_stridesUntilNextAction <= 0)
        {
            Entity.ChangeBehaviour(new Waiting());
        }
    }

    public void Exit()
    {
        Entity.OnMoveCallback -= OnMoveCallback;
    }
}

public class ObservingPlayer : IBehaviour
{
    public PKMNState State => PKMNState.Following;
    public PKMNBrain Entity { get; set; }

    private bool _shouldMove = true;

    protected float _timeUntilNextAction = -1f;

    private float _waitTime = 4f;
    public void Enter(PKMNBrain brain)
    {
        Entity = brain;
        Entity.OnHover += OnHoveredOn;
        _timeUntilNextAction = _waitTime;
    }
    public void UpdateState()
    {
        Vector2 pos = Entity.transform.position; // Entity's world position
        float mouseX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x; // Convert mouse to world position

        if ((Entity.FacingDirection == Direction.Left && mouseX > pos.x) ||
            (Entity.FacingDirection == Direction.Right && mouseX < pos.x))
        {
            Entity.FlipSprite();
        }
    }
    public void UpdateBehaviour()
    {
        if (_timeUntilNextAction <= 0)
        {
            Entity.ChangeBehaviour(new Waiting());
        }
        else
        {
            _timeUntilNextAction -= Time.deltaTime;
        }
    }

    public void OnHoveredOn()
    {
        _timeUntilNextAction = -1f;
        _shouldMove = false;
        Entity.OnHoverExit += ResetObserveTimer;
    }
    private void ResetObserveTimer()
    {
        _timeUntilNextAction = _waitTime;
        Entity.OnHoverExit -= ResetObserveTimer;
    }
    public void Exit()
    {
        Entity.OnHover -= OnHoveredOn;
        Entity.OnHoverExit -= ResetObserveTimer;
    }
}