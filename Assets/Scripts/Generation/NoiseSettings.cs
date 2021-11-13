using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public NoiseSettings()
    {
    }

    public Noise.NormalizeMode normalizeMode;
    public float scale = 50;
    public int octaves = 6;
    [Range(0, 1)] public float persistance = .6f;
    public float lacunarity = 2;
    public int seedOffset;
    public Vector2 offset;
}