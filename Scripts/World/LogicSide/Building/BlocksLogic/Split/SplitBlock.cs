using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Split Block", menuName = "FACTORY/Block/Split Block")]
public class SplitBlock : Block<SplitLogic>
{
    public int ticksToMoveConveyorItem = 60;
}
