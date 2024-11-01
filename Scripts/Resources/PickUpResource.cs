using Godot;
using Godot.Collections;


public partial class PickUpResource : Resource
{
    [Export] public PackedScene pickUpObj;
    [Export] public Dictionary<string, int> statChange { get; set; }

}