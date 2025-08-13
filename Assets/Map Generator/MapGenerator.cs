using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap, Mesh };
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 15;
    [Range(0, 6)]
    public int editorPreviewLOD;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public int mapChunkResolution = 121;
    public bool autoUpdate;
    
    public List<TerrainType> terrainRegions;

    [Header("GradientAndShader")]
    int gradientStep = 5; // colors between start and end color
    float thetaSun = 60f; // the azimuth angle(the angle between the geographic direction [North])
    float phiSun = 20f; // the elevation angle(the angle above the surface)

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay display = FindFirstObjectByType<MapDisplay>(); 

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkResolution, mapChunkResolution));
        }
        /*else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(
                MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD),
                TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize)
            );
        }*/
    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(
            mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod
        );
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void FixedUpdate()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }
    //Drawing textures
    MapData GenerateMapData(Vector2 centre)
    {
        Vector2 scaledCentre = (centre + offset) / (mapChunkSize-1) * mapChunkResolution;

        float[,] noiseMap = Noise.GenerateNoiseMap(
            mapChunkResolution, mapChunkResolution, seed, noiseScale,
            octaves, persistance, lacunarity, scaledCentre, normalizeMode
        );

        Color[] colourMap = new Color[mapChunkResolution * mapChunkResolution];

        // sun and shade details

        float thetaRadSun = thetaSun * Mathf.Deg2Rad;
        float phiRadSun = phiSun * Mathf.Deg2Rad;


        for (int y = 0; y < mapChunkResolution; y++)
        {
            for (int x = 0; x < mapChunkResolution; x++)
            {
                float currentHeight = noiseMap[x, y];

                //for (int i = 0; i < regions.Count; i++)
                //{
                //    if (currentHeight <= regions[i].height)
                //    {
                //        int flippedY = mapChunkResolution - 1 - y;
                //        colourMap[flippedY * mapChunkResolution + x] = regions[i].colour;
                //        break;
                //    }
                //}

                AddGradient(colourMap, currentHeight, x, y, gradientStep);
                AddShaders(colourMap, noiseMap, currentHeight, x, y, phiRadSun, phiRadSun);
                
            }
        }

        return new MapData(noiseMap, colourMap);
    }

    void AddGradient(Color[] colourMap, float currentHeight, int x, int y, int step = 5)
    {
        int flippedY = mapChunkResolution - 1 - y;
        for (int i = 0; i < terrainRegions.Count; i++)
        {
            float min = i == 0 ? 0f : terrainRegions[i - 1].height;
            float max = terrainRegions[i].height;

            if (currentHeight >= min && currentHeight <= max)
            {
                Color colorStart = terrainRegions[i].colorStart;
                Color colorEnd = terrainRegions[i].colorEnd;

                float normalizedHeight = (currentHeight - min) / (max - min);
                normalizedHeight = Mathf.Ceil(normalizedHeight * step) / step;
                normalizedHeight = Mathf.Clamp01(normalizedHeight);
                Color blendColor = Color.Lerp(colorStart, colorEnd, normalizedHeight);

                colourMap[flippedY * mapChunkResolution + x] = blendColor;
                break;
            }
        }
    }

    void AddShaders(Color[] colourMap, float[,] noiseMap,  float currentHeight, int x, int y,
        float phiRad,float thetaRad)
    {
        int flippedY = mapChunkResolution - 1 - y;

        float slope;
        float aspect;
        float brightness;
        float waterHeight = terrainRegions[0].height;
        // shading
        if (currentHeight > waterHeight)
        {
            // on border of chunks
            int xm1 = Mathf.Max(x - 1, 0);
            int xp1 = Mathf.Min(x + 1, mapChunkResolution - 1);
            int ym1 = Mathf.Max(y - 1, 0);
            int yp1 = Mathf.Min(y + 1, mapChunkResolution - 1);

            float dzdx = ((noiseMap[xm1, y] - noiseMap[xp1, y]) / 2f) * mapChunkResolution;
            float dzdy = ((noiseMap[x, ym1] - noiseMap[x, yp1]) / 2f) * mapChunkResolution;

            slope = Mathf.Atan(Mathf.Sqrt(dzdx * dzdx + dzdy * dzdy));

            aspect = Mathf.Atan2(dzdy, -dzdx);

            brightness = Mathf.Cos(phiRad) * Mathf.Cos(slope) + Mathf.Sin(phiRad) * Mathf.Sin(slope) * Mathf.Cos(thetaRad - aspect);
            float normalizedBrightness = (brightness + 1) / 2f;
            normalizedBrightness = Mathf.Pow(normalizedBrightness, 1f / 3f);
            if (normalizedBrightness > 0.8f) normalizedBrightness = Mathf.Pow(normalizedBrightness, 1f / 2f);

            Color currColor = colourMap[flippedY * mapChunkResolution + x];
            Color shadedColor = Color.Lerp(Color.black, currColor, normalizedBrightness);

            colourMap[flippedY * mapChunkResolution + x] = shadedColor;
        }
        else
        {
            Color currColor = colourMap[flippedY * mapChunkResolution + x];
            Color shadedColor = Color.Lerp(Color.black, currColor, 0.9f);

            colourMap[flippedY * mapChunkResolution + x] = shadedColor;
        }
    }

    void OnValidate()
    {
        if (lacunarity < 1)
            lacunarity = 1;
        if (octaves < 0)
            octaves = 0;
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colorStart;
    public Color colorEnd;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;

    public MapData(float[,] heightMap, Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}
