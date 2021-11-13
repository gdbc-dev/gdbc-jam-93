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
        for (int i = 0; i < 25; i++)
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

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (map[x, y] > 0)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
                }
                else
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
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
        Vector2Int min = new Vector2Int(9999,9999);
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
        
        int[,] newNoise = new int[max.x - min.x, max.y - min.y];

        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                newNoise[x - min.x, y - min.y] = noise[x, y];
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
                    intNoiseMap[x, y] = 1;
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