using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
*    Title: Pathfinding
*    Author: Lague, S
*    Date: 2017
*    Code version: N/A
*    Availability: https://github.com/SebLague/Pathfinding
*
*/
public class Pathfinding : MonoBehaviour
{
    MyGrid grid;
    PathRequestManager requestManager;
    private void Awake()
    {
        grid = GetComponent<MyGrid>();
        requestManager = GetComponent<PathRequestManager>();
    }
    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }
    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3[] wayPoints = new Vector3[0];
        bool pathSuccess = false;

        GridNode startNode = grid.GetNodeFromWorldPoint(startPos);
        GridNode targetNode = grid.GetNodeFromWorldPoint(targetPos);
        if (startNode.walkable && targetNode.walkable)
        {
            Heap<GridNode> openSet = new Heap<GridNode>(grid.MaxSize());
            HashSet<GridNode> closedSet = new HashSet<GridNode>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                GridNode node = openSet.RemoveFirst();
                closedSet.Add(node);

                if (node == targetNode)
                {
                    pathSuccess = true;

                    break;
                }

                foreach (GridNode neighbour in grid.GetNeighbours(node))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
                    if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = node;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
            
        }
        yield return null;
        if (pathSuccess)
        {
            wayPoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedPath(wayPoints, pathSuccess);

    }

    Vector3[] RetracePath(GridNode startNode, GridNode endNode)
    {
        List<GridNode> path = new List<GridNode> ();
        GridNode currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        return waypoints;
        
    }
    Vector3[] SimplifyPath(List<GridNode> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        //Vector2 directionOld = Vector2.zero;

        for(int i = 1; i < path.Count; i++)
        {
            waypoints.Add(path[i].worldPosition);
            /*Vector2 directionNew = new Vector2(path[i-1].gridX - path[i].gridX, path[i-1].gridY - path[i].gridY);
            if(directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;*/
        }
        return waypoints.ToArray();
    }
    int GetDistance(GridNode nodeA, GridNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if(dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX- dstY);
        }
        return 14 * dstX + 10 * (dstY- dstX);
    }
}
