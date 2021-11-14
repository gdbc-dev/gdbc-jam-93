using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanningPhaseController : MonoBehaviour
{
    private int shipsToSpawn;
    private bool isPlanning = false;
    public LayerMask hitMask;
    private List<List<Vector2Int>> shipPathLists;
    private int currentShipIndex = 0;
    private GameController gameController;
    public LineRenderer lineRenderer;
    public GameObject planningStartImage;

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

        if (Input.GetMouseButtonDown(0))
        {
            if (planningCamera == null)
            {
                planningCamera = Camera.main;
            }

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            ;
            if (Physics.Raycast(planningCamera.ScreenPointToRay(Input.mousePosition), out hit,
                Mathf.Infinity, hitMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance,
                    Color.yellow);
                Vector3 targetPos = hit.point;
                Vector2Int newPoint = new Vector2Int((int) targetPos.x, (int) targetPos.z);
                if (gameController.isWater(newPoint.x, newPoint.y))
                {
                    if (shipPathLists[currentShipIndex].Count > 0 && Vector2Int.Distance(shipPathLists[currentShipIndex][0], newPoint) < 5f)
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
                            planningStartImage.transform.position = new Vector3(shipPathLists[currentShipIndex][0].x + .5f, 5, shipPathLists[currentShipIndex][0].y + .5f);
                            lineRenderer.SetPosition(0, new Vector3(shipPathLists[currentShipIndex][0].x + .5f, 5, shipPathLists[currentShipIndex][0].y + .5f));
                            lineRenderer.SetPosition(1, new Vector3(shipPathLists[currentShipIndex][0].x + .5f, 5, shipPathLists[currentShipIndex][0].y + .5f));
                            return;
                        }

                        for (int i = 0; i < shipPathLists[currentShipIndex].Count; i++)
                        {
                            lineRenderer.SetPosition(i, new Vector3(shipPathLists[currentShipIndex][i].x + .5f, 5, shipPathLists[currentShipIndex][i].y + .5f));
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

    public void StartPlanning(int numShips)
    {
        planningStartImage.SetActive(false);
        lineRenderer.gameObject.SetActive(true);
        shipsToSpawn = numShips;
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
        planningStartImage.SetActive(false);

        if (currentShipIndex + 1 >= shipsToSpawn)
        {
            List<List<Vector3>> shipPaths = new List<List<Vector3>>();
            for (int i = 0; i < shipPathLists.Count; i++)
            {
                shipPaths.Add(new List<Vector3>());

                for (int j = 0; j < shipPathLists[i].Count; j++)
                {
                    shipPaths[i].Add(new Vector3(shipPathLists[i][j].x + .5f, 0, shipPathLists[i][j].y + .5f));
                }
            }

            Debug.Log("Finish Planning");
            lineRenderer.gameObject.SetActive(false);
            isPlanning = false;
            gameController.finishPlanning(shipPaths);
        }
        else
        {
            Debug.Log("Starting next ship");
            StartShipPath();
        }
    }
}