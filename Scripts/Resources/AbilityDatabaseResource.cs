using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class AbilityDatabaseResource : Resource
{
    [Export] public Godot.Collections.Dictionary<string, PackedScene> Abilities { get; set; }

    public List<PackedScene> GetRandomAbilities(int numbetToGet)
    {
        if (Abilities == null || Abilities.Count == 0 || numbetToGet <= 0)
            return new List<PackedScene>();

        var random = new System.Random();
        var abilitiesList = new List<PackedScene>(Abilities.Values);

        var selectedAbilities = new List<PackedScene>();

        for (int i = 0; i < numbetToGet && abilitiesList.Count > 0; i++)
        {
            int index = random.Next(abilitiesList.Count);
            selectedAbilities.Add(abilitiesList[index]);
            abilitiesList.RemoveAt(index); // Prevent duplicates
        }
        return selectedAbilities;
    }

}
