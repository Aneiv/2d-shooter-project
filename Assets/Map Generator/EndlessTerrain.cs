using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Windows;

public class EndlessTerrain : MonoBehaviour
{
    const float scale = 1f;

    const float viewerMoveThresholdForChunkUpdate = 0.2f; //how often chunks update
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public static float maxViewDst;

    //public Transform viewer;
    public Material tileMaterial;
    public int viewDistanceChunks = 3;
    public float scrollSpeed;
    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    Vector3 worldOffset;//center of screen

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    //[Header("Objects spawn settings")]
    //public GameObject[] objectsPrefabs;
    //public float[] objectSpawnChances;
    public List<ObjectSpawner> objectsSpawner;

    void Start()
    {
        mapGenerator = FindFirstObjectByType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        maxViewDst = chunkSize * viewDistanceChunks;
        //mapGenerator.seed = Random.Range(0, 10000);//random seed for map at game start
        UpdateVisibleChunks();
    }

    void Update()
    {
        Vector3 scroll = Vector2.down * scrollSpeed * Time.deltaTime;
        worldOffset -= scroll;
        //Debug.Log(worldOffset);
        viewerPosition = worldOffset; //new Vector2(viewer.position.x, viewer.position.y) / scale;

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
        //move chunks
        foreach (var chunk in terrainChunkDictionary.Values)
        {
            chunk.UpdatePositionRelativeToViewer(worldOffset);
        }
    }

    void UpdateVisibleChunks()
    {
        foreach (var chunk in terrainChunksVisibleLastUpdate)
        {
            chunk.SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        //Debug.DrawLine(Vector3.zero, new Vector3(viewerPosition.x, viewerPosition.y, 0), Color.red, 0.5f);
        //Debug.Log($"Player pos: {viewerPosition}, chunk: ({currentChunkCoordX}, {currentChunkCoordY})");

        //visible chunks
        HashSet<Vector2> currentlyVisibleCoords = new HashSet<Vector2>();

        for (int yOffset = -viewDistanceChunks; yOffset <= viewDistanceChunks; yOffset++)
        {
            for (int xOffset = -viewDistanceChunks; xOffset <= viewDistanceChunks; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                currentlyVisibleCoords.Add(viewedChunkCoord);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, tileMaterial, objectsSpawner));
                }
            }
        }
        //delete not visible chunks
        List<Vector2> keysToRemove = new List<Vector2>();
        foreach (var kvp in terrainChunkDictionary)
        {
            if (!currentlyVisibleCoords.Contains(kvp.Key))
            {
                kvp.Value.Destroy();
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            terrainChunkDictionary.Remove(key);
        }
    }

    public class TerrainChunk
    {
        GameObject chunkObject;
        List<ObjectSpawner> objectSpawner;
        Vector2 position;
        Rect bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material, List<ObjectSpawner> objectSpawner)
        {
            this.objectSpawner = objectSpawner;
            Vector2 offset = new Vector2(15f, 0f); //offset for chunk placement
            Vector2 centerPosition = coord * size + offset;
            position = centerPosition - Vector2.one * size / 2f;
            bounds = new Rect(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(centerPosition.x, centerPosition.y, 0);
            chunkObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            chunkObject.name = $"Chunk {coord.x}, {coord.y}";
            chunkObject.transform.position = positionV3;
            chunkObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            chunkObject.transform.localScale = new Vector3(size, size, 1f);//Vector3.one * size;
            chunkObject.transform.parent = parent;

            chunkObject.GetComponent<MeshRenderer>().material = new Material(material);
            SetVisible(false);

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            Texture2D texture = TextureGenerator.TextureFromColourMap(
                mapData.colourMap,
                mapGenerator.mapChunkResolution,
                mapGenerator.mapChunkResolution
            );
            chunkObject.GetComponent<MeshRenderer>().material.mainTexture = texture;


            //spawn objects on chunk
            int resolution = mapGenerator.mapChunkResolution;
            int step = 8; //map sampling
            for (int y = 0; y < resolution; y+=step)
            {
                for (int x = 0; x < resolution; x+=step)
                {
                    Color tileColor = mapData.colourMap[y * resolution + x];

                    for (int i = 0; i < objectSpawner.Count; i++)
                    {
                        //on certain tile
                        //if (tileColor == objectSpawner[i].colour && Random.value < objectSpawner[i].objectSpawnChance) // chance to spawn
                        if (ChechColorSimilarity(tileColor, objectSpawner[i].colours) && Random.value < objectSpawner[i].objectSpawnChance) // chance to spawn
                        {
                            //scale and spawn on right place
                            float chunkScale = chunkObject.transform.localScale.x;
                            Vector3 localOffset = new Vector3(
                                ((float)x / resolution - 0.5f) * chunkScale,
                                ((float)y / resolution - 0.5f) * chunkScale,
                                0f
                            );
                            Vector3 spawnPos = chunkObject.transform.position + localOffset;

                            //object instantion create
                            GameObject spawnObject = GameObject.Instantiate(objectSpawner[i].obejectPrefab, spawnPos, Quaternion.identity);
                            spawnObject.transform.parent = chunkObject.transform; //new object is child of chunk in which is placed
                            break;//so not to spawn 2 or more object in one place
                        }
                    }
                }
            }
            UpdateTerrainChunk();
        }
        //color similarity check
        bool ChechColorSimilarity(Color a, Color[] b, float tolerance = 0.02f)
        {
            foreach (Color c in b)
            {
                if (Mathf.Abs(a.r - c.r) < tolerance &&
                    Mathf.Abs(a.g - c.g) < tolerance &&
                    Mathf.Abs(a.b - c.b) < tolerance)
                {
                    return true;
                }
            }
            return false;
        }

        public void UpdateTerrainChunk()
        {
            float viewerDstFromEdge = DistanceToRectEdge(EndlessTerrain.viewerPosition, bounds);
            bool visible = viewerDstFromEdge <= EndlessTerrain.maxViewDst;

            SetVisible(visible);
            if (visible)
            {
                terrainChunksVisibleLastUpdate.Add(this);
            }
        }
        public void UpdatePositionRelativeToViewer(Vector2 worldOffset)
        {
            Vector3 positionV3 = new Vector3(position.x - worldOffset.x, position.y - worldOffset.y, 0);
            if (chunkObject != null)
            {
                chunkObject.transform.position = positionV3;
            }
        }


        public void Destroy()
        {
            if (chunkObject != null)
            {
                GameObject.Destroy(chunkObject);
            }
        }

        //calculate distance from point to the edge of rectangle
        float DistanceToRectEdge(Vector2 point, Rect rect)
        {
            float dx = Mathf.Max(rect.xMin - point.x, 0, point.x - rect.xMax);
            float dy = Mathf.Max(rect.yMin - point.y, 0, point.y - rect.yMax);
            if (dx == 0 && dy == 0)
                return 0;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public void SetVisible(bool visible)
        {
            if (chunkObject != null)
                chunkObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return chunkObject.activeSelf;
        }
    }
}

[System.Serializable]
public struct ObjectSpawner
{
    public GameObject obejectPrefab;
    public float objectSpawnChance;
    public Color[] colours;
}