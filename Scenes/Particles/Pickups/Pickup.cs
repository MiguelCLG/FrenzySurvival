using Godot;

public partial class Pickup : Node2D
{
  AnimationPlayer animationPlayer;
  [Export] public PickUpResource pickUp;

  public override void _Ready()
  {
    // Get the AnimationPlayer node
    AnimationPlayer animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    AnimationLibrary animationLibrary = new();

    // Create a new Animation
    Animation animation = new()
    {
      Length = .7f,
    };

    // Add a Value track for the position property (assuming this script is on a Node2D or similar)
    int trackIndex = animation.AddTrack(Animation.TrackType.Value);

    // Set the path of the property you want to animate (e.g., "position")
    animation.TrackSetPath(trackIndex, ".:position");

    // Insert keyframes for position changes at different times
    animation.TrackInsertKey(trackIndex, 0f, new Vector2(GlobalPosition.X + -20, GlobalPosition.Y));
    animation.TrackInsertKey(trackIndex, 0.3f, new Vector2(GlobalPosition.X + -10, GlobalPosition.Y + -20));
    animation.TrackInsertKey(trackIndex, .7f, new Vector2(GlobalPosition.X, GlobalPosition.Y));

    // Add the animation to the AnimationPlayer with a unique name
    string animationName = "move_animation";
    animationLibrary.AddAnimation(animationName, animation);

    animationPlayer.AddAnimationLibrary(animationName, animationLibrary);
    // Play the animation
    animationPlayer.Play($"{animationName}/{animationName}");
  }

  /// <summary>
  /// During the implementation of the Experience pickup, there was a
  /// need to have the resource be updated from the Mob.
  /// The pickup resource is then local to scene, so that we can change its values
  /// on runtime.
  /// This means that the values of the resoruce can be changed.
  /// </summary>
  /// <param name="key"></param>
  /// <param name="value"></param>
  public void ChangeStatChangeValue(string key, int value)
  {
    if (pickUp.statChange.ContainsKey(key))
      pickUp.statChange[key] = value;
  }



  public void OnBodyEntered(Node2D body)
  {
    if (body.IsInGroup("Player"))
    {
      EventRegistry.GetEventPublisher("IncreaseStatsFromDictionary").RaiseEvent(new object[] { pickUp.statChange, this });
      QueueFree();
    }
  }
}
