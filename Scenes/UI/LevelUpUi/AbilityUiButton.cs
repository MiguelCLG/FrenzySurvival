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

  public override void _Ready()
  {
    iconUI = GetNode<TextureRect>("%IconUI");
    nameUI = GetNode<Label>("%NameUI");
    coolDownUI = GetNode<Label>("%CooldownUI");
    castTimeUI = GetNode<Label>("%CastTimeUI");
    damageUI = GetNode<Label>("%DamageUI");
    kiRequiredUI = GetNode<Label>("%KIRequiredUI");
  }
  public void SetInitialValues(Texture2D icon, string name, string coolDown, string castTime, string damage, string kiRequired)
  {
    iconUI.Texture = icon;
    nameUI.Text = name;
    coolDownUI.Text = coolDown;
    castTimeUI.Text = castTime;
    damageUI.Text = damage;
    kiRequiredUI.Text = kiRequired;
  }
  public void OnClickButton()
  {

    EventRegistry.GetEventPublisher("AbilitySelected").RaiseEvent(new object[] { nameUI.Text });
  }

  public override void _ExitTree()
  {
  }
}
