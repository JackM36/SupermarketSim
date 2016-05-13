using UnityEngine;
using System.Collections;

public class NavMeshNode: IHeapItem<NavMeshNode>
{
    public bool walkable;
    public Vector3 position;
    public Vector2 gridPos;

    public int gCost;
    public int hCost;

    public NavMeshNode parent;

    int heapIndex;

    public NavMeshNode(bool walkable, Vector3 position, Vector2 gridPos)
    {
        this.walkable = walkable;
        this.position = position;
        this.gridPos = gridPos;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(NavMeshNode other)
    {
        int compare = fCost.CompareTo(other.fCost);
        if (compare == 0)
        {
            // if fCosts are the same, we compare the hCosts
            compare = hCost.CompareTo(other.hCost);
        }

        return -compare;
    }
}

