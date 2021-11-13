using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using UnityMovementAI;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public MapGenerator mapGenerator;
    public List<LevelData> levels;
    public LevelData menuLevel;
    public List<MovementAIRigidbody> aliveDolphins;
    public List<MovementAIRigidbody> patrolBoats;
    public List<MovementAIRigidbody> touristBoats;

    public GameObject[] playerShipsPrefabs;
    public GameObject[] touristShipsPrefabs;
    public GameObject[] dolphinsPrefabs;
    public GameObject[] environmentPrefabs;

    private int[,] map;

    public int currentLevel = 0;
    private bool isPlayingGame = false;

    [SerializeField] private bool generateMenuLevelEnabled = true;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (generateMenuLevelEnabled)
        {
            generateMenuLevel();
        }
        else
        {
            startGame();
        }

        // needs some code to populate the dolphin and patrol boats list
        // could do find objects with tags or add them when they are spawned?
    }

    [ContextMenu("Start Game")]
    public void startGame()
    {
        generateLevel(currentLevel);
    }

    [ContextMenu("Generate Menu Level")]
    public void generateMenuLevel()
    {
        mapGenerator.generateMap(menuLevel);
        isPlayingGame = false;
    }

    [ContextMenu("Win Level")]
    public void winLevel()
    {
        currentLevel++;

        if (currentLevel == levels.Count)
        {
            Debug.Log("Won Game!");
        }
        else
        {
            generateLevel(currentLevel);
        }
    }

    [ContextMenu("Lose Level")]
    public void loseLevel()
    {
        generateLevel(currentLevel);
    }

    [ContextMenu("Restart Game")]
    public void restart()
    {
        currentLevel = 0;
        generateLevel(currentLevel);
    }

    public void generateLevel(int levelNumber)
    {
        for (int i = 0; i < aliveDolphins.Count; i++)
        {
            Destroy(aliveDolphins[i].gameObject);
        }
        
        aliveDolphins.Clear();
        
        for (int i = 0; i < patrolBoats.Count; i++)
        {
            Destroy(patrolBoats[i].gameObject);
        }
        
        patrolBoats.Clear();
        
        isPlayingGame = true;
        if (levels.Count >= levelNumber)
        {
            generate(levels[levelNumber]);
        }
        else
        {
            Debug.LogError("Invalid level: " + levelNumber);
        }
    }


    private void generate(LevelData levelData)
    {
        map = mapGenerator.generateMap(levelData);
        generateEntities(levelData);
    }

    private void generateEntities(LevelData levelData)
    {
        if (touristShipsPrefabs.Length == 0 || dolphinsPrefabs.Length == 0)
        {
            Debug.LogError("SETUP THE PREFABS");
            return;
        }
        
        for (int i = 0; i < levelData.numShips; i++)
        {
            Instantiate(dolphinsPrefabs[Random.Range(0, dolphinsPrefabs.Length)],
                getWaterLocation(), Quaternion.Euler(0, Random.Range(0, 360), 0));
        }
        
        for (int i = 0; i < levelData.numShips; i++)
        {
            Instantiate(playerShipsPrefabs[Random.Range(0, playerShipsPrefabs.Length)],
                getWaterLocation(), Quaternion.Euler(0, Random.Range(0, 360), 0));
        }
        
        for (int i = 0; i < levelData.numShips; i++)
        {
            Instantiate(touristShipsPrefabs[Random.Range(0, touristShipsPrefabs.Length)],
                getWaterLocation(), Quaternion.Euler(0, Random.Range(0, 360), 0));
        }


    }

    private Vector3 getWaterLocation()
    {
        int attempts = 0;
        while (attempts < 1000)
        {
            attempts++;
            Vector2Int pos = new Vector2Int(Random.Range(5, map.GetLength(0)), Random.Range(5, map.GetLength(1)));
            if (map[pos.x, pos.y] == 0)
            {
                return new Vector3(pos.x, 0, pos.y);
            }
        }
        return new Vector3();
    }

    public void addPatrolBoat(MovementAIRigidbody boat)
    {
        this.patrolBoats.Add(boat);
    }
    
    public void addTouristBoat(MovementAIRigidbody ship)
    {
        this.touristBoats.Add(ship);
    }
    public void addDolphin(MovementAIRigidbody dolphin)
    {
        this.aliveDolphins.Add(dolphin);
    }
    public void removeTouristBoat(MovementAIRigidbody ship)
    {
        this.touristBoats.Remove(ship);
    }


    
    public void removePatrolBoat(MovementAIRigidbody boat)
    {
        this.patrolBoats.Remove(boat);
    }

    public void removeDolphin(MovementAIRigidbody dolphin)
    {
        this.aliveDolphins.Remove(dolphin);
    }
}