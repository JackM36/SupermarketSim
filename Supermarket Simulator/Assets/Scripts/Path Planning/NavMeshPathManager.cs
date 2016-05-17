using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class NavMeshPathManager : MonoBehaviour 
{
    Queue<PathRequest> pathRequests = new Queue<PathRequest>();
    PathRequest currentRequest;
    bool isProcessingPath;

    static NavMeshPathManager instance;
    static NavMeshPathFinding pathFinder;
    static NavMesh navMesh;

    void Awake()
    {
        // Get Components
        instance = this;
        pathFinder = GetComponent<NavMeshPathFinding>();
        navMesh = GetComponent<NavMesh>();
    }

    struct PathRequest
    {
        public Vector3 startPos;
        public Vector3 targetPos;
        public Action<Vector3[], bool> callback;

        // Conctructor
        public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> actionCallback) 
        {
            startPos = start;
            targetPos = end;
            callback = actionCallback;
        }
    }

    public static void requestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        // Create the request
        PathRequest request = new PathRequest(pathStart, pathEnd, callback);

        // Add it to the queue of requests
        instance.pathRequests.Enqueue(request);

        instance.processNextRequest();
    }

    public static void removeUsedNodePenalty(Vector3 nodePos)
    {
        // remove the penalty that was added by the agent for using this node
        navMesh.getNode(nodePos).usedPenalty -= pathFinder.usedNodeCost;
    }

    void processNextRequest()
    {
        // Check if it can process the next path (if no path processing is happening and if there are more requests to process)
        if (!isProcessingPath && pathRequests.Count > 0)
        {
            // Take the next request from the queue
            currentRequest = pathRequests.Dequeue();
            isProcessingPath = true;

            pathFinder.getPath(currentRequest.startPos, currentRequest.targetPos);
        }
    }

    public void finishedProcessingPath(Vector3[] path, bool success)
    {
        currentRequest.callback(path, success);
        isProcessingPath = false;

        processNextRequest();
    }
}
