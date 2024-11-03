using Godot;
using System;

public partial class LevelUpUi : CanvasLayer
{
  [Export] PackedScene abilityRow;
  [Export] PackedScene abilityUIButton;
  [Export] PlayerResource playerResource;
  public override void _Ready()
  {
    RefetchAbilities();
    EventSubscriber.SubscribeToEvent("AbilitySelected", AbilitySelected);
    EventSubscriber.SubscribeToEvent("OnLevelUp", OnLevelUp);

  }
  public void RemoveAbilitiesFromContainer()
  {
    var verticalContainer = GetNode<Control>("%VerticalContainer");
    foreach (Control child in verticalContainer.GetChildren())
    {
      if (child is HBoxContainer row)
      {
        verticalContainer.RemoveChild(row);
        row.QueueFree();
      }
    }
  }

  public void RefetchAbilities()
  {
    RemoveAbilitiesFromContainer();
    var abilitiesToShow = playerResource.Abilities.GetRandomAbilities(3, false);
    var verticalContainer = GetNode<Control>("%VerticalContainer");

    Control row = abilityRow.Instantiate<Control>();
    verticalContainer.AddChild(row);
    foreach (PackedScene abilityScene in abilitiesToShow)
    {
      var ability = abilityScene.Instantiate<Ability>();

      AbilityUiButton uiButton = abilityUIButton.Instantiate<AbilityUiButton>();
      row.AddChild(uiButton);
      uiButton.SetInitialValues(abilityScene, ability.abilityResource.Icon, ability.abilityResource.Name, ability.abilityResource.Cooldown.ToString(), ability.abilityResource.CastTime.ToString(), ability.abilityResource.Damage.ToString(), ability.abilityResource.kiRequired.ToString());
      ability.QueueFree();
    }
  }
  public void OnLevelUp(object sender, object[] args)
  {
    GetTree().Paused = true;
    Visible = true;
    RefetchAbilities();
  }
  public void AbilitySelected(object sender, object[] args)
  {
    GetTree().Paused = false;
    Visible = false;
  }
}
