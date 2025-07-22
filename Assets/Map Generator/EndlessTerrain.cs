using UnityEngine;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour
{
    const float scale = 1f;

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public static float maxViewDst;

    public Transform viewer;
    public Material tileMaterial;
    public int viewDistanceChunks = 3;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        mapGenerator = FindFirstObjectByType<MapGenerator>();

        chunkSize = MapGenerator.mapChunkSize - 1;
        maxViewDst = chunkSize * viewDistanceChunks;

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.y) / scale;

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
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

        for (int yOffset = -viewDistanceChunks; yOffset <= viewDistanceChunks; yOffset++)
        {
            for (int xOffset = -viewDistanceChunks; xOffset <= viewDistanceChunks; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, tileMaterial));
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject chunkObject;
        Vector2 position;
        Rect bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
        {
            Vector2 centerPosition = coord * size;
            position = centerPosition - Vector2.one * size / 2f;
            bounds = new Rect(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(centerPosition.x, centerPosition.y, 0);
            chunkObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            chunkObject.name = $"Chunk {coord.x}, {coord.y}";
            chunkObject.transform.position = positionV3;
            chunkObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            chunkObject.transform.localScale = Vector3.one * size;
            chunkObject.transform.parent = parent;

            chunkObject.GetComponent<MeshRenderer>().material = new Material(material);
            SetVisible(false);

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            Texture2D texture = TextureGenerator.TextureFromColourMap(
                mapData.colourMap,
                MapGenerator.mapChunkSize,
                MapGenerator.mapChunkSize
            );

            chunkObject.GetComponent<MeshRenderer>().material.mainTexture = texture;

            UpdateTerrainChunk();
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
