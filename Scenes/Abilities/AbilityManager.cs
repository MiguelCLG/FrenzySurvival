using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class AbilityManager : Node2D
{
  public AnimatedSprite2D AnimationPlayer;
  public Array<Ability> array = new Array<Ability>();

  public int actionindex = 0;


  public override void _Ready()
  {
    AnimationPlayer = GetParent().GetNode<AnimatedSprite2D>("Portrait");
    foreach (Ability child in GetChildren())
    {
      child.AnimationPlayer = AnimationPlayer;
      array.Add(child);
    }
    EventRegistry.RegisterEvent("ActionFinished");
    EventSubscriber.SubscribeToEvent("ActionFinished", DoNextActionAsync);


  }

  public void DoNextActionAsync()
  {
    if (actionindex > array.Count - 1)
    {
      actionindex = 0;

      EventRegistry.GetEventPublisher("OnComboFinished").RaiseEvent(new object[] { });
    }
    else
    {
      array[actionindex].Action();
      actionindex++;
    }
  }

  public void DoNextActionAsync(object sender, object[] args)
  {
    if (actionindex > array.Count - 1)
    {
      actionindex = 0;
      EventRegistry.GetEventPublisher("OnComboFinished").RaiseEvent(new object[] { });

    }
    else
    {
      array[actionindex].Action();
      actionindex++;
    }
  }

  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("ActionFinished", DoNextActionAsync);
  }
}
