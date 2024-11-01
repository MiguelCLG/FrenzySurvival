using Godot;
using Godot.Collections;

[GlobalClass]
public partial class PickUpResource : Resource
{
    [Export] public PackedScene pickUpObj;
    [Export] public Dictionary<string, int> statChange { get; set; }

}
