using Godot;
using Godot.Collections;

[GlobalClass]
public partial class PickUpResource : Resource
{
    [Export] public Dictionary<string, int> statChange { get; set; }

}
