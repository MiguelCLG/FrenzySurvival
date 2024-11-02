using Godot;
using Godot.Collections;
using System.Collections.Generic;

[GlobalClass]
public partial class LevelTableResource : Resource
{
    [Export] public Godot.Collections.Dictionary<int, Array<int>> levels { get; set; }

    public List<int> GetLevelUps(int currentExp, int newExp)
    {
        List<int> levelUps = new List<int>();
        int currentLevel = GetCurrentLevel(currentExp);
        
        // Check each level’s exp requirement to see if it’s reached with newExp
        foreach (var level in levels)
        {
            int minExp = level.Value[0];
            if (level.Key > currentLevel && newExp >= minExp)
            {
                levelUps.Add(level.Key);
            }
        }
        return levelUps;
    }

    public int GetCurrentLevel(int currentExp)
    {
        int currentLevel = 0;
       // Find the current level based on current experience
        foreach (var level in levels)
        {
            int minExp = level.Value[0];
            int maxExp = level.Value[1];
            if (currentExp >= minExp && currentExp <= maxExp)
            {
                currentLevel = level.Key;
                break;
            }
        }
        return currentLevel;
    }

}
