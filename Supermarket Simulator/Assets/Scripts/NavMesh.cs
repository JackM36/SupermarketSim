using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NavMesh : MonoBehaviour 
{
    [Header("NavMesh Grid")]
    public LayerMask unwalkableLayers;
    public Vector2 gridWorldSize;
    public float nodeRadius;

    [Header("Editor Visuals")]
    public bool showGrid = false;
    public float nodeGizmoOffset = 0.1f;
    public Color gizmoGridColor = Color.red;
    public Color gizmoWalkableColor = Color.white;
    public Color gizmoUnwalkableColor = Color.red;

    NavMeshNode[,] grid;
    float nodeDiameter;
    [HideInInspector]
    public int gridSizeX;
    [HideInInspector]
    public int gridSizeY;

    public int gridMaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

	void Start () 
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        CreateNavMeshGrid();
	}

    void CreateNavMeshGrid()
    {
        grid = new NavMeshNode[gridSizeX, gridSizeY];
        Vector3 gridBottomLeftPoint = transform.position - (Vector3.right * gridWorldSize.x / 2) - (Vector3.forward * gridWorldSize.y / 2);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 nodePos = gridBottomLeftPoint + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);

                // Determine if it is walkable by checking if there is a collision with unwalkable object in a sphere of node radius
                bool walkable = !(Physics.CheckSphere(nodePos,nodeRadius,unwalkableLayers));

                // Create and store node
                grid[x,y] = new NavMeshNode(walkable,nodePos, new Vector2(x, y));
            }
        }
    }

    public NavMeshNode getNode(Vector3 worldPos)
    {
        // Calculate the percentage of node position in wolrd space in the grid
        float percentX = Mathf.Clamp01((worldPos.x + gridWorldSize.x / 2) / gridWorldSize.x);
        float percentY = Mathf.Clamp01((worldPos.z + gridWorldSize.y / 2) / gridWorldSize.y);

        // Determine which node in the array it's on that world position 
        int x = Mathf.RoundToInt ((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt ((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    public List<NavMeshNode> getNodeNeighbours(NavMeshNode node)
    {
        List<NavMeshNode> neighbours = new List<NavMeshNode>();

        // Search the 3x3 grid around the node (its possible neighbours)
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Skip when is on the center (0, 0), because center is this node
                if (x == 0 && y == 0)
                {
                    continue;
                }

                // position to check for neighbour
                int checkX = (int)node.gridPos.x + x;
                int checkY = (int)node.gridPos.y + y;

                // If position is inside the grid, then there is a neighbour there
                if ((checkX >= 0 && checkX < gridSizeX) && (checkY >= 0 && checkY < gridSizeY))
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }
        
    void OnDrawGizmos()
    {
        // Show the grid gizmo
        Gizmos.color = gizmoGridColor;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 0.5f, gridWorldSize.y));

        if (showGrid && grid != null)
        {
            // draw gizmo for each node
            foreach (NavMeshNode node in grid)
            {
                if (node.walkable)
                {
                    Gizmos.color = gizmoWalkableColor;
                }
                else
                {
                    Gizmos.color = gizmoUnwalkableColor;
                }

                Gizmos.DrawCube(node.position, Vector3.one * (nodeDiameter - nodeGizmoOffset));
            }
        }
    }
}
