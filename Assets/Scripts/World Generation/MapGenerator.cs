using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum eTileType { DeepWater, ShallowWater, Grass, Rock, Snow, Sand, Ice, PlainsGrass, JungleGrass, Tree, Mountain, Swamp };
public enum eBiomeID { None = -1, Forest, Jungle, Plains, Desert, Tundra, Swamp };

[System.Serializable]
public class Biome
{
    [SerializeField] private eBiomeID mBiomeID;
    [SerializeField] private float mVegetationModifier;

    [SerializeField] private Tile mPrimaryTerrainTile;
    [SerializeField] private Tile mSecondaryTerrainTile;

    [SerializeField] private Tile mPrimaryVegetationTile;
    [SerializeField] private Tile mSecondaryVegetationTile;

    public eBiomeID ID { get { return mBiomeID; } }
    public float VegetationModifier { get { return mVegetationModifier; } }
    public Tile PrimaryTerrainTile { get { return mPrimaryTerrainTile; } }
    public Tile SecondaryTerrainTile { get { return mSecondaryTerrainTile; } }
    public Tile PrimaryTreeTile { get { return mPrimaryVegetationTile; } }
    public Tile SecondaryTreeTile { get { return mSecondaryVegetationTile; } }
}

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap mWaterTileMap;
    [SerializeField] private Tilemap mTerrainTileMap;
    [SerializeField] private Tilemap mPlantTileMap;

    [SerializeField] private Biome[] mBiomes;

    [SerializeField] private Tile[] mTiles;
    [SerializeField] private int mMapSize = 10;

    // DEBUG
    private Vector3Int DebugPlayerPos;
    private TileBase DebugCurrentTile;
    private eBiomeID DebugCurrentBiome;

    // use OnEnable here because it's after Awake (to give VSEventManager time to setup) and before Start
    private void OnEnable()
    {
        Debug.Log("Adding Listener for Request");
        VSEventManager.Instance.AddListener<GameEvents.RequestWorldGenEvent>(GenerateWorld);
        VSEventManager.Instance.AddListener<GameEvents.PlayerPositionUpdateEvent>(PlayerPosUpdated);
    }

    private void GenerateWorld(GameEvents.RequestWorldGenEvent e)
    {
        Debug.Log("Generating...");
        int seed = UnityEngine.Random.Range(0, int.MaxValue);
        Debug.LogFormat("World Seed: {0}", seed);

        BuildOcean(seed);
        BuildTerrain(seed);
        BuildBiomes(seed);

        e.Callback();
    }

    private void PlayerPosUpdated(GameEvents.PlayerPositionUpdateEvent e)
    {
        DebugPlayerPos = mTerrainTileMap.WorldToCell(e.WorldPosition);
        DebugCurrentTile = mTerrainTileMap.GetTile(DebugPlayerPos);

        if (DebugCurrentTile != null)
        {
            // LOL UGH
            switch (DebugCurrentTile.name)
            {
                case "grass_0":
                    DebugCurrentBiome = eBiomeID.Forest;
                    break;

                case "grass_1":
                    DebugCurrentBiome = eBiomeID.Plains;
                    break;

                case "grass_2":
                    DebugCurrentBiome = eBiomeID.Jungle;
                    break;

                case "sand_0":
                    DebugCurrentBiome = eBiomeID.Desert;
                    break;

                case "snow_0":
                case "ice_water":
                    DebugCurrentBiome = eBiomeID.Tundra;
                    break;

                case "swamp_0":
                    DebugCurrentBiome = eBiomeID.Swamp;
                    break;

                default:
                    DebugCurrentBiome = eBiomeID.None;
                    break;
            }
        }
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 40), "Terrain Pos: " + DebugPlayerPos);
        GUI.Label(new Rect(10, 30, 300, 40), "Tile: " + (DebugCurrentTile != null ? DebugCurrentTile.name : "NULL"));
        GUI.Label(new Rect(10, 50, 300, 40), "Biome: " + (DebugCurrentBiome != eBiomeID.None ? DebugCurrentBiome.ToString() : "---"));
    }
