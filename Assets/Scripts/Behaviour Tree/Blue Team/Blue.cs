using System;
using Unity.VisualScripting;
using UnityEngine;
//i want blue when fleeing to still shoot but slowly run then trun around then turn back around run
public class Blue : MonoBehaviour
{   
    private BlueBB bb;
    private BTNode BTRootNode;
    // Start is called before the first frame update
    void Start()
    {
        //reference Blue BlackBoard
        bb = GetComponent<BlueBB>();

        //root selector
        Selector rootChild = new (bb); //selectors execute children 1 by 1 until one succeeds
        BTRootNode = rootChild;

        //flee sequence
        CompositeNode fleeSequence = new Sequence(bb);//sequence of actions when flee
        BlueFleeDecorator fleeRoot = new (fleeSequence, bb);
        fleeSequence.AddChild(new CalculateFleeLocationBlue(bb)); //calculates flee location
        fleeSequence.AddChild(new BlueMoveTo(bb, this));//moves to location
        fleeSequence.AddChild(new BlueWaitAtLocation(bb, this));//wait till reached destination
        fleeSequence.AddChild(new BlueStopMovement(bb, this));//stops movement

        //fight sequence
        CompositeNode fightSequence = new Sequence(bb);//sequence of actions when fighting
        BlueFightDecorator fightRoot = new (fightSequence, bb);//defines condition to enter fight sequence
        fightSequence.AddChild(new BlueShoot(bb));
        fightSequence.AddChild(new DelayNode(bb, 0.5f));

        //Wander sequence
        CompositeNode wanderSequence = new Sequence(bb);
        BlueWanderDecorator wanderRoot = new (wanderSequence, bb);
        wanderSequence.AddChild(new BlueWanderLocation(bb));
        wanderSequence.AddChild(new BlueMoveTo(bb, this));//moves to location
        wanderSequence.AddChild(new BlueWaitAtLocation(bb, this));//wait till reached destination
        wanderSequence.AddChild(new BlueStopMovement(bb, this));//stops movement




        //adding to root selector
        rootChild.AddChild(fleeRoot);
        rootChild.AddChild(fightRoot);
        rootChild.AddChild(wanderRoot);

        InvokeRepeating(nameof(ExecuteBT), 0.1f, 0.1f);
    }
    /**/
    // Update is called once per frame
    void Update()
    {
        if (bb.health > 0)
        {
            bb.red = bb.CheckEnemyTeam(bb.red, "Red");
            bb.isTargetVisible = bb.IsWithinFOV(bb.myTarget);
            Vector3 myPos = gameObject.transform.position;

            float nearestDistance = float.MaxValue;

            for (int i = 0; i < bb.red.Length; i++)
            {
                if (bb.red[i] != null)
                {
                    float tempDist = Vector3.Distance(myPos, bb.red[i].transform.position);

                    if (tempDist < nearestDistance)
                    {

                        for (int j = 0; j < bb.perception.sensedObjects.Length; j++)
                        {
                            if (bb.perception.sensedObjects[j] == bb.red[i])
                            {
                                bb.myTarget = bb.red[i];
                                bb.lastKnownLocation = bb.FindLastKnownPosition(bb.myTarget);

                                nearestDistance = tempDist;
                            }
                        }

                    }
                }
            }
            if (!bb.isMoving)
            {
                if (bb.myTarget != null)
                {
                    Vector3 lookPos = bb.LookPosSet(bb.myTarget.transform.position);
                    if (lookPos != Vector3.zero)
                    {
                        Quaternion rot = Quaternion.LookRotation(lookPos);
                        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * bb.turnSpeed * 10000);
                    }
                }

            }
        }
        else if (bb.health <= 0)
        {
            Destroy(gameObject);
        }
    }
    public void BlueMovement(Vector3 MoveLocation)
    {
        bb.isMoving = true;
        PathRequestManager.RequestPath(transform.position, MoveLocation, bb.OnPathFound);

    }
    public void StopMovement()
    {
        bb.isMoving = false;
        bb.isFleeing = false;
    }
    public void ExecuteBT()
    {
        BTRootNode.Execute();
    }

}
//Flee Sequence
public class BlueFleeDecorator : ConditionalDecorator
{
    private BlueBB bBB;

