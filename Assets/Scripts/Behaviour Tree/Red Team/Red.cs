using UnityEngine;


[RequireComponent(typeof(RedBB))]
public class Red : MonoBehaviour
{
    private RedBB bb;
    private BTNode BTRootNode;

    // Start is called before the first frame update
    void Start()
    {
        //reference Red BlackBoard
        bb = GetComponent<RedBB>();
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < bb.blue.Length; i++)
        {
            float tempDist = Vector3.Distance(transform.position, bb.blue[i].transform.position);

            if (tempDist < nearestDistance)
            {
                //needs to check if this object is visible, will look to implement next
                bb.myTarget = bb.blue[i];
                nearestDistance = tempDist;
            }
        }
        //access targeted blues component to change variable
        bb.Bbb = bb.myTarget.GetComponent<BlueBB>();
        //MoveSpeed = bb.speed;

        //root selector
        Selector rootChild = new (bb); //selectors execute children 1 by 1 until one succeeds
        BTRootNode = rootChild;

        //Chase Sequence
        CompositeNode chaseSequence = new Sequence(bb); //Sequence of actions to take when chasing
        ChaseCheck chaseRoot = new (chaseSequence, bb); //defines the condition required to enter chase sequence
        chaseSequence.AddChild(new CalculateChaseLocation(bb)); //calculate chase location
        chaseSequence.AddChild(new RedChaseTo(bb, this)); //set move location
        chaseSequence.AddChild(new RedWaitChaseLocation(bb, this)); //wait till reached destination
        chaseSequence.AddChild(new RedStopMovement(bb, this)); //stop movement 

        //fight sequence
        CompositeNode attackSequence = new Sequence(bb);//attack sequence
        RedFightDecorator fightRoot = new (attackSequence, bb);//decorator for fight sequence
        attackSequence.AddChild(new RedAttack(bb)); //adds red attack to sequence


        //flee sequence
        CompositeNode fleeSequence = new Sequence(bb);//flee sequence after attacking or low health
        RedFleeDecorator fleeRoot = new (fleeSequence, bb);//flee decorator
        fleeSequence.AddChild(new CalculateFleeLocationRed(bb));
        fleeSequence.AddChild(new RedFleeTo(bb, this));
        fleeSequence.AddChild(new RedWaitFleeLocation(bb, this));
        fleeSequence.AddChild(new RedStopMovement(bb, this));



        //Adding to Root Selector
        rootChild.AddChild(fleeRoot);
        rootChild.AddChild(chaseRoot);
        rootChild.AddChild(fightRoot);
        

        InvokeRepeating(nameof(ExecuteBT), 0.1f, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (bb.health > 0)
        {
            bb.blue = bb.CheckEnemyTeam(bb.blue, "Blue");
            Vector3 myPos = gameObject.transform.position;

            float nearestDistance = float.MaxValue;


            for (int i = 0; i < bb.blue.Length; i++)
            {
                if (bb.blue[i] != null)
                {
                    float tempDist = Vector3.Distance(myPos, bb.blue[i].transform.position);

                    if (tempDist < nearestDistance)
                    {
                        if (bb.blue[i] != null)
                        {
                            bb.myTarget = bb.blue[i];
                            bb.Bbb = bb.myTarget.GetComponent<BlueBB>();
                            nearestDistance = tempDist;
                        }
                    }
                }
            }
            if (bb.isMoving)
            {
                bb.reachedTarget = false;
                bb.Club.SetActive(false);
            }
            else if(!bb.isMoving)
            {
                if (Vector3.Distance(bb.Bbb.transform.position, transform.position) <= 2.0f)
                {
                    bb.reachedTarget = true;
                    bb.Club.SetActive(true);
                    bb.isChasing = false;
                    if(!bb.Bbb.isFleeing)
                        bb.Bbb.isFleeing = true;
                }
               else if (Vector3.Distance(bb.fleeLocation, transform.position) <= 2.0f)
                {
                    //bb.reachedTarget = false;
                    //bb.isFleeing = false;
                    bb.isChasing = true;
                }
                
            }
        }
        else if(bb.health <= 0)
        {
            Destroy(gameObject);
        }

    }

    public void RedMoveTo(Vector3 MoveLocation)
    {
        bb.isMoving = true;
        PathRequestManager.RequestPath(transform.position, MoveLocation, bb.OnPathFound);
    }
    public void StopMovement()
    {
        Debug.Log("red stoping");
        bb.hasAttacked = false;
        bb.isMoving = false;
    }
    public void ExecuteBT()
    {
        BTRootNode.Execute();
    }
}

