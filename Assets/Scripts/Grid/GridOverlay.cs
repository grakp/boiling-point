using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class GridOverlay : MonoBehaviour
{
    [SerializeField] Color lineColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] float lineDepth = 0f;
    [SerializeField] Material lineMaterialOverride;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Material lineMaterial;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
        lineMaterial = lineMaterialOverride != null ? new Material(lineMaterialOverride) : new Material(shader);
    }

    void Start()
    {
        BuildGridMesh();
    }

    void OnDestroy()
    {
        if (lineMaterial != null) Destroy(lineMaterial);
    }

    public void BuildGridMesh()
    {
        var gm = GridManager.Instance;
        if (gm == null) return;

        float size = gm.CellSize;
        gm.GetGridBounds(out int minX, out int minY, out int maxX, out int maxY);
        Vector2 originWorld = gm.CellToWorld(new Vector2Int(minX, minY)) - new Vector2(size * 0.5f, size * 0.5f);

        // calculate number of vertices and indices
        int w = maxX - minX + 1;
        int h = maxY - minY + 1;
        int vertLines = w + 1;
        int horizLines = h + 1;
        int lineCount = vertLines + horizLines;
        var vertices = new Vector3[lineCount * 2];
        var indices = new int[lineCount * 2];

        // calculate maximum x and y world positions
        float maxXw = originWorld.x + (w + 1) * size;
        float maxYw = originWorld.y + (h + 1) * size;
        int v = 0;

        // create vertical lines
        for (int i = 0; i <= w; i++)
        {
            float x = originWorld.x + i * size;
            vertices[v] = new Vector3(x, originWorld.y, lineDepth);
            vertices[v + 1] = new Vector3(x, maxYw, lineDepth);
            indices[v] = v;
            indices[v + 1] = v + 1;
            v += 2;
        }
        // create horizontal lines
        for (int j = 0; j <= h; j++)
        {
            float y = originWorld.y + j * size;
            vertices[v] = new Vector3(originWorld.x, y, lineDepth);
            vertices[v + 1] = new Vector3(maxXw, y, lineDepth);
            indices[v] = v;
            indices[v + 1] = v + 1;
            v += 2;
        }

        // create mesh
        var mesh = new Mesh { name = "GridOverlay" };
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        meshFilter.mesh = mesh;

        // set material color and sorting order
        lineMaterial.color = lineColor;
        meshRenderer.sharedMaterial = lineMaterial;
        meshRenderer.sortingOrder = -100;
    }
}
