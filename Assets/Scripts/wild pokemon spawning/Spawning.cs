using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private List<TextAsset> _spawnTables;
    [SerializeField]
    private GameObject _pokemonPrefab;
    private SpriteFetcher _spriteFetcher;

    const int SHINY_CHANCE = 1; // 1 in 1000
    private void Awake()
    {
        _spriteFetcher = new SpriteFetcher();
    }

    public PKMNSpecies GetWeightedRandomSpecies(List<PKMNSpecies> list)
    {
        int totalWeight = 0;
        foreach (var species in list)
        {
            if (species.IsSpawnable)
                totalWeight += species.SpawnWeight;
        }

        int randomWeight = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var species in list)
        {
            if (species.IsSpawnable)
            {
                cumulative += species.SpawnWeight;
                if (randomWeight < cumulative)
                {
                    // Found the species
                    Debug.Log($"Selected Species: {species.Name}");
                    return species;
                }
            }
        }
        return null; // fallback
    }
    public PKMNSpecies GetSpeciesByName(string name)
    {
        foreach (var textAsset in _spawnTables)
        {
            List<PKMNSpecies> validPokemon = JsonUtility.FromJson<PKMNSpawnTable>(textAsset.text).SpeciesList;
            foreach (var species in validPokemon)
            {
                if (species.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return species;
                }
            }
        }
        return null; // fallback
    }
    public PKMNSpecies GetWeightedRandomSpecies(List<PKMNSpecies> list, int playerAverageLevel)
    {
        int totalWeight = 0;
        List<(PKMNSpecies species, int adjustedWeight)> weightedList = new List<(PKMNSpecies, int)>();

        foreach (var species in list)
        {
            if (!species.IsSpawnable)
                continue;

            // Determine if the species is much stronger than the player
            int levelDiff = species.MinLevel - playerAverageLevel;

            float modifier = 1f;

            if (levelDiff > 0)
            {
                // For every level above the player, reduce weight
                modifier = 1f / (1f + levelDiff * 0.5f); // Tune this factor to control dropoff
            }

            int adjustedWeight = Mathf.Max(1, Mathf.RoundToInt(species.SpawnWeight * modifier));
            weightedList.Add((species, adjustedWeight));
            totalWeight += adjustedWeight;
        }

        int randomWeight = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var (species, adjustedWeight) in weightedList)
        {
            cumulative += adjustedWeight;
            if (randomWeight < cumulative)
            {
                Debug.Log($"Selected Species: {species.Name} (Adjusted Weight: {adjustedWeight})");
                return species;
            }
        }

        return null; // fallback
    }
    public GameObject InstantiateWildPokemon(PKMNSpecies species)
    {
        GameObject pokemon = Instantiate(_pokemonPrefab);

        PKMNBrain brain = pokemon.GetComponent<PKMNBrain>();

        string name = "Wild " + species.Name; // Set the name of the Pokemon

        bool isShiny = Random.Range(0, 1000) < SHINY_CHANCE; // Set shiny chance
        Sprite newSprite = null;
        if (_spriteFetcher == null)
        {
            Debug.Log("no sprite fetcher!!");
            Destroy(pokemon);
            return null;
        }
        else
        {
            newSprite = _spriteFetcher.GetSpriteFromSpecies(species, 0, brain.IsShiny);
            if (newSprite == null)
            {
                Debug.LogError($"Failed to load sprite for {species.Name}");
                Destroy(pokemon);
                return null;
            }
        }
        float speed = Random.Range(1f, 5f);
        float activity = Random.Range(3f, 10f);

        IMovement movement = new Walking(brain);

        if (species.Movement == "Stationary")
        {
            speed = 0f;
        }
        else if (species.Movement == "Flying")
        {
            speed = Random.Range(2.4f, 4f);
            movement = new Flying(brain);
        }

        //Debug.Log("brain is null: " + (brain == null).ToString() + " species is null: " + (species == null).ToString() + " name is null: " + (name == null).ToString() + " shiny is null: " + (isShiny == null).ToString() + " sprite is null: " + (newSprite == null).ToString() + " speed is null: " + (speed == null).ToString() + " activity is null: " + (activity == null).ToString() + " movement is null: " + (movement == null));

        brain.Initialize(species, name, isShiny, true, newSprite, speed, activity, movement);

        return pokemon;
    }
    public GameObject InstantiateOwnedPokemon(PKMNBrain brain)
    {
        GameObject pokemon = Instantiate(_pokemonPrefab);

        Sprite sprite = null;

        if (_spriteFetcher == null)
        {
            Debug.Log("no sprite fetcher!!");
            Destroy(pokemon);
            return null;
        }
        else
        {
            sprite = _spriteFetcher.GetSpriteFromSpecies(brain.Species, 0, brain.IsShiny);
            if (sprite == null)
            {
                Debug.LogError($"Failed to load sprite for {brain.Species.Name}");
                throw new System.Exception($"Failed to load sprite for {brain.Species.Name}");
            }
        }

        PKMNBrain newBrain = pokemon.GetComponent<PKMNBrain>();
        newBrain.Initialize(brain.Species, brain.Name, brain.IsShiny, false, sprite, brain.Speed, brain.Activity, GetMovement(brain.Species.Movement, newBrain));


        return pokemon;
    }
    public GameObject SpawnWildPokemon()
    {
        TextAsset textAsset = _spawnTables[Random.Range(0, _spawnTables.Count)];
        List<PKMNSpecies> validPokemon = JsonUtility.FromJson<PKMNSpawnTable>(textAsset.text).SpeciesList;
        Debug.Log($"Loaded {validPokemon.Count} species from {textAsset.name}");

        LogSpawnChances(validPokemon);

        PKMNSpecies species = GetWeightedRandomSpecies(validPokemon);

        return InstantiateWildPokemon(species);
    }
    public GameObject SpawnWildPokemon(int playerAverageLevel)
    {
        TextAsset textAsset = _spawnTables[Random.Range(0, _spawnTables.Count)];
        List<PKMNSpecies> validPokemon = JsonUtility.FromJson<PKMNSpawnTable>(textAsset.text).SpeciesList;
        Debug.Log($"Loaded {validPokemon.Count} species from {textAsset.name}");

        LogSpawnChances(validPokemon);

        PKMNSpecies species = GetWeightedRandomSpecies(validPokemon, playerAverageLevel);

        return InstantiateWildPokemon(species);
    }

    public GameObject SpawnWildPokemon(string name)
    {
        PKMNSpecies species = GetSpeciesByName(name);
        if (species == null)
        {
            Debug.LogError($"Species {name} not found!");
            return null;
        }
        return InstantiateWildPokemon(species);
    }

    public void LogSpawnChances(List<PKMNSpecies> list)
    {
        int totalWeight = 0;

        // First pass: calculate total weight
        foreach (var species in list)
        {
            if (species.IsSpawnable)
                totalWeight += species.SpawnWeight;
        }

        foreach (var species in list)
        {
            if (!species.IsSpawnable)
                continue;

            float chance = (float)species.SpawnWeight / totalWeight * 100f;
            Debug.Log($"{species.Name}: {chance:F2}%");
        }
    }
    public IMovement GetMovement(string movementType, PKMNBrain brain)
    {
        switch (movementType)
        {
            case "Walking":
                return new Walking(brain);
            case "Flying":
                return new Flying(brain);
            //case "Stationary":
                //return new Stationary(brain);
            default:
                Debug.LogError($"Unknown movement type: {movementType}");
                return null;
        }
    }
}
public class SpriteFetcher
{
    public Sprite GetSpriteFromSpecies(PKMNSpecies species, int region, bool shiny)
    {
        string regionFolder = "";
        switch (region)  
        {
            case 0:
                regionFolder = "Kanto";
                break;
            default:
                Debug.LogError($"Invalid region: {region}");
                break;
        }

        string path = $"Sprites/{regionFolder}/{(shiny ? "Shiny/0" : "")}{species.PokedexNumber} - {species.Name}";

        Sprite sprite = Resources.Load<Sprite>(path);
        if(sprite != null)
        {
            Debug.Log($"Loaded sprite for {species.Name} from {regionFolder}");
            return sprite;
        }
        else
        {
            Debug.LogError($"Failed to load sprite for {species.Name} from {regionFolder} at {path}");
        }

        return null;
    }
}