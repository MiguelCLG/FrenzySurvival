using Godot;
using Godot.Collections;
using System.Collections.Generic;
public partial class CharacterSelectionScreen : Control
{

  [Export] Array<CharacterSelectionResource> CharacterSelectionResource;
  [Export] PackedScene CharacterSelectScene;
  public GridContainer CharacterSelectionContainer { get; set; }
  [Export] Godot.Collections.Dictionary<string, AudioOptionsResource> characterSelectionSounds;
  private AudioManager audioManager;

  public override void _Ready()
  {
    CharacterSelectionContainer = GetNode<GridContainer>("%GridContainer");
    RefetchCharacters();
    audioManager = GetNode<AudioManager>("/root/AudioManager");
  }

  public void RefetchCharacters()
  {
    foreach (var characterInformation in CharacterSelectionResource)
    {
      Control character = CharacterSelectScene.Instantiate<Control>();
      CharacterSelectionContainer.AddChild(character);
      character.GetNode<TextureRect>("%IconUI").Texture = characterInformation.Portrait;
      character.GetNode<Label>("%NameLabel").Text = characterInformation.Name;
      character.GetNode<Button>("%Button").Connect("pressed", Callable.From(() => OnClickButton(characterInformation)));
      character.GetNode<Button>("%Button").Connect("mouse_entered", Callable.From(() => OnMouseEntered()));
    }
  }

  public void OnMouseEntered()
  {
    audioManager?.Play(characterSelectionSounds.GetValueOrDefault("HoverClick"), this);
  }
  public void OnClickButton(CharacterSelectionResource characterInformation)
  {
    GD.Print($"A character was selected: {characterInformation.Name}");
    EventRegistry.GetEventPublisher("CharacterSelected").RaiseEvent(new object[] { characterInformation });
    audioManager?.Play(characterSelectionSounds.GetValueOrDefault("HoverClick"), this);
    Visible = false;
  }
}
