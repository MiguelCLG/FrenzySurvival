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


    public List<PackedScene> GetRandomAbilities(int numbetToGet, bool isSuper)
    {
        if (Abilities == null || Abilities.Count == 0 || numbetToGet <= 0)
            return new List<PackedScene>();

        var random = new System.Random();
        var abilitiesList = new List<PackedScene>(Abilities.Values);
        var filteredAbilities = new List<PackedScene>();

        // Filter abilities based on the isSuper flag
        foreach (PackedScene ability in abilitiesList)
        {
            var instantiatedAbility = ability.Instantiate<Ability>();
            if (instantiatedAbility.abilityResource.isSuperAbility == isSuper)
                filteredAbilities.Add(ability);

            instantiatedAbility.QueueFree();
        }

        // Randomly select abilities from the filtered list
        var selectedAbilities = new List<PackedScene>();
        for (int i = 0; i < numbetToGet && filteredAbilities.Count > 0; i++)
        {
            int index = random.Next(filteredAbilities.Count);
            selectedAbilities.Add(filteredAbilities[index]);
            filteredAbilities.RemoveAt(index); // Remove to prevent duplicates
        }

        return selectedAbilities;
    }


}