    public BlueFleeDecorator(BTNode WrappedNode, BlackBoard bb) : base(WrappedNode, bb)
    {
        bBB = (BlueBB)bb;
    }
    public override bool CheckStatus()
    {
        return bBB.isFleeing;
    }
}
public class CalculateFleeLocationBlue : BTNode
{
    private BlueBB bBB;
    public CalculateFleeLocationBlue(BlackBoard bb) : base(bb)
    {
        bBB = (BlueBB)bb;
    }
    public override BTStatus Execute()
    {
        //Debug.Log("entering flee loop");
        bBB.moveToLocation = bBB.FindCoverPoint(true);
        
        return BTStatus.SUCCESS;
    }
}
public class BlueMoveTo : BTNode
{
    private BlueBB bBB;
    private Blue blueRef;

    public BlueMoveTo(BlackBoard bb , Blue bleu) : base(bb)
    {
        bBB = (BlueBB)bb;
        blueRef = bleu;
    }
    public override BTStatus Execute()
    {

        blueRef.BlueMovement(bBB.moveToLocation);
        return BTStatus.SUCCESS;

    }
}
public class BlueWaitAtLocation : BTNode
{
    private BlueBB bBB;
    private Blue blueRef;
    public BlueWaitAtLocation(BlackBoard bb, Blue bleu) : base(bb)
    {
        bBB = (BlueBB)bb;
        blueRef= bleu;
    }
    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.RUNNING;
        //Debug.Log("waiting to reach destination");
        if (Vector3.Distance(blueRef.transform.position, bBB.moveToLocation) <= 2f)
        {
            rv = BTStatus.SUCCESS;
        }
        else if(bBB.isTargetVisible && !bBB.isFleeing)
        {
            rv = BTStatus.SUCCESS;
        }
        
        return rv;
    }
}

//Fight Sequence
public class BlueFightDecorator : ConditionalDecorator
{
    private BlueBB bBB;
    public BlueFightDecorator(BTNode WrappedNode, BlackBoard bb) : base(WrappedNode, bb)
    {
        bBB = (BlueBB)bb;
    }
    public override bool CheckStatus()
    {
        if(!bBB.isFleeing && bBB.isTargetVisible)
        {
            //Debug.Log("entering fight loop");
            return true;
        }
        else
        {
            return false;
        }
    }
}
public class BlueShoot : BTNode
{
    private BlueBB bBB;
    public BlueShoot(BlackBoard bb) : base(bb)
    {
        bBB = (BlueBB)bb;
    }
    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.SUCCESS;
        if(bBB.myTarget != null)
            bBB.Shooting();
        return rv;
    }
}
public class BlueStopMovement : BTNode
{
    private Blue blueRef;
    public BlueStopMovement(BlackBoard bb, Blue bleu) : base(bb)
    {
        blueRef = bleu;
    }
    public override BTStatus Execute()
    {
        blueRef.StopMovement();
        return BTStatus.SUCCESS;
    }
}

//Wander Sequence
public class BlueWanderDecorator : ConditionalDecorator
{
    private BlueBB bBB;
    public BlueWanderDecorator(BTNode WrappedNode, BlackBoard bb) : base(WrappedNode, bb)
    {
        bBB = (BlueBB)bb;
    }
    public override bool CheckStatus()
    {
        if (bBB.isTargetVisible && bBB.isFleeing)
        {
            
            return false;
        }
        else
        {
            //Debug.Log("entering wander loop");
            return true;
        }
    }
}
public class BlueWanderLocation : BTNode
{
    private BlueBB bBB;
    public BlueWanderLocation(BlackBoard bb) : base(bb)
    {
        bBB = (BlueBB)bb;
    }
    public override BTStatus Execute()
    {
        bBB.moveToLocation = bBB.lastKnownLocation;
        return BTStatus.SUCCESS;
    }
}