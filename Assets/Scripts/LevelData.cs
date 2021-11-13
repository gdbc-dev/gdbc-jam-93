using System;
using System.Collections.Generic;

[Serializable]
public class LevelData
{
    public int worldSize = 128;
    public int numIslands = 35;

    public int numShips = 3;
    public int numDolphins = 3;
    public List<TouristData> touristSpawns;
}

[Serializable]
public class TouristData
{
    public float spawnDelay;
}