using Godot;


public partial class LootTable : Resource
{
    [Export] public PackedScene[] DropItems { get; set; }
    [Export] public float[] DropChances { get; set; }

    public PackedScene GetDroppedItem()
    {
        // Check if arrays are empty or mismatched
        if (DropItems == null || DropChances == null || DropItems.Length == 0 || DropItems.Length != DropChances.Length)
            return null;

        float roll = (float)GD.Randf();  // Random value between 0 and 1
        float cumulative = 0.0f;

        for (int i = 0; i < DropChances.Length; i++)
        {
            cumulative += DropChances[i];

            if (roll < cumulative)
            {
                return DropItems[i]; // Return the dropped item
            }
        }

        return null; // In case of an error
    }
}