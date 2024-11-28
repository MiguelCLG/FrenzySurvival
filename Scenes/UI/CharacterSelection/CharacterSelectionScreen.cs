using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
public partial class CharacterSelectionScreen : Control
{

  [Export] Array<CharacterSelectionResource> CharacterSelectionResource;
  [Export] PackedScene CharacterSelectScene;
  public GridContainer CharacterSelectionContainer { get; set; }
  [Export] Godot.Collections.Dictionary<string, AudioOptionsResource> characterSelectionSounds;
  private AudioManager audioManager;
  private List<Button> characterButtons = new();

  public override void _Ready()
  {
    CharacterSelectionContainer = GetNode<GridContainer>("%GridContainer");
    RefetchCharacters();
    audioManager = GetNode<AudioManager>("/root/AudioManager");
    DelayedGrabFocus(0.2f);
  }

  public async void DelayedGrabFocus(float waitTime)
  {
    GD.Print("Before CharacterSelection timer");
    await ToSignal(GetTree().CreateTimer(waitTime, true, true), "timeout");
    GD.Print("After CharacterSelection timer");
    //CharacterSelectionContainer
    //  .GetChildren()
    //  .OfType<Control>()
    //  .FirstOrDefault()
    //  .GetNode<Button>("%Button")
    //  .GrabFocus();
    GD.Print(characterButtons.FirstOrDefault().Name);
    characterButtons.FirstOrDefault().GrabFocus();  // we fetch a button to select the focus.
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
      character.GetNode<Button>("%Button").Connect("mouse_entered", Callable.From(() => OnMouseEntered(character.GetNode<Button>("%Button"))));
      character.GetNode<Button>("%Button").Connect("focus_entered", Callable.From(() => OnMouseEntered(character.GetNode<Button>("%Button"))));
      characterButtons.Add(character.GetNode<Button>("%Button")); // We add the button to the list to have a reference to it.
    }
  }

  private void OnInputEvent(InputEvent @event)
  {
    Node focusOwner = GetViewport().GuiGetFocusOwner();
    if (@event is InputEventKey eventKey && eventKey.Pressed && eventKey.IsAction("ui_accept"))
    {
      // This gets the node that has focus, and if its a button it will press it.
      if (focusOwner is Button button)
      {
        button.EmitSignal("pressed");
      }
    }
  }

  public void OnMouseEntered(Button button)
  {
    button.GrabFocus();
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
