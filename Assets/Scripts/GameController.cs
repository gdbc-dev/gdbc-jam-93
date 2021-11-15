using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using UnityMovementAI;
using Cinemachine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public PlanningPhaseController planningPhaseController;
    public ActionScreenUI actionScreenUi;
    public GameObject planningScreenUI;
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
    public Texture2D[] planningCursor;
    public GameObject victoryScreen;
    public GameObject defeatScreen;

    private int[,] map;

    public int currentLevelNum = 0;

    [NonSerialized] public GAME_STATE gameState = GAME_STATE.PLANNING;

    private List<TouristData> touristShipsToSpawn;
    private float spawnTimer;
    private float surviveeTimer;
    private bool isQuitting = false;
    private int lastLevelGenerated = -1;
    private int remainingTouristBoats = 0;

    // Camera management
    [SerializeField] private GameObject planningCam;
    [SerializeField] private GameObject actionCam;
    public GameObject cameraTarget;

    public Vector2 cursorOffset;

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
        victoryScreen.SetActive(true);
        StartCoroutine(winInABit());
    }

    IEnumerator winInABit()
    {
        currentLevelNum++;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(2.5f);
        victoryScreen.SetActive(false);

        if (currentLevelNum >= levels.Count)
        {
            Debug.Log("Won Game!");
            SceneManager.LoadScene("StartMenu");
        }
        else
        {
            Debug.Log("Starting next level!");
            startLevel(currentLevelNum);
        }
    }

    IEnumerator loseInABit()
    {
        if (!isQuitting)
        {
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(2.5f);

            defeatScreen.SetActive(false);
            if (Application.isPlaying)
            {
                startLevel(currentLevelNum);
            }
        }
    }

    [ContextMenu("Restart Level")]
    public void restartLevel()
    {
        Debug.Log("Restarting Level!!");
        startLevel(currentLevelNum);
    }

    [ContextMenu("Lose Level")]
    public void loseLevel()
    {
        defeatScreen.SetActive(true);
        Debug.Log("Lost Level!!");
        StartCoroutine(loseInABit());
    }

    [ContextMenu("Restart Game")]
    public void restartGame()
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

    private float cursorTimer = 0;
    private int cursorIndex = 0;

    private void Update()
    {
        if (generateMenuLevelEnabled)
        {
            return;
        }

        if (gameState == GAME_STATE.PLANNING)
        {
            if (planningPhaseController.isPlanning)
            {
                cursorTimer += Time.unscaledDeltaTime;
                if (cursorTimer >= .2f)
                {
                    cursorTimer = 0;
                    cursorIndex = (cursorIndex + 1) % planningCursor.Length;

                    Cursor.SetCursor(planningCursor[cursorIndex], new Vector2(planningCursor[cursorIndex].width / 2f, planningCursor[cursorIndex].height / 2f), CursorMode.Auto);
                }
            }
            else
            {
                Cursor.SetCursor(null, new Vector2(.5f, .5f), CursorMode.Auto);
            }
        }
        else if (gameState == GAME_STATE.PLAYING)
        {
            Cursor.SetCursor(null, new Vector2(.5f, .5f), CursorMode.Auto);
            spawnTimer += Time.deltaTime;
            if (touristShipsToSpawn.Count > 0)
            {
                if (spawnTimer >= touristShipsToSpawn[0].spawnDelay)
                {
                    spawnTimer = 0;
                    Instantiate(touristShipsPrefabs[Random.Range(0, touristShipsPrefabs.Length)],
                        getEdgeLocation(), Quaternion.Euler(0, Random.Range(0, 360), 0));
                    touristShipsToSpawn.RemoveAt(0);
                }
            }

            surviveeTimer += Time.deltaTime;

            if (surviveeTimer >= levels[currentLevelNum].surviveTime)
            {
                surviveeTimer = 0;
                Debug.Log("Win Level");
                winLevel();
            }
        }
    }

    public void StartPlanningPhase()
    {
        gameState = GAME_STATE.PLANNING;
        actionScreenUi.gameObject.SetActive(false);
        planningScreenUI.SetActive(true);
        LevelData currentLevel = levels[currentLevelNum];
        Time.timeScale = 0;
        planningPhaseController.StartPlanning(currentLevel.numShips);

        RepositionPlanningCamera();
        actionCam.SetActive(false);
        planningCam.SetActive(true);
        remainingTouristBoats = touristShipsToSpawn.Count;
        actionScreenUi.StartingTouristIcons(remainingTouristBoats);
    }

    public void StartGamePhase()
    {
        planningScreenUI.SetActive(false);
        Cursor.SetCursor(null, new Vector2(.5f, .5f), CursorMode.Auto);
        cameraTarget.transform.position =
            new Vector3(getMapSize() / 2f, cameraTarget.transform.position.y, getMapSize() / 2f);
        actionScreenUi.gameObject.SetActive(true);
        gameState = GAME_STATE.PLAYING;
        Time.timeScale = 1;
        actionScreenUi.timeRemaining = levels[currentLevelNum].surviveTime;
        remainingTouristBoats = touristShipsToSpawn.Count;

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
        float offScreenAmount = 35;
        if (Random.Range(0, 1f) > .5f)
        {
            // Top or bottom spawn
            return new Vector3(Random.Range(0, map.GetLength(0)), 0,
                Random.Range(0, 1f) > .5f ? -offScreenAmount : map.GetLength(1) + offScreenAmount);
        }

        // Left or right spawn
        return new Vector3(Random.Range(0, 1f) > .5f ? -offScreenAmount : map.GetLength(0) + offScreenAmount, 0,
            Random.Range(0, map.GetLength(1)));
    }

    private Vector3 getWaterLocation()
    {
        int attempts = 0;
        while (attempts < 1000)
        {
            attempts++;
            Vector2Int pos = new Vector2Int(Random.Range(15, map.GetLength(0)), Random.Range(15, map.GetLength(1)));
            if (hasWaterAround(pos.x, pos.y, 2))
            {
                return new Vector3(pos.x, 0, pos.y);
            }
        }

        return new Vector3();
    }

    private bool hasWaterAround(int currentX, int currentY, int amount)
    {
        int surroundCheckAmount = amount;

        for (int x = -surroundCheckAmount; x <= surroundCheckAmount; x++)
        {
            for (int y = -surroundCheckAmount; y <= surroundCheckAmount; y++)
            {
                if (!isWater(currentX + x, currentY + y))
                {
                    return false;
                }
            }
        }

        return true;
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
        actionScreenUi.StartingDolphinsIcons(aliveDolphins.Count);
    }

    public void removeTouristBoat(MovementAIRigidbody ship)
    {
        if (isQuitting)
        {
            return;
        }

        remainingTouristBoats--;
        actionScreenUi.StartingTouristIcons(remainingTouristBoats);

        if (remainingTouristBoats == 0 && touristShipsToSpawn.Count == 0)
        {
            winLevel();
        }
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
        actionScreenUi.StartingDolphinsIcons(aliveDolphins.Count);

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

            if (currentPath != null && currentPath.Count > 0)
            {
                GameObject gameObject = Instantiate(playerShipsPrefabs[Random.Range(0, playerShipsPrefabs.Length)],
                    new Vector3(currentPath[0].x, 0, currentPath[0].z), Quaternion.Euler(0, Random.Range(0, 360), 0));
                gameObject.GetComponent<Rigidbody>().position = new Vector3(currentPath[0].x, 0, currentPath[0].z);
                gameObject.GetComponent<PlayerBoatsController>().path = new LinePath(currentPath.ToArray());
            }
        }

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

    public Vector3 ReturnCentrePointOfMap()
    {
        return new Vector3(getMapSize() / 2, 0, getMapSize() / 2);
    }

    private void RepositionPlanningCamera()
    {
        var pos = new Vector3(getMapSize() / 2f, 50, getMapSize() / 2f);
        planningCam.transform.position = pos;
        planningCam.GetComponent<Camera>().orthographicSize = 35;
    }

    private void RepositionActionCamera()
    {
        actionCam.transform.position = planningCam.transform.position;
        actionCam.transform.Find("Action Virtual Camera").GetComponent<CinemachineVirtualCamera>()
            .GetCinemachineComponent<CinemachineOrbitalTransposer>().m_FollowOffset.y = 45;
    }
}