using UnityEngine;
/*
*    Title: Pathfinding
*    Author: Lague, S
*    Date: 2017
*    Code version: N/A
*    Availability: https://github.com/SebLague/Pathfinding
*
*/
public class GridNode : IHeapItem<GridNode>
{
    public bool walkable;
    public Vector3 worldPosition;

    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public GridNode parent;

    int heapIndex;
    public GridNode(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
    public int fCost { get { return gCost + hCost; } }

    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }
    public int CompareTo(GridNode nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if(compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}
