using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using UnityMovementAI;

public class GameController : MonoBehaviour
{
    public PlanningPhaseController planningPhaseController;
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

    public int currentLevelNum = 0;

    private GAME_STATE gameState = GAME_STATE.PLANNING;

    private int shipsLeftToSpawn;

    public enum GAME_STATE
    {
        PLANNING,
        PLAYING
    }

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
        startLevel(currentLevelNum);
    }

    [ContextMenu("Generate Menu Level")]
    public void generateMenuLevel()
    {
        mapGenerator.generateMap(menuLevel);
    }

    [ContextMenu("Win Level")]
    public void winLevel()
    {
        currentLevelNum++;

        if (currentLevelNum == levels.Count)
        {
            Debug.Log("Won Game!");
        }
        else
        {
            startLevel(currentLevelNum);
        }
    }

    [ContextMenu("Lose Level")]
    public void loseLevel()
    {
        startLevel(currentLevelNum);
    }

    [ContextMenu("Restart Game")]
    public void restart()
    {
        currentLevelNum = 0;
        startLevel(currentLevelNum);
    }

    public void startLevel(int levelNumber)
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

        if (levels.Count >= levelNumber)
        {
            generate(levels[levelNumber]);
        }
        else
        {
            Debug.LogError("Invalid level: " + levelNumber);
        }

        StartPlanningPhase();
    }

    private void Update()
    {
        if (gameState == GAME_STATE.PLANNING)
        {
            if (shipsLeftToSpawn <= 0)
            {
                StartGamePhase();
            }
        }
    }

    public void StartPlanningPhase()
    {
        gameState = GAME_STATE.PLANNING;
        LevelData currentLevel = levels[currentLevelNum];
        shipsLeftToSpawn = currentLevel.numShips;
        Time.timeScale = 0;
        planningPhaseController.StartPlanning(shipsLeftToSpawn);
    }

    public void StartGamePhase()
    {
        gameState = GAME_STATE.PLAYING;
        Time.timeScale = 1;
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

//        for (int i = 0; i < levelData.numShips; i++)
//        {
//            Instantiate(playerShipsPrefabs[Random.Range(0, playerShipsPrefabs.Length)],
//                getWaterLocation(), Quaternion.Euler(0, Random.Range(0, 360), 0));
//        }
//        
//        for (int i = 0; i < levelData.numShips; i++)
//        {
//            Instantiate(touristShipsPrefabs[Random.Range(0, touristShipsPrefabs.Length)],
//                getWaterLocation(), Quaternion.Euler(0, Random.Range(0, 360), 0));
//        }
    }

    private Vector3 getWaterLocation()
    {
        int attempts = 0;
        while (attempts < 1000)
        {
            attempts++;
            Vector2Int pos = new Vector2Int(Random.Range(5, map.GetLength(0)), Random.Range(5, map.GetLength(1)));
            if (isWater(pos.x, pos.y))
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

    public void finishPlanning(List<List<Vector3>> shipPaths)
    {
        for (int i = 0; i < shipPaths.Count; i++)
        {
            List<Vector3> currentPath = shipPaths[i];

            GameObject gameObject = Instantiate(playerShipsPrefabs[Random.Range(0, playerShipsPrefabs.Length)],
                new Vector3(currentPath[0].x, 0, currentPath[0].y), Quaternion.Euler(0, Random.Range(0, 360), 0));
            gameObject.GetComponent<Rigidbody>().position = new Vector3(currentPath[0].x, 0, currentPath[0].y);

            gameObject.GetComponent<PlayerBoatsController>().path = new LinePath(currentPath.ToArray());
        }

        shipsLeftToSpawn = 0;
        StartGamePhase();
    }

    public bool isWater(int x, int y)
    {
        if (x > 0 && y > 0 && x < map.GetLength(0) && y < map.GetLength(1))
        {
            return map[x, y] == 0;
        }

        return false;
    }
}