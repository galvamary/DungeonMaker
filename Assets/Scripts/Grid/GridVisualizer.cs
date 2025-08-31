using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridVisualizer : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Color gridColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private int visibleGridSize = 50;  // Moderate size for performance
    
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Camera mainCamera;
    private GridManager gridManager;
    private Vector2Int lastCameraGridPos = Vector2Int.zero;
    
    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        mainCamera = Camera.main;
        gridManager = GridManager.Instance;
        
        CreateGridMaterial();
        UpdateGridMesh();
    }
    
    private void Update()
    {
        if (mainCamera != null && gridManager != null)
        {
            Vector3 cameraPos = mainCamera.transform.position;
            
            // Snap grid to nearest grid unit to prevent jumping
            float cellSize = gridManager.CellSize;
            float snapX = Mathf.Floor(cameraPos.x / cellSize) * cellSize;
            float snapY = Mathf.Floor(cameraPos.y / cellSize) * cellSize;
            
            transform.position = new Vector3(snapX, snapY, 0);
        }
    }
    
    private void CreateGridMaterial()
    {
        Material gridMaterial = new Material(Shader.Find("Sprites/Default"));
        gridMaterial.color = gridColor;
        meshRenderer.material = gridMaterial;
        meshRenderer.sortingOrder = -100;
    }
    
    private void UpdateGridMesh()
    {
        Mesh mesh = new Mesh();
        
        float cellSize = gridManager.CellSize;
        int halfSize = visibleGridSize / 2;
        
        int vertexCount = ((visibleGridSize + 1) * 2 + (visibleGridSize + 1) * 2) * 4;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[vertexCount / 4 * 6];
        Vector2[] uvs = new Vector2[vertexCount];
        
        int vIndex = 0;
        int tIndex = 0;
        
        float startX = -halfSize * cellSize;
        float startY = -halfSize * cellSize;
        float endX = halfSize * cellSize;
        float endY = halfSize * cellSize;
        
        // Vertical lines
        for (int i = 0; i <= visibleGridSize; i++)
        {
            float x = startX + i * cellSize;
            
            AddLine(vertices, triangles, uvs, ref vIndex, ref tIndex,
                    new Vector3(x - lineWidth / 2, startY, 0),
                    new Vector3(x + lineWidth / 2, startY, 0),
                    new Vector3(x - lineWidth / 2, endY, 0),
                    new Vector3(x + lineWidth / 2, endY, 0));
        }
        
        // Horizontal lines
        for (int i = 0; i <= visibleGridSize; i++)
        {
            float y = startY + i * cellSize;
            
            AddLine(vertices, triangles, uvs, ref vIndex, ref tIndex,
                    new Vector3(startX, y - lineWidth / 2, 0),
                    new Vector3(endX, y - lineWidth / 2, 0),
                    new Vector3(startX, y + lineWidth / 2, 0),
                    new Vector3(endX, y + lineWidth / 2, 0));
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        
        meshFilter.mesh = mesh;
    }
    
    private void AddLine(Vector3[] vertices, int[] triangles, Vector2[] uvs, 
                        ref int vIndex, ref int tIndex,
                        Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        vertices[vIndex] = p1;
        vertices[vIndex + 1] = p2;
        vertices[vIndex + 2] = p3;
        vertices[vIndex + 3] = p4;
        
        uvs[vIndex] = new Vector2(0, 0);
        uvs[vIndex + 1] = new Vector2(1, 0);
        uvs[vIndex + 2] = new Vector2(0, 1);
        uvs[vIndex + 3] = new Vector2(1, 1);
        
        triangles[tIndex] = vIndex;
        triangles[tIndex + 1] = vIndex + 2;
        triangles[tIndex + 2] = vIndex + 1;
        triangles[tIndex + 3] = vIndex + 1;
        triangles[tIndex + 4] = vIndex + 2;
        triangles[tIndex + 5] = vIndex + 3;
        
        vIndex += 4;
        tIndex += 6;
    }
}