#endif

    private void BuildOcean(int seed)
    {
        int halfMap = mMapSize / 2;
        Tile currentTile = null;

        PerlinGenerator oceanGen = new PerlinGenerator();
        int oceanOctaves = 5;
        float[][] oceanNoise = oceanGen.Generate(seed, oceanOctaves, mMapSize);

        for (int y = 0; y < mMapSize; y++)
        {
            for (int x = 0; x < mMapSize; x++)
            {
                float height = oceanNoise[x][y];
                if (height < 0.35f)
                {
                    currentTile = mTiles[(int)eTileType.DeepWater];
                }
                else
                {
                    currentTile = mTiles[(int)eTileType.ShallowWater];
                }

                int worldPosX = x - halfMap;
                int worldPosY = y - halfMap;
                Vector3Int pos = new Vector3Int(worldPosX, worldPosY, 0);

                mWaterTileMap.SetTile(pos, currentTile);
            }
        }
    }

    private void BuildTerrain(int seed)
    {
        int halfMap = mMapSize / 2;
        Tile currentTile = null;

        PerlinGenerator terrainGen = new PerlinGenerator();
        int terrainOctaves = 5;
        float[][] terrainNoise = terrainGen.Generate(seed, terrainOctaves, mMapSize);

        for (int y = 0; y < mMapSize; y++)
        {
            for (int x = 0; x < mMapSize; x++)
            {
                float height = terrainNoise[x][y];
                if (height < 0.45f)
                {
                    continue; // don't cover up water
                }
                else if (height < 0.55f)
                {
                    currentTile = mTiles[(int)eTileType.Sand];
                }
                else if (height < 0.725f)
                {
                    currentTile = mTiles[(int)eTileType.Grass];
                }
                else if (height < 0.775f)
                {
                    currentTile = mTiles[(int)eTileType.Rock];
                }
                else if (height < 0.865f)
                {
                    currentTile = mTiles[(int)eTileType.Mountain];
                }
                else
                {
                    currentTile = mTiles[(int)eTileType.Snow];
                }

                int worldPosX = x - halfMap;
                int worldPosY = y - halfMap;
                Vector3Int pos = new Vector3Int(worldPosX, worldPosY, 0);

                mTerrainTileMap.SetTile(pos, currentTile);
            }
        }
    }

    private void BuildBiomes(int seed)
    {
        int halfMap = mMapSize / 2;
        Tile currentTerrtainTile = null;
        System.Random rand = new System.Random(seed);

        PerlinGenerator moistureGen = new PerlinGenerator();
        int moistOctaves = 8;
        float[][] moistureNoise = moistureGen.Generate(rand.Next(), moistOctaves, mMapSize);

        PerlinGenerator temperatureGen = new PerlinGenerator();
        int tempOctaves = 9;
        float[][] tempNoise = temperatureGen.Generate(rand.Next(), tempOctaves, mMapSize);

        PerlinGenerator vegetationGen = new PerlinGenerator();
        int vegOctaves = 1;
        float[][] vegNoise = vegetationGen.Generate(rand.Next(), vegOctaves, mMapSize);

        for (int y = 0; y < mMapSize; y++)
        {
            for (int x = 0; x < mMapSize; x++)
            {
                int worldPosX = x - halfMap;
                int worldPosY = y - halfMap;
                Vector3Int pos = new Vector3Int(worldPosX, worldPosY, 0);
                TileBase existingWaterTile = mWaterTileMap.GetTile(pos);
                TileBase existingTerrainTile = mTerrainTileMap.GetTile(pos);

                Biome currentBiome = GetBiome(moistureNoise[x][y], tempNoise[x][y]);

                // don't modify certain tiles
                if ((existingTerrainTile == null && currentBiome.ID != eBiomeID.Tundra) ||
                    existingTerrainTile == mTiles[(int)eTileType.Rock] ||
                    existingTerrainTile == mTiles[(int)eTileType.Mountain] ||
                    existingTerrainTile == mTiles[(int)eTileType.Snow] )
                {
                    continue;
                }

                // tile from biome
                currentTerrtainTile = currentBiome.PrimaryTerrainTile;
                if (currentBiome.ID == eBiomeID.Tundra)
                {
                    if (existingTerrainTile == null)
                    {
                        // over water
                        if (existingWaterTile == mTiles[(int)eTileType.ShallowWater])
                        {
                            currentTerrtainTile = currentBiome.SecondaryTerrainTile;
                        }
                        else
                        {
                            // deep water
                            currentTerrtainTile = null;
                        }
                    }
                }

                // vegetation from biome
                if (existingTerrainTile != null && vegNoise[x][y] > 1f - currentBiome.VegetationModifier)
                {
                    mPlantTileMap.SetTile(pos, currentBiome.PrimaryTreeTile);
                }

                mTerrainTileMap.SetTile(pos, currentTerrtainTile);
            }
        }
    }

    // TODO
    // maybe change all of this to weight average? (see Procedural_Dungeon project Utils.cs)
    private Biome GetBiome(float moistureValue, float tempValue)
    {
        if (tempValue < 0.25f)       // cold
        {
            return mBiomes[(int)eBiomeID.Tundra];
        }
        else if (tempValue < 0.45f)  // cool
        {
            if (moistureValue < 0.35f)
            {
                return mBiomes[(int)eBiomeID.Desert];
            }
            else if (moistureValue < 0.75f)
            {
                return mBiomes[(int)eBiomeID.Plains];
            }
            else
            {
                return mBiomes[(int)eBiomeID.Tundra];
            }
        }
        else if (tempValue < 0.6f)  // neutral
        {
           if (moistureValue < 0.75f)
            {
                return mBiomes[(int)eBiomeID.Forest];
            }
            else
            {
                return mBiomes[(int)eBiomeID.Plains];
            }
        }
        else if (tempValue < 0.75f)  // warm
        {
            if (moistureValue < 0.5f)
            {
                return mBiomes[(int)eBiomeID.Jungle];
            }
            else
            {
                return mBiomes[(int)eBiomeID.Swamp];
            }
        }
        else                        // hot
        {
            return mBiomes[(int)eBiomeID.Desert];
        }
    }
}
