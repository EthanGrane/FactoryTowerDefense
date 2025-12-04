using System.Collections.Generic;
using UnityEngine;

public class LogicManager : MonoBehaviour
{
    public static LogicManager Instance { get; private set; }

    private List<BuildingLogic> logics = new List<BuildingLogic>();

    public const int TICKS_PER_SECOND = 60;
    private float tickLength => 1f / TICKS_PER_SECOND;
    private float accumulator = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void Register(BuildingLogic logic)
    {
        logics.Add(logic);
        logic.Initialize(logic.building.block);
    }

    public void Unregister(BuildingLogic logic)
    {
        logics.Remove(logic);
    }
    
    void Update()
    {
        accumulator += Time.deltaTime;

        while (accumulator >= tickLength)
        {
            accumulator -= tickLength;

            foreach (var logic in logics)
                if (logic.update)
                    logic.Tick();
        }
    }
}
