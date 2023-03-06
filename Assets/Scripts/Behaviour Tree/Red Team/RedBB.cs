using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RedBB : BlackBoard
{
    [Header("Blue Team")]
    public GameObject[] blue;
    public BlueBB Bbb;
    public GameObject myTarget;

    [Header("Bot Values")]
    public float MoveSpeed = 5.0f;
    public int health = 3;
    private float AttackCooldown = 1.0f;
    public GameObject Club;

    [Header("World Locations")]
    public Vector3 fleeLocation;
    public Vector3 chaseLocation;

    [Header("Combat Booleans")]
    public bool reachedTarget = false;
    public bool hasAttacked = false;
    public bool canAttack = true;

    [Header("Booleans")]
    public bool isChasing = true;
    public bool isFleeing = false;
    public bool isMoving = false;
    public bool isVisible = false;


    //private bool isAttacking = false;

    // Start is called before the first frame update
    void Start()
    {
        blue = GameObject.FindGameObjectsWithTag("Blue");
        Club = gameObject.transform.Find("Club").gameObject;
        SetSpeed(MoveSpeed);
    }
    private void Update()
    {
        
    }
    public void ClubAttack()
    {
        //isAttacking = true;
        if (myTarget != null)
        {
            Vector3 lookPos = LookPosSet(myTarget.transform.position);
            if (lookPos != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * turnSpeed * 10000);
            }
        }
        canAttack = false;
   
        Animator anim = Club.GetComponent<Animator>();
        anim.SetTrigger("Attack");
        StartCoroutine(ResetAttackCooldown());
        reachedTarget = false;
    }
    IEnumerator ResetAttackCooldown()
    {
        StartCoroutine(ResetAttackBool());
        yield return new WaitForSeconds(AttackCooldown);
        canAttack = true;
    }
    IEnumerator ResetAttackBool()
    {
        yield return new WaitForSeconds(1.0f);
        isFleeing = true;
        //isAttacking = false;
    }
}
