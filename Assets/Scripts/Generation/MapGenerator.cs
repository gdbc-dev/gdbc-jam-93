using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    public Tilemap groundTilemap;
    public NoiseSettings noiseSettings;
    public int size = 180;

    public TileBase groundTile;
    public float distanceFalloff = .6f;
    public float minHeightForLand = .5f;

    public bool autoGenerate = false;
    public int minIslandSize = 16;
    public int maxIslandSize = 32;
    private float timer = 0;
    public int maxIslands = 25;
    public GameObject groundPrefab;
    public GameObject[] groundEdgePrefabs;
    public GameObject[] groundMainsPrefab;
    public GameObject[] outerDoodads;
    public GameObject[] innerDoodads;
    public GameObject[] largeInnerDoodads;
    public GameObject[] waterDoodads;
    public GameObject[] waterBesideLand;
    public List<GameObject> grounds;

    private void Start()
    {
        generateMap();
    }

    public void Update()
    {
        if (autoGenerate)
        {
            timer += Time.deltaTime;
            if (timer >= 0)
            {
                generateMap();
                timer -= .5f;
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            generateMap();
        }
    }

    [ContextMenu("New Map")]
    private void generateMap()
    {
        List<IslandData> islands = new List<IslandData>();
        for (int i = 0; i < maxIslands; i++)
        {
            IslandData islandData = generateIslandData();
            if (islandData != null)
            {
                islands.Add(islandData);
            }
        }


        int[,] map = new int[size, size];
        List<RectInt> usedBounds = new List<RectInt>();
        for (int i = 0; i < islands.Count; i++)
        {
            for (int attempts = 0; attempts < 10; attempts++)
            {
                RectInt newPlacement = new RectInt(Random.Range(0, size - islands[i].width), Random.Range(0, size - islands[i].height), islands[i].width, islands[i].height);

                bool valid = true;
                for (int j = 0; j < usedBounds.Count; j++)
                {
                    if (newPlacement.Overlaps(usedBounds[j]))
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    usedBounds.Add(newPlacement);
                    islands[i].xOffset = newPlacement.x;
                    islands[i].yOffset = newPlacement.y;
                    applyIslandToMap(map, islands[i]);
                    break;
                }
            }
        }

        for (int i = 0; i < grounds.Count; i++)
        {
            Destroy(grounds[i]);
        }

        grounds.Clear();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (map[x, y] > 0)
                {
                    bool isEde = isEdge(map, x, y);
                    grounds.Add(Instantiate(groundMainsPrefab[Random.Range(0, groundMainsPrefab.Length)], new Vector3(x, map[x,y], y ), Quaternion.identity));

                    if (isEde)
                    {
                        if (Random.Range(0, 1f) > .85f)
                        {
                            grounds.Add(Instantiate(outerDoodads[Random.Range(0, outerDoodads.Length)], new Vector3(x, map[x,y], y ), Quaternion.Euler(0, Random.Range(0,360),0)));
                        }
                    }
                    else
                    {
                        if (Random.Range(0, 1f) > .97f)
                        {
                            grounds.Add(Instantiate(largeInnerDoodads[Random.Range(0, largeInnerDoodads.Length)], new Vector3(x, map[x,y], y ), Quaternion.Euler(0, Random.Range(0,360),0)));
                        } else if (Random.Range(0, 1f) > .75f)
                        {
                            grounds.Add(Instantiate(innerDoodads[Random.Range(0, innerDoodads.Length)], new Vector3(x, map[x,y], y ), Quaternion.Euler(0, Random.Range(0,360),0)));
                        }
                    }
                    
//
//                    if (isEde)
//                    {
//                        grounds.Add(Instantiate(groundEdgePrefabs[Random.Range(0, groundEdgePrefabs.Length)], new Vector3(x, -Random.Range(.01f, .25f), y ), Quaternion.identity));
//                    }
//                    else
//                    {
//                        grounds.Add(Instantiate(groundMainsPrefab[Random.Range(0, groundMainsPrefab.Length)], new Vector3(x, 0, y ), Quaternion.identity));
//                    }
                    
//                    groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
                }
                else
                {
                    bool isBesideLand = besideLand(map, x, y);
                    if (isBesideLand)
                    {
                        if (Random.Range(0, 1f) > .80f)
                        {
                            grounds.Add(Instantiate(waterBesideLand[Random.Range(0, waterBesideLand.Length)], new Vector3(x, -.8f, y ), Quaternion.Euler(0, Random.Range(0,360),0)));
                        }
                    }
                    else
                    {
                        
                    }
//                    groundTilemap.SetTile(new Vector3Int(x, y, 0), null);
                    if (Random.Range(0, 1f) > .999f)
                    {
                        grounds.Add(Instantiate(waterDoodads[Random.Range(0, waterDoodads.Length)], new Vector3(x, -1.4f, y ), Quaternion.Euler(0, Random.Range(0,360),0)));
                    }
                    
                }
            }
        }
    }

    private bool isEdge(int[,] map, int x, int y)
    {
        if (
            (x > 0 && map[x - 1, y] == 0) || 
            (x < map.GetLength(0) && map[x + 1, y] == 0) || 
            (y > 0 && map[x, y - 1] == 0) || 
            (y < map.GetLength(1) && map[x, y + 1] == 0))
        {
            return true;
        }

        return false;
    }
    
    private bool besideLand(int[,] map, int x, int y)
    {
        if (
            (x > 0 && map[x - 1, y] == 1) || 
            (x < map.GetLength(0) - 1 && map[x + 1, y] == 1) || 
            (y > 0 && map[x, y - 1] == 1) || 
            (y < map.GetLength(1) - 1 && map[x, y + 1] == 1))
        {
            return true;
        }

        return false;
    }

    private void applyIslandToMap(int[,] map, IslandData islandData)
    {
        for (int x = 0; x < islandData.width; x++)
        {
            for (int y = 0; y < islandData.height; y++)
            {
                if (islandData.data[x, y] > 0)
                {
                    map[x + islandData.xOffset, y + islandData.yOffset] = islandData.data[x, y];
                }
            }
        }
    }

    private IslandData generateIslandData()
    {
        for (int attempts = 0; attempts < 10; attempts++)
        {
            int width = Random.Range(minIslandSize, maxIslandSize);
            int height = width; //Random.Range(32, 32);

            int[,] noise = generateNoise(width, height);
            noise = trimNoise(noise, width, height);
            if (noise == null)
            {
                continue;
            }

            IslandData islandData = new IslandData(noise.GetLength(0), noise.GetLength(1), noise);
            return islandData;
        }

        return null;
    }

    private int[,] trimNoise(int[,] noise, int baseWidth, int baseHeight)
    {
        Vector2Int min = new Vector2Int(9999, 9999);
        Vector2Int max = new Vector2Int(-1000, -1000);
        for (int x = 0; x < baseWidth; x++)
        {
            for (int y = 0; y < baseHeight; y++)
            {
                if (noise[x, y] > 0)
                {
                    if (x < min.x)
                    {
                        min.x = x;
                    }

                    if (x > max.x)
                    {
                        max.x = x;
                    }

                    if (y < min.y)
                    {
                        min.y = y;
                    }

                    if (y > max.y)
                    {
                        max.y = y;
                    }
                }
            }
        }

        if (min.x == 9999 || max.y == -1000)
        {
            return null;
        }

        int[,] newNoise = new int[max.x - min.x + 4, max.y - min.y + 4];

        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                newNoise[x - min.x + 2, y - min.y + 2] = noise[x, y];
            }
        }

        return newNoise;
    }

    public int[,] generateNoise(int width, int height)
    {
        float[,] noise = Noise.GenerateNoiseMap(width, height, Random.Range(5, 9999999), noiseSettings, new Vector2(width / 2f, height / 2f));
        noise = Noise.distanceFalloff(noise, distanceFalloff);
//        float[,] falloffMap = Noise.generateFalloffMap(width, height);
//        Noise.applyNoiseMap(noise, falloffMap, distanceFalloff);
//        Noise.overlayNoiseMap(noise, falloffMap, distanceFalloff, 0, 0);
        int[,] intNoiseMap = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (noise[x, y] > minHeightForLand)
                {
                    intNoiseMap[x, y] = (int)(1 + (noise[x,y] - minHeightForLand) * 10);
                }
                else
                {
                    intNoiseMap[x, y] = 0;
                }
            }
        }

        return intNoiseMap;
    }

    private class IslandData
    {
        public int[,] data;
        public int width;
        public int height;
        public int xOffset;
        public int yOffset;

        public IslandData(int width, int height, int[,] data)
        {
            this.data = data;
            this.width = width;
            this.height = height;
            this.bounds = new RectInt(0, 0, width, height);
        }

        public RectInt bounds;
    }
}