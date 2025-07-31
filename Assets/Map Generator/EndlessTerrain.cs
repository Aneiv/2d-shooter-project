using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Windows;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 1f; //how often chunks update
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public static float maxViewDst;
    public float scrollSpeed;
    public Material tileMaterial;
    [Header("Chunk render distance")]
    public int viewDistanceChunksX = 0;
    public int viewDistanceChunksY = 1;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    Vector3 worldOffset;//center of screen
    private Vector3 offsetAhead = Vector2.down * -10f; //Y-offset to make chunks spawn faster in fromt of player and disapper faster behind


    public Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();//dictionary of all chunks on map
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
    public List<ObjectSpawner> objectsSpawner;
    Queue<TerrainChunk> chunkPool = new Queue<TerrainChunk>(); //queue for TerrainChunk pool

    void Start()
    {
        mapGenerator = FindFirstObjectByType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        maxViewDst = chunkSize * viewDistanceChunksY;
        //mapGenerator.seed = Random.Range(0, 10000);//random seed for map at game start

        UpdateVisibleChunks();
    }

    void FixedUpdate()
    {
        Vector3 scroll = Vector2.down * scrollSpeed * Time.deltaTime;
        worldOffset -= scroll;
        viewerPosition = worldOffset + offsetAhead;
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
        //move chunks
        foreach (var chunk in terrainChunkDictionary.Values)
        {
            if (chunk.IsVisible())
                chunk.UpdatePositionRelativeToViewer(worldOffset);
        }
    }
    //get chunk on which 'prefab' object is placed (its parent)
    public TerrainChunk GetCurrentChunkByChild(GameObject prefab)
    {
        Vector2 chunkVector = ParseVector2FromString(prefab.transform.parent.name);
        terrainChunkDictionary.TryGetValue(chunkVector, out TerrainChunk chunk);
        return chunk;
    }
    //parse chunk name to vector2 from string
    public static Vector2 ParseVector2FromString(string s)
    {
        string[] parts = s.Split('|');

        if (parts.Length != 2)
        {
            Debug.LogError("cant format to Vector2: " + s);
            return Vector2.zero;
        }

        if (float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y))
        {
            return new Vector2(x, y);
        }
        else
        {
            Debug.LogError("cant parse: " + s);
            return Vector2.zero;
        }
    }
    //update chunks (spawn, activate, chunk pool control)
    void UpdateVisibleChunks()
    {
        foreach (var chunk in terrainChunksVisibleLastUpdate)
        {
            chunk.SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        //visible chunks
        HashSet<Vector2> currentlyVisibleCoords = new HashSet<Vector2>();

        for (int yOffset = -viewDistanceChunksY; yOffset <= viewDistanceChunksY; yOffset++)
        {
            for (int xOffset = -viewDistanceChunksX; xOffset <= viewDistanceChunksX; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                currentlyVisibleCoords.Add(viewedChunkCoord);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    TerrainChunk chunk;
                    if (chunkPool.Count > 0) //check if there is free chunk in pool
                    {
                        chunk = chunkPool.Dequeue();
                        chunk.Reuse(viewedChunkCoord, chunkSize, transform, tileMaterial, objectsSpawner);
                    }
                    else
                    {
                        chunk = new TerrainChunk(viewedChunkCoord, chunkSize, transform, tileMaterial, objectsSpawner);
                    }
                    terrainChunkDictionary.Add(viewedChunkCoord, chunk);
                }
            }
        }
        // enqueue not visible chunks
        List<Vector2> keysToRemove = new List<Vector2>();
        foreach (var kvp in terrainChunkDictionary)
        {
            if (!currentlyVisibleCoords.Contains(kvp.Key))
            {
                kvp.Value.Deactivate();
                chunkPool.Enqueue(kvp.Value);//return to pool
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            terrainChunkDictionary.Remove(key);
        }

    }
    //Class for TerrainChunk gameObject
    public class TerrainChunk
    {
        GameObject chunkObject;
        List<ObjectSpawner> objectSpawner;
        Vector2 position;
        Rect bounds;

        //dictionary for local(per chunk) pool with gameObjects to spawn on that chunk
        private Dictionary<GameObject, Queue<GameObject>> localObjectPools = new Dictionary<GameObject, Queue<GameObject>>();
        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material, List<ObjectSpawner> objectSpawner)
        {
            this.objectSpawner = objectSpawner;
            Vector2 offset = new Vector2(size / 2, 0f);
            Vector2 centerPosition = coord * size + offset;
            position = centerPosition - Vector2.one * size / 2f;
            bounds = new Rect(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(centerPosition.x, centerPosition.y, 0);
            chunkObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            chunkObject.name = $"{coord.x}|{coord.y}";
            chunkObject.transform.position = positionV3;
            chunkObject.transform.rotation = Quaternion.identity;
            chunkObject.transform.localScale = new Vector3(size, size, 1f);
            chunkObject.transform.parent = parent;

            chunkObject.GetComponent<MeshRenderer>().material = new Material(material);
            SetVisible(false);

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }
        //spawn objects on chunk map (from local pool per every chunk)
        void OnMapDataReceived(MapData mapData)
        {
            if (chunkObject == null) return;

            Texture2D texture = TextureGenerator.TextureFromColourMap(
                mapData.colourMap,
                mapGenerator.mapChunkResolution,
                mapGenerator.mapChunkResolution
            );
            chunkObject.GetComponent<MeshRenderer>().material.mainTexture = texture;

            ClearSpawnedObjects();

            int resolution = mapGenerator.mapChunkResolution;
            int step = 8; //map sampling
            for (int y = 0; y < resolution; y += step)
            {
                for (int x = 0; x < resolution; x += step)
                {
                    Color tileColor = mapData.colourMap[y * resolution + x];

                    for (int i = 0; i < objectSpawner.Count; i++)
                    {
                        if (CheckColorSimilarity(tileColor, objectSpawner[i].colours) && Random.value < objectSpawner[i].objectSpawnChance)
                        {
                            float chunkScale = chunkObject.transform.localScale.x;
                            Vector3 localOffset = new Vector3(
                                ((float)x / resolution - 0.5f) * chunkScale,
                                ((float)y / resolution - 0.5f) * chunkScale,
                                0f
                            );
                            Vector3 spawnPos = chunkObject.transform.position + localOffset;
                            //get object from pool to spawn
                            GameObject spawnObject = GetPooledObject(objectSpawner[i].objectPrefab);

                            spawnObject.transform.position = spawnPos;
                            spawnObject.transform.rotation = Quaternion.identity;
                            spawnObject.transform.parent = chunkObject.transform;//new object is child of chunk in which is placed

                            break; //so not to spawn 2 or more object in one place
                        }
                    }
                }
            }
            UpdateTerrainChunk();
        }

        //get chunk from local pool or create new if pool is empty
        private GameObject GetPooledObject(GameObject prefab)
        {
            if (!localObjectPools.ContainsKey(prefab))
                localObjectPools[prefab] = new Queue<GameObject>();

            if (localObjectPools[prefab].Count > 0)
            {
                GameObject obj = localObjectPools[prefab].Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                //create new if pool is empty
                return GameObject.Instantiate(prefab);
            }
        }

        //return spawned objects back to pool
        private void ClearSpawnedObjects()
        {
            foreach (Transform child in chunkObject.transform)
            {
                GameObject childObj = child.gameObject;
                childObj.SetActive(false);

                //return to pool
                foreach (var spawner in objectSpawner)
                {
                    if (childObj.name.Contains(spawner.objectPrefab.name))
                    {
                        if (!localObjectPools.ContainsKey(spawner.objectPrefab))
                            localObjectPools[spawner.objectPrefab] = new Queue<GameObject>();

                        localObjectPools[spawner.objectPrefab].Enqueue(childObj);
                        break;
                    }
                }
            }
        }
        //used to add collected (e.g. money) objects to pool
        public void AddObjectToQueue(GameObject prefab)
        {
            //change object active state
            prefab.SetActive(false);

            //spawner searching
            foreach (var spawner in objectSpawner)
            {
                if (prefab.name.Contains(spawner.objectPrefab.name))
                {
                    if (!localObjectPools.ContainsKey(spawner.objectPrefab))
                    {
                        localObjectPools[spawner.objectPrefab] = new Queue<GameObject>();
                    }

                    //add object to queue
                    localObjectPools[spawner.objectPrefab].Enqueue(prefab);
                    break;
                }
            }
        }

        //color similarity check to place object on certain color tiles
        bool CheckColorSimilarity(Color a, Color[] b, float tolerance = 0.02f)
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
        //change visibiliy depending on distance from player
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
        //chunk position update
        public void UpdatePositionRelativeToViewer(Vector2 worldOffset)
        {
            Vector3 positionV3 = new Vector3(position.x - worldOffset.x, position.y - worldOffset.y, 0);
            if (chunkObject != null)
            {
                chunkObject.transform.position = positionV3;
            }
        }
        //deactivate chunk
        public void Deactivate()
        {
            ClearSpawnedObjects();
            if (chunkObject != null)
            {
                chunkObject.SetActive(false);
            }
        }
        //distance calculation
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

        //reuse chunk
        public void Reuse(Vector2 coord, int size, Transform parent, Material material, List<ObjectSpawner> objectSpawner)
        {
            this.objectSpawner = objectSpawner;

            Vector2 offset = new Vector2(size / 2, 0f);
            Vector2 centerPosition = coord * size + offset;
            position = centerPosition - Vector2.one * size / 2f;
            bounds = new Rect(position, Vector2.one * size);

            chunkObject.transform.position = new Vector3(centerPosition.x, centerPosition.y, 0);
            chunkObject.transform.localScale = new Vector3(size, size, 1f);
            chunkObject.transform.parent = parent;
            chunkObject.name = $"{coord.x}|{coord.y}";

            chunkObject.SetActive(true);

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }
    }

}

[System.Serializable]
public struct ObjectSpawner
{
    public GameObject objectPrefab;
    public float objectSpawnChance;
    public Color[] colours;
}