//Chase Sequence stuff
public class ChaseCheck : ConditionalDecorator
{
    private RedBB rBB;

    public ChaseCheck(BTNode WrappedNode, BlackBoard bb) : base(WrappedNode, bb)
    {
        rBB = (RedBB)bb;
    }
    public override bool CheckStatus()
    {
        return rBB.isChasing;
    }
}

public class CalculateChaseLocation : BTNode
{
    private RedBB rBB;

    public CalculateChaseLocation(BlackBoard bb) : base(bb)
    {
        rBB = (RedBB)bb;
    }
    public override BTStatus Execute()
    {
        //Debug.Log("Red Getting Move Location");
        rBB.chaseLocation = rBB.myTarget.transform.position;
        return BTStatus.SUCCESS;
    }
}

public class RedChaseTo : BTNode
{
    private RedBB rBB;
    private Red redRef;

    public RedChaseTo(BlackBoard bb, Red rad) : base(bb)
    {
        rBB = (RedBB)bb;
        redRef = rad;
    }
    public override BTStatus Execute()
    {
        redRef.RedMoveTo(rBB.chaseLocation);
        return BTStatus.SUCCESS;
    }
}
public class RedWaitChaseLocation : BTNode
{
    private RedBB rBB;
    private Red redRef;
    
    public RedWaitChaseLocation(BlackBoard bb, Red rad) : base(bb)
    {
        rBB = (RedBB)bb;
        redRef = rad;
    }
    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.RUNNING;
        //Vector3.Distance(redRef.transform.position, rBB.moveToLocation);
        if (Vector3.Distance(redRef.transform.position, rBB.chaseLocation) <= 2.0f)
        {
            rv = BTStatus.SUCCESS;
        }
        return rv;
    }
}
public class RedStopMovement : BTNode
{
    private Red redRef;
    private RedBB rBB;
    public RedStopMovement(BlackBoard bb, Red rad) : base(bb)
    {
        redRef = rad;
        rBB = (RedBB)bb;
    }
    public override BTStatus Execute()
    {

        if(rBB.isFleeing)
        {
            redRef.StopMovement();
            rBB.isFleeing = false;

        }
        redRef.StopMovement();
        return BTStatus.SUCCESS;
    }
}

//fight Sequence
public class RedFightDecorator : ConditionalDecorator
{
    private RedBB rBB;
    public RedFightDecorator(BTNode WrappedNode, BlackBoard bb) : base(WrappedNode, bb)
    {
        rBB = (RedBB)bb;
    }
    public override bool CheckStatus()
    {
        if (rBB.reachedTarget && !rBB.hasAttacked)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
public class RedAttack : BTNode
{
    private RedBB rBB;
    public RedAttack(BlackBoard bb) : base(bb)
    {
        rBB = (RedBB)bb;
    }
    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.SUCCESS;

        if (rBB.canAttack)
        {
            rBB.ClubAttack();
            rBB.hasAttacked = true;
        }
        return rv;
    }
}
    //flee sequence
public class RedFleeDecorator : ConditionalDecorator
{
    private RedBB rBB;
    public RedFleeDecorator(BTNode WrappedNode, BlackBoard bb) : base(WrappedNode, bb)
    {
        rBB = (RedBB)bb;
    }
    public override bool CheckStatus()
    {
        if(rBB.isFleeing && !rBB.isChasing)
            return true;
        else
            return false;

    }
}
public class CalculateFleeLocationRed : BTNode
{
    private RedBB rBB;

    public CalculateFleeLocationRed(BlackBoard bb) : base(bb)
    {
        rBB = (RedBB)bb;
    }
    public override BTStatus Execute()
    {
        rBB.fleeLocation = rBB.FindCoverPoint(false);
        return BTStatus.SUCCESS;
    }

}
public class RedFleeTo : BTNode
{
    private RedBB rBB;
    private Red redRef;

    public RedFleeTo(BlackBoard bb, Red rad) : base(bb)
    {
        rBB = (RedBB)bb;
        redRef = rad;
    }
    public override BTStatus Execute()
    {
        redRef.RedMoveTo(rBB.fleeLocation);
        return BTStatus.SUCCESS;
    }
}
public class RedWaitFleeLocation : BTNode
{
    private RedBB rBB;
    private Red redRef;

    public RedWaitFleeLocation(BlackBoard bb, Red rad) : base(bb)
    {
        rBB = (RedBB)bb;
        redRef = rad;
    }
    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.RUNNING;
        if (Vector3.Distance(redRef.transform.position, rBB.fleeLocation) <= 2.0f)
        {
            rv = BTStatus.SUCCESS;
        }
        return rv;
    }
}