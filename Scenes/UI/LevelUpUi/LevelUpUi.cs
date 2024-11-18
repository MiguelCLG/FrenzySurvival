using Godot;
using System.Collections.Generic;

public partial class LevelUpUi : CanvasLayer
{
  [Export] PackedScene abilityRow;
  [Export] PackedScene abilityUIButton;
  [Export] PlayerResource playerResource;
  private int currentLevel = 1;
  [Export] Godot.Collections.Dictionary<string, AudioOptionsResource> levelUpSelectionSounds;
  private AudioManager audioManager;

  public override void _Ready()
  {
    audioManager = GetNode<AudioManager>("/root/AudioManager");
    RefetchAbilities();
    EventSubscriber.SubscribeToEvent("AbilitySelected", AbilitySelected);
    EventSubscriber.SubscribeToEvent("OnLevelUp", OnLevelUp);

  }

  public void SetPlayerResource(PlayerResource resource)
  {
    playerResource = resource;
    RefetchAbilities();
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
    var abilitiesToShow = playerResource.Abilities.GetRandomAbilities(3, currentLevel % 2 == 0);
    var verticalContainer = GetNode<Control>("%VerticalContainer");

    Control row = abilityRow.Instantiate<Control>();
    verticalContainer.AddChild(row);
    foreach (PackedScene abilityScene in abilitiesToShow)
    {
      var ability = abilityScene.Instantiate<Ability>();

      AbilityUiButton uiButton = abilityUIButton.Instantiate<AbilityUiButton>();
      row.AddChild(uiButton);
      uiButton.SetInitialValues(abilityScene, ability.abilityResource.Icon, ability.abilityResource.Name, ability.abilityResource.Cooldown.ToString(), ability.abilityResource.CastTime.ToString(), ability.abilityResource.Damage.ToString(), ability.abilityResource.kiRequired.ToString());
      uiButton.GetNode<Button>("%Button").Connect("mouse_entered", Callable.From(() => OnMouseEntered()));
      ability.QueueFree();
    }
  }
  public void OnMouseEntered()
  {
    audioManager?.Play(levelUpSelectionSounds.GetValueOrDefault("HoverClick"), this);
  }
  public void OnLevelUp(object sender, object[] args)
  {
    currentLevel = (int)args[0];
    GetTree().Paused = true;
    Visible = true;
    RefetchAbilities();
  }
  public void AbilitySelected(object sender, object[] args)
  {
    audioManager?.Play(levelUpSelectionSounds.GetValueOrDefault("HoverClick"), this);
    GetTree().Paused = false;
    Visible = false;
  }

  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("AbilitySelected", AbilitySelected);
    EventSubscriber.UnsubscribeFromEvent("OnLevelUp", OnLevelUp);
  }
}
