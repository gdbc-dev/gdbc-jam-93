using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using UnityMovementAI;
using Cinemachine;

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
    private List<TouristData> touristShipsToSpawn;
    private float spawnTimer;
    private float surviveeTimer;
    private bool isQuitting = false;
    private int lastLevelGenerated = -1;

    // Camera management
    [SerializeField] private GameObject planningCam;
    [SerializeField] private GameObject actionCam;

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

        
    }

    [ContextMenu("Start Game")]
    public void startGame()
    {
        startLevel(currentLevelNum);
    }

    [ContextMenu("Generate Menu Level")]
    public void generateMenuLevel()
    {
        map = mapGenerator.generateMap(menuLevel);
    }

    [ContextMenu("Win Level")]
    public void winLevel()
    {
        currentLevelNum++;

        if (currentLevelNum >= levels.Count)
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
        surviveeTimer = 0;
        for (int i = 0; i < aliveDolphins.Count; i++)
        {
            Destroy(aliveDolphins[i].gameObject);
        }

        aliveDolphins.Clear();

        for (int i = 0; i < patrolBoats.Count; i++)
        {
            if (patrolBoats[i] && patrolBoats[i].gameObject)
            {
                Destroy(patrolBoats[i].gameObject);
            }
        }

        patrolBoats.Clear();
        
        for (int i = 0; i < touristBoats.Count; i++)
        {
            if (touristBoats[i] && touristBoats[i].gameObject)
            {
                Destroy(touristBoats[i].gameObject);
            }
        }

        touristBoats.Clear();

        if (levels.Count >= levelNumber)
        {
            generate(levels[levelNumber], lastLevelGenerated != currentLevelNum);
            lastLevelGenerated = currentLevelNum;
        }
        else
        {
            Debug.LogError("Invalid level: " + levelNumber);
        }

        touristShipsToSpawn = new List<TouristData>();

        for (var i = 0; i < levels[levelNumber].touristSpawns.Count; i++)
        {
            touristShipsToSpawn.Add(new TouristData
                {
                    spawnDelay = levels[levelNumber].touristSpawns[i].spawnDelay
                }
            );
        }

        StartPlanningPhase();
    }

    private void Update()
    {
        if (gameState == GAME_STATE.PLANNING)
        {
        }
        else if (gameState == GAME_STATE.PLAYING)
        {
            spawnTimer += Time.deltaTime;
            if (touristShipsToSpawn.Count > 0)
            {
                if (spawnTimer >= touristShipsToSpawn[0].spawnDelay)
                {
                    Instantiate(touristShipsPrefabs[Random.Range(0, touristShipsPrefabs.Length)],
                        getEdgeLocation(), Quaternion.Euler(0, Random.Range(0, 360), 0));
                    touristShipsToSpawn.RemoveAt(0);
                }
            }

            surviveeTimer += Time.deltaTime;

            if (surviveeTimer >= levels[currentLevelNum].surviveTime)
            {
                Debug.Log("Win Level");
                winLevel();
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

        RepositionPlanningCamera();
        actionCam.SetActive(false);
        planningCam.SetActive(true);
    }

    public void StartGamePhase()
    {
        gameState = GAME_STATE.PLAYING;
        Time.timeScale = 1;

        RepositionActionCamera();
        actionCam.SetActive(true);
        planningCam.SetActive(false);
    }


    private void generate(LevelData levelData, bool newMap)
    {
        if (newMap)
        {
            map = mapGenerator.generateMap(levelData);
        }

        generateEntities(levelData);
    }

    private void generateEntities(LevelData levelData)
    {
        if (touristShipsPrefabs.Length == 0 || dolphinsPrefabs.Length == 0)
        {
            Debug.LogError("SETUP THE PREFABS");
            return;
        }

        for (int i = 0; i < levelData.numDolphins; i++)
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

    private Vector3 getEdgeLocation()
    {
        float offScreenAmount = 5;
        if (Random.Range(0, 1f) > .5f)
        {
            // Top or bottom spawn
            return new Vector3(Random.Range(0, map.GetLength(0)), 0, Random.Range(0, 1f) > .5f ? -offScreenAmount : map.GetLength(1) + offScreenAmount);
        }

        // Left or right spawn
        return new Vector3(Random.Range(0, 1f) > .5f ? -offScreenAmount : map.GetLength(0) + offScreenAmount, 0, Random.Range(0, map.GetLength(1)));
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
        if (isQuitting)
        {
            return;
        }
        this.aliveDolphins.Remove(dolphin);

        if (aliveDolphins.Count == 0)
        {
            Debug.Log("All the dolphins were photographed!");
            loseLevel();
        }
    }

    public void finishPlanning(List<List<Vector3>> shipPaths)
    {
        for (int i = 0; i < shipPaths.Count; i++)
        {
            List<Vector3> currentPath = shipPaths[i];

            GameObject gameObject = Instantiate(playerShipsPrefabs[Random.Range(0, playerShipsPrefabs.Length)],
                new Vector3(currentPath[0].x, 0, currentPath[0].z), Quaternion.Euler(0, Random.Range(0, 360), 0));
            gameObject.GetComponent<Rigidbody>().position = new Vector3(currentPath[i].x, 0, currentPath[i].z);
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

        return true;
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    public int getMapSize()
    {
        return map.GetLength(0);
    }

    private void RepositionPlanningCamera()
    {
        var pos = new Vector3(getMapSize() / 2f, 50, getMapSize() / 2f);
        planningCam.transform.position = pos;
    }

    private void RepositionActionCamera()
    {
        actionCam.transform.position = planningCam.transform.position;
        actionCam.transform.Find("Action Virtual Camera").
            GetComponent<CinemachineVirtualCamera>().
            GetCinemachineComponent<CinemachineOrbitalTransposer>().
            m_FollowOffset.y = 90;
    }
}