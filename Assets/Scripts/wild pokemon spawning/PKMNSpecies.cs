using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class PKMNSpecies
{
    public int ID; //unique identifier for each species(pokedex number)4
    public string PokedexNumber => ID.ToString("D3"); // Pokedex number with leading zeros

    public List<string> TypeStrings;

    // Cached fields
    private PKMNType _primaryType = PKMNType.None;
    private PKMNType _secondaryType = PKMNType.None;

    // Constructor or an initialization method
    public void InitializeTypes()
    {
        if (TypeStrings != null && TypeStrings.Count > 0 &&
            Enum.TryParse(TypeStrings[0], true, out PKMNType parsedPrimary))
            _primaryType = parsedPrimary;
        else
            _primaryType = PKMNType.None;

        if (TypeStrings != null && TypeStrings.Count > 1 &&
            Enum.TryParse(TypeStrings[1], true, out PKMNType parsedSecondary))
            _secondaryType = parsedSecondary;
        else
            _secondaryType = PKMNType.None;
    }

    public PKMNType PrimaryType => _primaryType;
    public PKMNType SecondaryType => _secondaryType;

    public string Name;
    public int SpawnWeight; // Higher = more common, 1-100 (-1 if not spawnable)
    public bool IsSpawnable => SpawnWeight > 0;
    public int Strength; //1-10, for hp, attack strength etc

    public int MinLevel;       // Minimum possible level
    public int MaxLevel;       // Maximum possible level

    public int EvolvesAtLevel = -1; // Level at which the species evolves(-1 if it doesn't evolve)
    public int EvolvesToID = -1;   // ID of the species it evolves into(-1 if it doesn't evolve)
    public string SpecialEvolutionMethod = ""; // Special method of evolution (e.g., trade, item, etc.)
}
public enum PKMNType
{
    None,
    Normal,
    Fire,
    Water,
    Grass,
    Electric,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Fairy
}
