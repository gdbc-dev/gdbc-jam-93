using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityMovementAI;

public class GameController : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public List<LevelData> levels;
    public LevelData menuLevel;
    public List<MovementAIRigidbody> aliveDolphins;
    public List<MovementAIRigidbody> patrolBoats;

    public GameObject[] playerShipsPrefabs;
    public GameObject[] touristShipsPrefabs;
    public GameObject[] dolphinsPrefabs;
    public GameObject[] environmentPrefabs;

    public int currentLevel = 0;
    private bool isPlayingGame = false;

    [SerializeField] private bool generateMenuLevelEnabled = true;

    private void Start()
    {
        if (generateMenuLevelEnabled)
        {
            generateMenuLevel();
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
        mapGenerator.generateMap(levelData);
    }

    private void generateEntities()
    {
        List<GameObject> alivePrefabs = new List<GameObject>();

        alivePrefabs.Add(Instantiate(touristShipsPrefabs[Random.Range(0, touristShipsPrefabs.Length)],
            getWaterLocation(), Quaternion.Euler(0, Random.Range(0, 360), 0)));
    }

    private Vector3 getWaterLocation()
    {
        return new Vector3();
    }
}