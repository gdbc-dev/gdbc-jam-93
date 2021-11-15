using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanningPhaseController : MonoBehaviour
{
    private int shipsToSpawn;
    public bool isPlanning = false;
    public LayerMask hitMask;
    private List<List<Vector2Int>> shipPathLists;
    private int currentShipIndex = 0;
    private GameController gameController;
    public LineRenderer lineRenderer;
    public LineRenderer finishLineRenderer;
    public GameObject planningStartImage;
    public Material goodLineMaterial;
    public Material badLineMaterial;
    [NonSerialized] public List<LineRenderer> previousShipLinerenders;
    public LineRenderer lineRendererPrefab;
    public GameObject remainingPatrolBoatPrefab;
    public GameObject remainingPatrolContainer;
    public GameObject gridLines;

    [SerializeField] private Camera planningCamera;

    // Start is called before the first frame update
    void Awake()
    {
        gameController = FindObjectOfType<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlanning)
        {
            return;
        }


        if (gameController.gameState == GameController.GAME_STATE.PLAYING)
        {
            return;
        }

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        ;
        if (Physics.Raycast(planningCamera.ScreenPointToRay(Input.mousePosition), out hit,
            Mathf.Infinity, hitMask))
        {
            if (shipPathLists[currentShipIndex].Count > 0)
            {
                Vector3 targetPos = hit.point;
                Vector2Int newPoint = new Vector2Int((int) targetPos.x, (int) targetPos.z);
                bool isValid = isValidPath(shipPathLists[currentShipIndex][shipPathLists[currentShipIndex].Count - 1],
                    newPoint);
                finishLineRenderer.material = isValid ? goodLineMaterial : badLineMaterial;

                finishLineRenderer.positionCount = 2;
                Vector2Int lastPos = shipPathLists[currentShipIndex][shipPathLists[currentShipIndex].Count - 1];
                finishLineRenderer.SetPosition(0, new Vector3(lastPos.x + .5f, 5, lastPos.y + .5f));
                finishLineRenderer.SetPosition(1, new Vector3(newPoint.x + .5f, 5, newPoint.y + .5f));
            }
            else
            {
                finishLineRenderer.positionCount = 0;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (planningCamera == null)
            {
                planningCamera = Camera.main;
            }

            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(planningCamera.ScreenPointToRay(Input.mousePosition), out hit,
                Mathf.Infinity, hitMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance,
                    Color.yellow);
                Vector3 targetPos = hit.point;
                Vector2Int newPoint = new Vector2Int((int) targetPos.x, (int) targetPos.z);
                if (gameController.isWater(newPoint.x, newPoint.y))
                {
                    if (shipPathLists[currentShipIndex].Count == 0)
                    {
                        if (!isValidPathPosition(newPoint.x, newPoint.y))
                        {
                            Debug.Log("Invalid Path");

                            return;
                        }
                    }
                    else if (!isValidPath(shipPathLists[currentShipIndex][shipPathLists[currentShipIndex].Count - 1],
                        newPoint))
                    {
                        Debug.Log("Invalid Path");

                        return;
                    }

                    if (shipPathLists[currentShipIndex].Count > 0 &&
                        Vector2Int.Distance(shipPathLists[currentShipIndex][0], newPoint) < 5f)
                    {
                        shipPathLists[currentShipIndex].Add(shipPathLists[currentShipIndex][0]);
                        finishPlanning();
                        lineRenderer.positionCount = 0;
                    }
                    else
                    {
                        shipPathLists[currentShipIndex].Add(newPoint);
                        lineRenderer.positionCount = shipPathLists[currentShipIndex].Count;
                        if (shipPathLists[currentShipIndex].Count == 1)
                        {
                            lineRenderer.positionCount = 2;
                            planningStartImage.SetActive(true);
                            planningStartImage.transform.position = new Vector3(
                                shipPathLists[currentShipIndex][0].x + .5f, 5,
                                shipPathLists[currentShipIndex][0].y + .5f);
                            lineRenderer.SetPosition(0,
                                new Vector3(shipPathLists[currentShipIndex][0].x + .5f, 5,
                                    shipPathLists[currentShipIndex][0].y + .5f));
                            lineRenderer.SetPosition(1,
                                new Vector3(shipPathLists[currentShipIndex][0].x + .5f, 5,
                                    shipPathLists[currentShipIndex][0].y + .5f));
                            return;
                        }

                        for (int i = 0; i < shipPathLists[currentShipIndex].Count; i++)
                        {
                            lineRenderer.SetPosition(i,
                                new Vector3(shipPathLists[currentShipIndex][i].x + .5f, 5,
                                    shipPathLists[currentShipIndex][i].y + .5f));
                        }
                    }
                }
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            }
        }
    }

    public bool isValidPath(Vector2Int start, Vector2Int end)
    {
        Vector2 slope = new Vector2(end.x + .5f, end.y + .5f) - new Vector2(start.x + .5f, start.y + .5f);
        slope.Normalize();
        slope *= .5f;
        Vector2 currentPos = new Vector2(start.x + .5f, start.y + .5f);
        int attempts = 0;
        while (new Vector2Int((int) currentPos.x, (int) currentPos.y) != end && attempts < 1000)
        {
            attempts++;
            currentPos += slope;
            if (!isValidPathPosition((int) currentPos.x, (int) currentPos.y))
            {
                return false;
            }
        }

        return true;
    }

    private bool isValidPathPosition(int currentX, int currentY)
    {
        int surroundCheckAmount = 1;

        for (int x = -surroundCheckAmount; x <= surroundCheckAmount; x++)
        {
            for (int y = -surroundCheckAmount; y <= surroundCheckAmount; y++)
            {
                if (!gameController.isWater(currentX + x, currentY + y))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void StartPlanning(int numShips)
    {
        gridLines.SetActive(true);
        shipsToSpawn = numShips;

        int childCount = remainingPatrolContainer.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(remainingPatrolContainer.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < shipsToSpawn; i++)
        {
            Instantiate(remainingPatrolBoatPrefab, remainingPatrolContainer.transform);
        }

        if (previousShipLinerenders == null)
        {
            previousShipLinerenders = new List<LineRenderer>();
        }

        for (int i = 0; i < previousShipLinerenders.Count; i++)
        {
            Destroy(previousShipLinerenders[i]);
        }

        previousShipLinerenders.Clear();
        planningStartImage.SetActive(false);
        lineRenderer.gameObject.SetActive(true);
        isPlanning = true;
        shipPathLists = new List<List<Vector2Int>>();
        currentShipIndex = -1;
        StartShipPath();
    }

    private void StartShipPath()
    {
        shipPathLists.Add(new List<Vector2Int>());
        currentShipIndex++;
    }

    public void finishPlanning()
    {
        finishLineRenderer.positionCount = 0;

        planningStartImage.SetActive(false);
        for (int i = 0; i < previousShipLinerenders.Count; i++)
        {
            DestroyImmediate(previousShipLinerenders[i].gameObject);
        }
        previousShipLinerenders.Clear();

        for (int k = 0; k < shipPathLists.Count; k++)
        {
            previousShipLinerenders.Add(Instantiate(lineRendererPrefab));
        }

        for (int k = 0; k < shipPathLists.Count; k++)
        {
            previousShipLinerenders[k].positionCount = shipPathLists[k].Count;

            for (int i = 0; i < shipPathLists[k].Count; i++)
            {
                previousShipLinerenders[k].SetPosition(i,
                    new Vector3(shipPathLists[k][i].x + .5f, 5,
                        shipPathLists[k][i].y + .5f));
            }
        }
        
        if (remainingPatrolContainer.transform.childCount > 0)
        {
            Destroy(remainingPatrolContainer.transform.GetChild(remainingPatrolContainer.transform.childCount - 1).gameObject);
        }

        if (currentShipIndex + 1 >= shipsToSpawn)
        {
            isPlanning = false;
        }
        else
        {
            Debug.Log("Starting next ship");
            StartShipPath();
        }
    }

    public void finishPlanningAndStart()
    {
        if (isPlanning)
        {
            return;
        }
        
        gridLines.SetActive(false);
        
        for (int i = 0; i < previousShipLinerenders.Count; i++)
        {
            Destroy(previousShipLinerenders[i].gameObject);
        }

        previousShipLinerenders.Clear();
        Debug.Log("Finish Planning");
        lineRenderer.gameObject.SetActive(false);
        finishLineRenderer.positionCount = 0;

        List<List<Vector3>> shipPaths = new List<List<Vector3>>();
        for (int i = 0; i < shipPathLists.Count; i++)
        {
            shipPaths.Add(new List<Vector3>());

            for (int j = 0; j < shipPathLists[i].Count; j++)
            {
                shipPaths[i].Add(new Vector3(shipPathLists[i][j].x + .5f, 0, shipPathLists[i][j].y + .5f));
            }
        }

        gameController.finishPlanning(shipPaths);
    }
}