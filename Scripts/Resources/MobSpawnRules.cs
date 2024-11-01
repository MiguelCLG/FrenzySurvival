using Godot;
using Godot.Collections;


public partial class MobSpawnRules : Resource
{
    [Export] public float StartTimeInSeconds { get; set; }
    [Export] public float FinishTimeInSeconds { get; set; }

    // Export a dictionary where keys are max units per type and values are the unit resources to spawn
    [Export] public BaseCharacterResource[] MobsToSpawn { get; set; }
    [Export] public int[] SpawnNumbers { get; set; }

    // Method to validate spawn rules for each unit type
    public Dictionary<int, BaseCharacterResource> GetUnitsToSpawn(double currentTime)
    {
        //GD.Print($"StartTimeInSeconds[{StartTimeInSeconds}] - currentTime[{currentTime}] - FinishTimeInSeconds[{FinishTimeInSeconds}]");
        //if(StartTimeInSeconds > currentTime || currentTime > FinishTimeInSeconds)
        if(StartTimeInSeconds <= currentTime && currentTime <= FinishTimeInSeconds)
        {
            Dictionary<int, BaseCharacterResource> mobsToSpawn = new();
            for (int i = 0; i < MobsToSpawn.Length; i++)
                mobsToSpawn.Add(SpawnNumbers[i], MobsToSpawn[i]);    
            return mobsToSpawn;
        }
        return new();
    }
}