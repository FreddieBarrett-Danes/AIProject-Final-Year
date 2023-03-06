using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueBB : BlackBoard
{
    [Header("Red Team")]
    public GameObject[] red;
    public GameObject myTarget;
    private GameObject forward;

    [Header("Bot Values")]
    public int health = 3;
    [SerializeField]
    private float bulletSpeed;
    public float MoveSpeed = 10.0f;

    [Header("World Positions")]
    public Vector3 myLocation;
    public Vector3 moveToLocation;
    public Vector3 lastKnownLocation;


    [Header("Boolean")]
    public bool isMoving = false;
    public bool isFleeing = false;
    public bool isVisible = false;
    public bool isTargetVisible = false;


    // Start is called before the first frame update
    void Start()
    {
        red = GameObject.FindGameObjectsWithTag("Red");
        forward = gameObject.transform.GetChild(0).gameObject;
        SetSpeed(MoveSpeed);
        //initialises variables at start for determining closest enemy
        float nearestDistance = float.MaxValue;


        for (int i = 0; i < red.Length; i++)
        {
            float tempDist = Vector3.Distance(transform.position, red[i].transform.position);

            if (tempDist < nearestDistance)
            {

                myTarget = red[i];
                nearestDistance = tempDist;
            }
        }
        //lastKnownLocation = myTarget.transform.position;
    }


    // Update is called once per frame
    void Update()
    {
        myLocation = gameObject.transform.position;
    }
    public void Shooting()
    {
        GameObject bullet = Resources.Load<GameObject>("Bullet"); 
        
        GameObject tempBullet = Instantiate(bullet, forward.transform.position, forward.transform.rotation);
        Rigidbody tempRigidBodyBullet = tempBullet.GetComponent<Rigidbody>();
        tempRigidBodyBullet.AddForce(tempRigidBodyBullet.transform.forward * bulletSpeed);
        //tempBullet.tag = gameObject.transform.tag;
        Destroy(tempBullet, 5f);
    }
}
