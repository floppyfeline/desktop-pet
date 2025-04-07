using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _activePokemon = new List<GameObject>();

    static InputAction _mousePos;
    public static Vector2 MousePosition;

    SpawnManager _spawnManager;

    private void Start()
    {
        _spawnManager = GetComponent<SpawnManager>();
        _mousePos = new InputAction(binding: "<Mouse>/position");
        _mousePos.Enable();

        _activePokemon.Add(_spawnManager.SpawnWildPokemon());
        _activePokemon.Add(_spawnManager.SpawnWildPokemon("Butterfree"));
    }
    private void LateUpdate()
    {
        MousePosition = _mousePos.ReadValue<Vector2>();
    }

    public static Vector2 GetMousePosition()
    {
        return _mousePos.ReadValue<Vector2>();
    }

    public void SaveGame()
    {
    }
}
