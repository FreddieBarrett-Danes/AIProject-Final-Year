using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/*
*    Title: Pathfinding
*    Author: Lague, S
*    Date: 2017
*    Code version: N/A
*    Availability: https://github.com/SebLague/Pathfinding
*
*/
public class PathRequestManager : MonoBehaviour
{
    Queue<PathRequest> pathRequestsQueue = new Queue<PathRequest>();
    PathRequest currentPathRequest;

    static PathRequestManager instance;
    Pathfinding pathfinding;
    bool isProcessingPath;
    private void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }
    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callBack)
    {
        PathRequest pathRequest = new PathRequest(pathStart, pathEnd, callBack);
        instance.pathRequestsQueue.Enqueue(pathRequest);
        instance.TryProcessNext();
    }
    void TryProcessNext()
    {
        if(!isProcessingPath && pathRequestsQueue.Count > 0)
        {
            currentPathRequest = pathRequestsQueue.Dequeue();
            isProcessingPath = true;
            pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        }
    }
    public void FinishedPath(Vector3[] path, bool success)
    {
        currentPathRequest.callBack(path, success);
        isProcessingPath = false;
        TryProcessNext();
    }
    struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callBack;

        public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callBack)
        {
            pathStart = _start;
            pathEnd = _end;
            callBack = _callBack;   
        }
    }
}
