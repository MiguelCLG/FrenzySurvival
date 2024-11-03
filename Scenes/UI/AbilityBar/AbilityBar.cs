using Godot;
using System;

public partial class AbilityBar : Control
{
  Control horizontalBar;
  [Export] PackedScene abilitySlot;

  public override void _Ready()
  {
    horizontalBar = GetNode<Control>("%HorizontalBar");
    EventSubscriber.SubscribeToEvent("AbilitySelected", AbilitySelected);

  }

  public void AbilitySelected(object sender, object[] args)
  {
    AbilitySlot ability = abilitySlot.Instantiate<AbilitySlot>();
    horizontalBar.AddChild(ability);
    var abilityScene = (args[0] as PackedScene).Instantiate<Ability>();
    ability.SpawnUIIcon(abilityScene.abilityResource.Icon);
    abilityScene.QueueFree();
  }

  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("AbilitySelected", AbilitySelected);

  }

}
