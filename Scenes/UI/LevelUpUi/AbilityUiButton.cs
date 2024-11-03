using Godot;
using System;

public partial class AbilityUiButton : Control
{
  [Export] public TextureRect iconUI;
  [Export] public Label nameUI;
  [Export] public Label coolDownUI;
  [Export] public Label castTimeUI;
  [Export] public Label damageUI;
  [Export] public Label kiRequiredUI;

  PackedScene abilityScene;


  public override void _Ready()
  {
    iconUI = GetNode<TextureRect>("%IconUI");
    nameUI = GetNode<Label>("%NameUI");
    coolDownUI = GetNode<Label>("%CooldownUI");
    castTimeUI = GetNode<Label>("%CastTimeUI");
    damageUI = GetNode<Label>("%DamageUI");
    kiRequiredUI = GetNode<Label>("%KIRequiredUI");
  }
  public void SetInitialValues(PackedScene ab, Texture2D icon, string name, string coolDown, string castTime, string damage, string kiRequired)
  {
    abilityScene = ab;
    iconUI.Texture = icon;
    nameUI.Text = name;
    coolDownUI.Text = $"Cooldown: {coolDown}";
    castTimeUI.Text = $"Cast Time: {castTime}";
    damageUI.Text = $"Damage: {damage}";
    kiRequiredUI.Text = $"KI Required: {kiRequired}";
  }
  public void OnClickButton()
  {
    GD.Print($"An ability was selected: {nameUI.Text}");

    EventRegistry.GetEventPublisher("AbilitySelected")?.RaiseEvent(new object[] { abilityScene });
  }

  public override void _ExitTree()
  {
  }
}
