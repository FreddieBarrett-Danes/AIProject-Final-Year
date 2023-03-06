//using System;
using System.Collections.Generic;
using System.Timers;

using UnityEngine;

//Execute and return one of three Enums
public enum BTStatus
{
    RUNNING,
    SUCCESS,
    FAILURE
}

//Base class, sets foundation for everything else
public abstract class BTNode
{
    protected BlackBoard bb;
    public BTNode(BlackBoard bb)
    {
        this.bb = bb;
    }
    public abstract BTStatus Execute();
    //Reset to be overriden in child classes as and when necessary
    //called when Node is abruptly aborted before can exit with success and failure
    public virtual void Reset()
    {

    }
}

//base class for node that can take child nodes. used in subclasses like selector and sequence
public abstract class CompositeNode : BTNode
{
    protected int CurrentChildIndex = 0;
    protected List<BTNode> children;
    public CompositeNode(BlackBoard bb) : base(bb)
    {
        children = new List<BTNode>();
    }
    public void AddChild(BTNode child)
    {
        children.Add(child);
    }

    //when composite node is reset it sets child index to 0, propogates down the rest to children
    public override void Reset()
    {
        CurrentChildIndex = 0;
        //Reset children
        for (int j = 0; j < children.Count; j++)
        {
            children[j].Reset();
        }
    }
}
/// <summary>
/// Selectors execute their children in order until a child succeeds, at which point it stops execution
/// If a child returns RUNNING, then it will need to stop execution but resume from the same point the next time it executes
/// </summary>
public class Selector : CompositeNode
{
    public Selector(BlackBoard bb) : base(bb)
    {

    }
    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.FAILURE;
        for (int j = CurrentChildIndex; j < children.Count; j++)
        {
            rv = children[j].Execute();
            if (rv == BTStatus.SUCCESS)
            {
                Reset();
                return BTStatus.SUCCESS;
            }
            else if (rv == BTStatus.RUNNING)
            {
                CurrentChildIndex = j;
                return BTStatus.RUNNING;
            }
            else if (rv == BTStatus.FAILURE && j == children.Count - 1)
            {
                Reset();
                return BTStatus.FAILURE;
            }
        }
        return rv;
    }
}
/// <summary>
/// Sequences execute their children in order until a child fails, at which point it stops execution
/// If a child returns RUNNING, then it will need to stop execution but resume from the same point the next time it executes
/// </summary>
public class Sequence : CompositeNode
{
    public Sequence(BlackBoard bb) : base(bb)
    {

    }
    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.SUCCESS;

        for (int j = CurrentChildIndex; j < children.Count; j++)
        {
            rv = children[j].Execute();
            if (rv == BTStatus.FAILURE)
            {
                Reset();
                return BTStatus.FAILURE;
            }
            else if (rv == BTStatus.RUNNING)
            {
                CurrentChildIndex = j;
                return BTStatus.RUNNING;
            }
            else if (rv == BTStatus.SUCCESS && j == children.Count - 1)
            {
                Reset();
                return BTStatus.SUCCESS;
            }
        }

        return rv;
    }
} 

/// <summary>
/// Decorator nodes customise functionality of other nodes by wrapping around them, see InverterDecorator for example
/// </summary>
public abstract class DecoratorNode : BTNode
{

    protected BTNode WrappedNode;
    public DecoratorNode(BTNode WrappedNode, BlackBoard bb) : base(bb)
    {
        this.WrappedNode = WrappedNode;
    }

    public BTNode GetWrappedNode()
    {
        return WrappedNode;
    }

    //reset wrapped node
    public override void Reset()
    {
        WrappedNode.Reset();
    }
}
   

/// <summary>
/// Inverter decorator simply inverts the result of success/failure of the wrapped node
/// </summary>
public class InverterDecorator : DecoratorNode
{
    public InverterDecorator(BTNode WrappedNode, BlackBoard bb) : base(WrappedNode, bb)
    {

    }
    public override BTStatus Execute()
    {
        BTStatus rv = WrappedNode.Execute();
        if (rv == BTStatus.FAILURE)
        {
            rv = BTStatus.SUCCESS;
        }
        else if (rv == BTStatus.SUCCESS)
        {
            rv = BTStatus.FAILURE;
        }
        return rv;
    }
}

/// <summary>
/// Inherit this and override CheckStatus. If that returns true, then it will execute the WrappedNode otherwise it will return failure
/// </summary>
public abstract class ConditionalDecorator : DecoratorNode
{
    public ConditionalDecorator(BTNode WrappedNode, BlackBoard bb) : base(WrappedNode, bb)
    {

    }
    public abstract bool CheckStatus();
    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.FAILURE;

        if(CheckStatus())
        {
            rv = WrappedNode.Execute();
        }
        
        return rv;
    }
}

/// <summary>
/// This node simply returns success after the allotted delay time has passed
/// </summary>
public class DelayNode : BTNode
{
    protected float delay = 0.0f;
    bool started = false;
    private Timer regulator;
    bool delayFinished = false;

    public DelayNode(BlackBoard bb, float DelayTime) : base(bb)
    {
        this.delay = DelayTime;
        regulator = new Timer(delay * 1000.0f); // in milliseconds, so multiply by 1000
        regulator.Elapsed += OnTimedEvent;
        regulator.Enabled = true;
        regulator.Stop();
    }

    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.RUNNING;

        if (!started && !delayFinished)
        {
            started = true;
            regulator.Start();
        }
        else if (delayFinished)
        {
            delayFinished = false;
            started = false;
            rv = BTStatus.SUCCESS;
        }

        return rv;
    }

    private void OnTimedEvent(object sender, ElapsedEventArgs e)
    {
        started = false;
        delayFinished = true;
        regulator.Stop();
    }
    //Timers count down independently of the Behaviour Tree, so we need to stop them when the behaviour is aborted/reset
    public override void Reset()
    {
        regulator.Stop();
        delayFinished = false;
        started = false; 
    }
}




