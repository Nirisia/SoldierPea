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
            new Sequence(new List<Node>
            {
                new SeeEnemyTask(owner),
                new AttackTask(owner),
                new RepairingTask(owner)
            }),
            new Sequence(new List<Node>
            {
                new SeePointTask(owner),
                new CaptureTask(owner)
            }),
            new Sequence(),
        });
        return root;
    }
}
