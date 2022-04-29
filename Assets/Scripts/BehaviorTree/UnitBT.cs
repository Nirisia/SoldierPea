using System.Collections.Generic;
using BehaviorTree;

public class UnitBT : Tree
{
    public static Unit owner;

    protected override Node SetupTree()
    {
        owner = this.GetComponent<Unit>();

        Node root = new Selector(new List<Node> 
        {
            new CaptureTask(owner),
            new AttackTask(owner),
            new Sequence(),
        });
        return root;
    }
}
