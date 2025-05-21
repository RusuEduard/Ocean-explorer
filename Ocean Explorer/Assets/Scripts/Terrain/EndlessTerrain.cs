using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public Material material;
    public Gradient gradient;
    public const float maxViewDist = 100;
    public Transform viewer;
    public static Vector2 viewerPos;
    int chunkSize;
    int chunksVisibleInViewDst;
    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
    static HashSet<TerrainChunk> terrainChunksVisibleLastUpdate = new HashSet<TerrainChunk>();
    static MarchingCubes mapGenerator;

    void Start()
    {
        chunkSize = MarchingCubes.chunkSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDist / chunkSize);
        mapGenerator = FindObjectOfType<MarchingCubes>();
    }

    private void Update()
    {
        viewerPos = new Vector2(viewer.position.x, viewer.position.z);
        updateVisibleChunks();
    }

    void updateVisibleChunks()
    {
        foreach (var chunk in terrainChunksVisibleLastUpdate)
        {
            var distToChunk = Mathf.Sqrt(chunk.GetBounds().SqrDistance(viewerPos));
            if (distToChunk > maxViewDist)
            {
                chunk.SetVisible(false);
            }
        }

        terrainChunksVisibleLastUpdate.RemoveWhere(x => !x.IsVisible());

        int currentChunkCoordX = Mathf.RoundToInt(viewerPos.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPos.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                ManageChunkAtPosition(viewedChunkCoord);
            }
        }
    }

    private void ManageChunkAtPosition(Vector2 position)
    {
        if (terrainChunkDict.ContainsKey(position))
        {
            terrainChunkDict[position].UpdateTerrainChunk();
        }
        else
        {
            terrainChunkDict.Add(position, new TerrainChunk(position, chunkSize, transform, material, gradient));
        }
    }

    public class TerrainChunk
    {
        TerrainData terrainData;
        Vector2 position;
        Bounds bounds;
        Material material;
        Gradient gradient;
        List<Mesh> allMeshes;
        List<GameObject> allGameObjects;
        Transform parent;
        bool isVisible;

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material mat, Gradient gradient)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            this.material = mat;
            this.gradient = gradient;
            this.allMeshes = new List<Mesh>();
            this.allGameObjects = new List<GameObject>();
            this.parent = parent;

            SetVisible(false);
            mapGenerator.requestTerrainData(positionV3, OnMapDataReceived);
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var chunk = (TerrainChunk)obj;
            return chunk.position == this.position;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return this.position.GetHashCode();
        }

        public void UpdateTerrainChunk()
        {
            float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPos));
            bool visible = viewerDistFromNearestEdge <= maxViewDist;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            this.isVisible = visible;
            if (visible)
            {
                BuildMesh();
            }
            else
            {
                ClearMesh();
            }
        }

        public bool IsVisible()
        {
            return this.isVisible;
        }

        public Bounds GetBounds()
        {
            return this.bounds;
        }

        void OnMapDataReceived(TerrainData terrainData)
        {
            this.terrainData = terrainData;
            this.UpdateTerrainChunk();
        }

        void BuildMesh()
        {
            if (this.terrainData == null || this.allGameObjects.Count > 0 || this.allMeshes.Count > 0)
            {
                return;
            }

            int requiredMeshes = this.terrainData.GetVertices().Count / 65535 + (this.terrainData.GetVertices().Count % 65535 == 0 ? 0 : 1);
            for (int i = 0; i < requiredMeshes; i++)
            {
                Mesh mesh = new Mesh();
                var meshVertices = this.terrainData.GetVertices().GetRange(i * 65535, i == requiredMeshes - 1 ? this.terrainData.GetVertices().Count % 65535 : 65535).ToArray();
                var colors = new Color[meshVertices.Length];
                for (int j = 0; j < colors.Length; j++)
                {
                    float height = Mathf.InverseLerp(0, mapGenerator.height, meshVertices[j].y);
                    colors[j] = gradient.Evaluate(height);
                }
                mesh.vertices = meshVertices;
                mesh.triangles = Enumerable.Range(0, i == requiredMeshes - 1 ? this.terrainData.GetVertices().Count % 65535 : 65535).ToArray();
                mesh.colors = colors;
                mesh.RecalculateNormals();
                this.allMeshes.Add(mesh);
            }
            foreach (var mesh in this.allMeshes)
            {
                GameObject g = new GameObject("Terrain Chunk");
                MeshFilter mf = g.AddComponent<MeshFilter>();
                MeshRenderer mr = g.AddComponent<MeshRenderer>();
                MeshCollider mc = g.AddComponent<MeshCollider>();
                mr.material = material;
                mf.mesh = mesh;
                mc.sharedMesh = mesh;
                g.transform.parent = this.parent;
                this.allGameObjects.Add(g);
            }

            terrainChunksVisibleLastUpdate.Add(this);

        }

        void ClearMesh()
        {
            foreach (var gameObj in this.allGameObjects)
            {
                Destroy(gameObj);
            }
            foreach (var mesh in this.allMeshes)
            {
                Destroy(mesh);
            }
            this.allGameObjects.Clear();
            this.allMeshes.Clear();
        }
    }
}
