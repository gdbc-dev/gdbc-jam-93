using UnityEngine;

public class Noise
{
    public enum NormalizeMode
    {
        Local,
        Global
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, NoiseSettings settings,
        Vector2 sampleCentre)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed + settings.seedOffset);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
            float offsetY = prng.Next(-100000, 100000) + settings.offset.y + sampleCentre.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistance;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistance;
                    frequency *= settings.lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }

                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;

                if (settings.normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
                    normalizedHeight = normalizedHeight / maxPossibleHeight; // Normalize to be 0-1
                    noiseMap[x, y] = normalizedHeight;
                }
            }
        }

        if (settings.normalizeMode == NormalizeMode.Local)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
            }
        }

        return noiseMap;
    }

    public static float[,] convertNoiseToHeightMap(float[,] baseMap, float baseHeight, float maxHeight)
    {
        int mapWidth = baseMap.GetLength(0);
        int mapHeight = baseMap.GetLength(1);
        
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                baseMap[x, y] = baseHeight + baseMap[x, y] * maxHeight;
            }
        }

        return baseMap;
    }

    public static float[,] distanceFalloff(float[,] baseMap, AnimationCurve curve)
    {
        int mapWidth = baseMap.GetLength(0);
        int mapHeight = baseMap.GetLength(1);
        int mapCenterX = mapWidth / 2;
        int mapCenterY = mapHeight / 2;
        Vector2 mapCenter = new Vector2(mapCenterX, mapCenterY);
//        float dist;
//        float val; 
//        float maxDistance = Vector2.Distance(new Vector2(mapWidth, 0), mapCenter);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float xx = x / (float) mapWidth * 2 - 1;
                float yy = y / (float) mapHeight * 2 - 1;
                float value = Mathf.Max(Mathf.Abs(xx), Mathf.Abs(yy));
//                dist = Vector2.Distance(new Vector2(x, y), mapCenter);
//                val = 0;
//                val -= (dist / maxDistance) * power;

//                baseMap[x, y] += val;

                baseMap[x, y] *= curve.Evaluate(value);
            }
        }

        return baseMap;
    }

    public static float[,] distanceFalloff(float[,] baseMap, float power)
    {
        int mapWidth = baseMap.GetLength(0);
        int mapHeight = baseMap.GetLength(1);
        int mapCenterX = mapWidth / 2;
        int mapCenterY = mapHeight / 2;
        Vector2 mapCenter = new Vector2(mapCenterX, mapCenterY);
        float dist;
//        float val; 
        float maxDistance = Vector2.Distance(new Vector2(mapWidth, 0), mapCenter) - 10;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                dist = Vector2.Distance(new Vector2(x, y), mapCenter);
//                val = 0;
//                val -= (dist / maxDistance) * power;

//                baseMap[x, y] += val;

                baseMap[x, y] -= baseMap[x, y] * (dist / maxDistance) * power;
            }
        }

        return baseMap;
    }

    public static float[,] overlayNoiseMap(float[,] baseMap, float[,] overlayMap, float weight, int baseOffsetX, int baseOffsetY)
    {
        int mapWidth = overlayMap.GetLength(0);
        int mapHeight = overlayMap.GetLength(1);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                baseMap[x + baseOffsetX, y + baseOffsetY] = baseMap[x + baseOffsetX, y + baseOffsetY] + (baseMap[x + baseOffsetX, y + baseOffsetY] * (overlayMap[x, y] * weight));
            }
        }

        return baseMap;
    }

    public static float[,] applyNoiseMap(float[,] baseMap, float[,] overlayMap, float weight)
    {
        int mapWidth = baseMap.GetLength(0);
        int mapHeight = baseMap.GetLength(1);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                baseMap[x, y] *= overlayMap[x, y] * weight;
            }
        }

        return baseMap;
    }

    public static float[,] generateFalloffMap(int width, int height)
    {
        float[,] map = new float[width, height];
        Vector2 center = new Vector2(width/2f, height/2f);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = 1 - Vector2.Distance(new Vector2(x, y), center) / (width/2f);
//                float xx = x / (float) width * 2 - 1;
//                float yy = y / (float) height * 2 - 1;
//                float value = 1 - Mathf.Max(Mathf.Abs(xx), Mathf.Abs(yy));
                map[x, y] = value;
            }
        }

        return map;
    }

    public static float[,] overlayNoiseMap(float[,] baseMap, float[,] overlayMap, float weight, float minToApply, AnimationCurve curve)
    {
        int mapWidth = baseMap.GetLength(0);
        int mapHeight = baseMap.GetLength(1);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (baseMap[x, y] >= minToApply)
                {
                    baseMap[x, y] = baseMap[x, y] + (baseMap[x, y] * (overlayMap[x, y] * weight * curve.Evaluate(overlayMap[x, y])));
                }
            }
        }

        return baseMap;
    }
}