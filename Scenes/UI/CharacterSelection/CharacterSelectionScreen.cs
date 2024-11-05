using Godot;
using Godot.Collections;

public partial class CharacterSelectionScreen : Control
{

  [Export] Array<CharacterSelectionResource> CharacterSelectionResource;
  [Export] PackedScene CharacterSelectScene;
  public GridContainer CharacterSelectionContainer { get; set; }
  public override void _Ready()
  {
    CharacterSelectionContainer = GetNode<GridContainer>("%GridContainer");
    RefetchCharacters();
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
    }
  }

  public void OnClickButton(CharacterSelectionResource characterInformation)
  {
    GD.Print($"A character was selected: {characterInformation.Name}");
    EventRegistry.GetEventPublisher("CharacterSelected").RaiseEvent(new object[] { characterInformation });
    Visible = false;
  }
}
