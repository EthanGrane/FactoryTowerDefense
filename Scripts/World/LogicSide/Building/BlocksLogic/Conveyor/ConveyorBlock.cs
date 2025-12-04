using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Conveyor Block", menuName = "FACTORY/Block/Conveyor Block")]
public class ConveyorBlock : Block<ConveyorLogic>
{
    public int ticksToMoveConveyorItem = 60;
}
