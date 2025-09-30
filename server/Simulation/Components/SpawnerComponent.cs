using System;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Spawner, NetworkSync = true)]
public struct SpawnerComponent(int unitId, TimeSpan interval, int amountPerInterval, int maxPopulation, int minPopulation)
{
    public long ChangedTick = NttWorld.Tick;
    public readonly int UnitIdToSpawn = unitId;

    // Spawn Interval 
    public readonly TimeSpan Interval = interval;

    public float TimeSinceLastSpawn = 0;

    // Amount of UnitType to spawn each Interval
    public readonly int AmountPerInterval = amountPerInterval;

    // Limit of living UnitIdToSpawn after which Interval will be ignored
    public readonly int MaxPopulation = maxPopulation;

    // The idea is that when there's less than MinPopulation of living UnitIdToSpawn,
    // Interval will be ignored and units will be spawned each frame until 
    // MinPopulation of alive UnitIdToSpawn has been reached
    public readonly int MinPopulation = minPopulation;


}