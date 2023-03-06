using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
//works look at editing entire script rather than casting rays uses a distance check, also want to sense objects not nescerily enemy units. is a battle sim units will generally know where the enemy is

[RequireComponent(typeof(Perception))]
public class FieldOfView : MonoBehaviour
{
    /// <summary>
    /// The Radius (distance) the agent can see
    /// </summary>
    public float ViewRadius;

    /// <summary>
    /// The angle (in degrees) the agent can see. Range of 0 to 360
    /// </summary>
    [Range(0, 360)]
    public float ViewAngle;

    /// <summary>
    /// The layer of target objects (what we're trying to sense)
    /// </summary>
    public LayerMask TargetLayer;

    /// <summary>
    /// The layer of obstacles (things that should block our line of sight)
    /// </summary>
    public LayerMask ObstacleLayer;

    /// <summary>
    /// List of visible targets is updated at a set interval
    /// </summary>
    public List<Transform> visibleTargets = new List<Transform>();

    private void Start()
    {


        //Check for visible targets every 0.2 seconds, which is approx. the average human response time to stimulus
        InvokeRepeating("FindVisibleTargets", 0.2f, 0.2f);
    }

    void FindVisibleTargets()
    {
        //Clear the current visible targets
        visibleTargets.Clear();

        //Do simple sphere collision check for nearby targets
        Collider[] targets = Physics.OverlapSphere(transform.position, ViewRadius, TargetLayer);
        

        //Iterate through each target
        foreach (Collider target in targets)
        {
            //Get direction and magnitude to target
            Vector3 ToTarget = (target.transform.position - transform.position);

            //Normalize so we have direction without magnitude
            Vector3 ToTargetNormalized = ToTarget.normalized;


            if (Vector3.Angle(transform.forward, ToTargetNormalized) < ViewAngle / 2 //Check if the target is within our FoV
                && !Physics.Raycast(transform.position, ToTargetNormalized, ToTarget.magnitude, ObstacleLayer)) // then do the raycast to determine LoS
            {
                //I see you!
                visibleTargets.Add(target.transform);
                if (target.gameObject.GetComponent<BlueBB>())
                {
                    //may need changing after further implementation of other behaviours
                    target.gameObject.GetComponent<BlueBB>().isVisible = true;
                }
                else if (target.gameObject.GetComponent<RedBB>())
                {
                    target.gameObject.GetComponent<RedBB>().isVisible = true;
                }
            }
            else
            {
                if (target.gameObject.GetComponent<BlueBB>())
                {
                    //may need changing after further implementation of other behaviours
                    target.gameObject.GetComponent<BlueBB>().isVisible = false;
                }
                else if (target.gameObject.GetComponent<RedBB>())
                {
                    target.gameObject.GetComponent<RedBB>().isVisible = false;
                }
            }
        }

        //Add memory record to our perception system
        Perception percept = GetComponent<Perception>();

        percept.ClearFOV();
        foreach (Transform target in visibleTargets)
        {
            percept.AddMemory(target.gameObject);
        }
    }
    
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
    private void LateUpdate()
    {
        /*foreach (Transform target in visibleTargets)
        {
            Debug.DrawLine(transform.position, target.position, Color.red);
        }*/
        //Draw or hide our FoV
        /*if (DrawFOV)
        {
            viewMeshFilter.gameObject.SetActive(true);
            DrawFieldOfView();

            
        }
        else
        {
            viewMeshFilter.gameObject.SetActive(false);
        }
    }*/

        //This part draws our FoV for debug purposes. I got this function from the interwebz
        /*    void DrawFieldOfView()
            {
                int stepCount = Mathf.RoundToInt(ViewAngle * meshResolution);
                float stepAngleSize = ViewAngle / stepCount;
                List<Vector3> viewPoints = new List<Vector3>();
                ViewCastInfo oldViewCast = new ViewCastInfo();
                for (int i = 0; i <= stepCount; i++)
                {
                    float angle = transform.eulerAngles.y - ViewAngle / 2 + stepAngleSize * i;
                    ViewCastInfo newViewCast = ViewCast(angle);

                    if (i > 0)
                    {
                        bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                        if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                        {
                            EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                            if (edge.pointA != Vector3.zero)
                            {
                                viewPoints.Add(edge.pointA);
                            }
                            if (edge.pointB != Vector3.zero)
                            {
                                viewPoints.Add(edge.pointB);
                            }
                        }

                    }


                    viewPoints.Add(newViewCast.point);
                    oldViewCast = newViewCast;
                }

                int vertexCount = viewPoints.Count + 1;
                Vector3[] vertices = new Vector3[vertexCount];
                int[] triangles = new int[(vertexCount - 2) * 3];

                vertices[0] = Vector3.zero;
                for (int i = 0; i < vertexCount - 1; i++)
                {
                    vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

                    if (i < vertexCount - 2)
                    {
                        triangles[i * 3] = 0;
                        triangles[i * 3 + 1] = i + 1;
                        triangles[i * 3 + 2] = i + 2;
                    }
                }

                viewMesh.Clear();

                viewMesh.vertices = vertices;
                viewMesh.triangles = triangles;
                viewMesh.RecalculateNormals();
            }

            EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
            {
                float minAngle = minViewCast.angle;
                float maxAngle = maxViewCast.angle;
                Vector3 minPoint = Vector3.zero;
                Vector3 maxPoint = Vector3.zero;

                for (int i = 0; i < edgeResolveIterations; i++)
                {
                    float angle = (minAngle + maxAngle) / 2;
                    ViewCastInfo newViewCast = ViewCast(angle);

                    bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                    if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
                    {
                        minAngle = angle;
                        minPoint = newViewCast.point;
                    }
                    else
                    {
                        maxAngle = angle;
                        maxPoint = newViewCast.point;
                    }
                }

                return new EdgeInfo(minPoint, maxPoint);
            }

            ViewCastInfo ViewCast(float globalAngle)
            {
                Vector3 dir = DirFromAngle(globalAngle, true);
                RaycastHit hit;

                if (Physics.Raycast(transform.position, dir, out hit, ViewRadius, ObstacleLayer))
                {
                    return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
                }
                else
                {
                    return new ViewCastInfo(false, transform.position + dir * ViewRadius, ViewRadius, globalAngle);
                }
            }

            public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
            {
                if (!angleIsGlobal)
                {
                    angleInDegrees += transform.eulerAngles.y;
                }
                return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
            }

            public struct ViewCastInfo
            {
                public bool hit;
                public Vector3 point;
                public float dst;
                public float angle;

                public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
                {
                    hit = _hit;
                    point = _point;
                    dst = _dst;
                    angle = _angle;
                }
            }

            public struct EdgeInfo
            {
                public Vector3 pointA;
                public Vector3 pointB;

                public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
                {
                    pointA = _pointA;
                    pointB = _pointB;
                }
            }
            private void OnDrawGizmos()
            {
                //Gizmos.DrawWireSphere(transform.position, ViewRadius);
            }*/
    }
}

