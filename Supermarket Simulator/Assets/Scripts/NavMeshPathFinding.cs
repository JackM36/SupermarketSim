using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NavMeshPathFinding : MonoBehaviour 
{
    [Header("Move Costs")]
    public int straightMoveCost = 10;
    public int diagonalMoveCost = 14;
    public int usedNodeCost = 1;

    NavMesh navMesh;
    NavMeshPathManager pathManager;

	void Awake () 
    {
	    // Get components
        navMesh = GetComponent<NavMesh>();
        pathManager = GetComponent<NavMeshPathManager>();
	}

    public void getPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(findPath(startPos, targetPos));
    }
	
    IEnumerator findPath(Vector3 startPos, Vector3 targetPos)
    {
        bool foundPath = false;

        // Get start and target nodes
        NavMeshNode startNode = navMesh.getNode(startPos);
        NavMeshNode targetNode = navMesh.getNode(targetPos);

        // Only try to find a path if both start and target nodes are actually walkable
        if (startNode.walkable && targetNode.walkable)
        {
            Heap<NavMeshNode> openSet = new Heap<NavMeshNode>(navMesh.gridMaxSize);
            List<NavMeshNode> closedSet = new List<NavMeshNode>();

            // add the start node in the open set
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                /* REPLACED BY THE HEAP
                // Create temp node for the current node
                NavMeshNode currentNode = openSet[0];
                // go through all nodes in the openSet (except current)
                for (int i = 1; i < openSet.Count; i++)
                {
                    // Check if this node's f cost is less than current. If equal, take their hCost
                    if ((openSet[i].fCost < currentNode.fCost) || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    {
                        //If yes, take that as current and put current back in openSet
                        currentNode = openSet[i];
                    }
                }

                // remove lowest cost node (current) from openSet, and add to closedSet
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                */

                NavMeshNode currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                // Check if currentNode is the target
                if (currentNode == targetNode)
                {
                    foundPath = true;
                    break;
                }

                // check all neighbours of current node
                foreach (NavMeshNode neighbour in navMesh.getNodeNeighbours(currentNode))
                {
                    // Check if this neighbour is not walkable or is already in the closedSet. If yes, just skip.
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    // Calculate the new path from start to neighbour;
                    int newpathCost = currentNode.gCost + getGridDistance(currentNode, neighbour);

                    // check if new path cost is less than current gCost of neighbour, or if neighbour is not in the openSet
                    if (newpathCost < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        // update neighbour costs and parent.
                        neighbour.gCost = newpathCost;
                        neighbour.hCost = getGridDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        // if not in the openSet, add it
                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
        }

        //wait one frame
        yield return null;

        List<NavMeshNode> path = new List<NavMeshNode>();
        Vector3[] vectorPath = new Vector3[0];

        if (foundPath)
        {
            // Trace the found path
            path = tracePath(startNode, targetNode);

            path = smoothPath(path);

            // penalize all nodes of this path because they are being used by the agent that will follow this path
            // This will make agents take slightly different paths to avoid overused paths
            penalizePath(path);

            // Convert path to vector positions instead of node objects
            vectorPath = vectorizePath(path);
        }

        // Let the request manager know the path was processed, and pass the path
        pathManager.finishedProcessingPath(vectorPath, foundPath);
    }

    Vector3[] vectorizePath(List<NavMeshNode> path)
    {
        Vector3[] vectorPath = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            vectorPath[i] = path[i].position;
        }

        return vectorPath;
    }

    List<NavMeshNode> tracePath(NavMeshNode startNode, NavMeshNode targetNode)
    {
        List<NavMeshNode> path = new List<NavMeshNode>();

        // Trace the path backwards, so start with targetNode
        NavMeshNode currentNode = targetNode;

        // keep getting the parents of nodes in order to retrace the found path, until startNode is found
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        // Reverse the list because the path was stored from target to start.
        path.Reverse();

        return path;
    }

    List<NavMeshNode> smoothPath(List<NavMeshNode> path)
    {
        List<NavMeshNode> smoothPath = new List<NavMeshNode>();
        int lastVisibleNodeIndex = 0;

        // Add the first node
        smoothPath.Add(path[0]);

        // For each node, check if all other nodes onwards are visible (exclude those that are later decided not to be included in path)
        for (int i = 0; i < path.Count-1; i+=(lastVisibleNodeIndex-i))
        {
            for (int j = i+1; j < path.Count; j++)
            {
                // shoot a linecast from node a to b to determine if b is visible to a
                if (Physics.Linecast(path[i].position, path[j].position))
                {
                    // If there is a hit, it is not visible. Therefore add the last visible one to the list
                    smoothPath.Add(path[lastVisibleNodeIndex]);
                    break;
                }
                else
                {
                    // Mark it as the last visible node
                    lastVisibleNodeIndex = j;
                }
            }
        }

        // Always add the last(target) node to the list
        smoothPath.Add(path[path.Count-1]);
            
        return smoothPath;
    }

    void penalizePath(List<NavMeshNode> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            path[i].usedPenalty += usedNodeCost;
        }
    }

    int getGridDistance(NavMeshNode nodeA, NavMeshNode nodeB)
    {
        int distX = (int)Mathf.Abs(nodeA.gridPos.x - nodeB.gridPos.x);
        int distY = (int)Mathf.Abs(nodeA.gridPos.y - nodeB.gridPos.y);

        if (distX > distY)
        {
            return diagonalMoveCost * distY + straightMoveCost * (distX - distY);
        }
        else
        {
            return diagonalMoveCost * distX + straightMoveCost * (distY - distX);
        }
    }
}
