using System.Collections;
using System.Linq;
using UnityEngine;

public class BlackBoard : MonoBehaviour
{
    [Header("Default BlackBoard Variables")]
    [HideInInspector] 
    public float turnSpeed = 30.0f;

    public GameObject[] covers;
    public GameObject grid;

    float speed;

    Vector3[] path;
    int targetIndex;

    

    GameObject cube;
    GameObject currentCover = null;

    public Perception perception;

    Vector3 directionToTarget;
    public LayerMask coverLayer;


    private void Awake()
    {
        perception = GetComponent<Perception>();
        covers = GameObject.FindGameObjectsWithTag("Cover");
        cube = Resources.Load<GameObject>("Cube");
        //coverLayer = LayerMask.GetMask("UnWalkable");
    }
    /*
    *    Title: Pathfinding
    *    Author: Lague, S
    *    Date: 2017
    *    Code version: N/A
    *    Availability: https://github.com/SebLague/Pathfinding
    *
    */
   public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
   {
        path = newPath;
        targetIndex = 0;
        StopCoroutine("FollowPath");
        StartCoroutine("FollowPath");
   }

    IEnumerator FollowPath()
    {
        if (path.Length == 0)
            yield break;

        Vector3 currentWaypoint = path[0];
        while (true)
        {
            if (transform.position == currentWaypoint)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                {
                    yield break;
                }
                currentWaypoint = path[targetIndex];
            }
            //Rotate Towards Next Waypoint
            Vector3 targetDir = currentWaypoint - transform.position;
            float step = turnSpeed * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);

            //move Towards Next Waypoint
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            yield return null;

        }
    }

    //My own Code Below
    public Vector3 LookPosSet(Vector3 targetPosition)
    {
        Vector3 rv = targetPosition - gameObject.transform.position;
        rv.y = 0;
        return rv;
    }
    public void SetSpeed(float desiredSpeed)
    {
        speed = desiredSpeed;
    }
    //Finding Cover
    public Vector3 FindCoverPoint(bool CloseorFar)
    {
        Vector3 rv = Vector3.zero;
        RaycastHit hit;
        GameObject cCover;
        if (CloseorFar)
        {
            cCover = GetClosestCover(covers);
            currentCover = cCover;
            if (Physics.Raycast(transform.position, cCover.transform.position - transform.position, out hit, 50f, coverLayer))
            {
                float dist = Vector3.Distance(hit.point, transform.position);
                float exactDist = Vector3.Distance(cCover.transform.position, transform.position);
                float halfNormalToCenter = exactDist - dist;

                rv = transform.position + ((cCover.transform.position - transform.position).normalized * (exactDist + halfNormalToCenter + 3f));
                
                rv.y = 0;
                //Instantiate(cube, rv, Quaternion.identity);
            }
        }
        else
        {
            cCover = GetFurthestCover(covers);
            Vector3 dir = transform.position - cCover.transform.position;
            Vector3 Origin = (cCover.transform.position - transform.position);
            if (Physics.Raycast(Origin, dir, out hit, 50f, coverLayer))
            {
                float dist = Vector3.Distance(hit.point, transform.position);
                float exactDist = Vector3.Distance(cCover.transform.position, transform.position);
                float halfNormalToCenter = exactDist - dist;

                rv = transform.position + (Origin.normalized * (exactDist - halfNormalToCenter + 3f));
                rv.y = 0;
            }
        }
        
        return rv;
    }
    GameObject GetClosestCover(GameObject[] coverArray)
    {
        GameObject closestCover = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject cover in coverArray)
        {
            directionToTarget = cover.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                if (cover != currentCover)
                {
                    closestDistanceSqr = dSqrToTarget;
                    closestCover = cover;
                }
            }
        }
        return closestCover;
    }
    GameObject GetFurthestCover(GameObject[] coverArray)
    {
        float furthestDistance = 0;
        GameObject furthestObject = null;
        foreach (GameObject cover in coverArray)
        {
            float ObjectDistance = Vector3.Distance(transform.position, cover.transform.position);
            if (ObjectDistance > furthestDistance)
            {
                furthestObject = cover;
                furthestDistance = ObjectDistance;
            }
        }
        return furthestObject;
    }
    //Go To Last Known Position
    public Vector3 FindLastKnownPosition(GameObject targetObject)
    {
        Vector3 rv = Vector3.zero;
        if(perception.memoryMap.ContainsKey(targetObject))
        {
            rv = perception.memoryMap[targetObject].lastSensedPosition;
        }
        return rv;
    }
    public bool IsWithinFOV(GameObject targetObject)
    {
        bool temp = false;
        if (perception.memoryMap.ContainsKey(targetObject))
        {
            temp = perception.memoryMap[targetObject].withinFOV;
        }
        return temp;
    }
    public GameObject[] CheckEnemyTeam(GameObject[] enemyTeam, string tag)
    {

        // Create a new array that contains only non-null gameObjects
        GameObject[] nonNullGameObjects = enemyTeam.Where(gameObject => gameObject != null).ToArray();
        return nonNullGameObjects;

    }
    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }

}

