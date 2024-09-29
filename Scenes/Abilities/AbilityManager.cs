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
    EventSubscriber.SubscribeToEvent("ActionFinished", DoNextAction);


  }

  public void DoNextAction()
  {
    if (actionindex > array.Count - 1)
    {
      actionindex = 0;

      EventRegistry.GetEventPublisher("OnComboFinished").RaiseEvent(new object[] { });
    }
    else
    {
      array.ElementAt(actionindex).Action();
      actionindex++;
    }
  }
  public void DoNextAction(object sender, object[] args)
  {
    if (actionindex > array.Count - 1)
    {
      actionindex = 0;
      EventRegistry.GetEventPublisher("OnComboFinished").RaiseEvent(new object[] { });

    }
    else
    {
      array.ElementAt(actionindex).Action();
      actionindex++;
    }
  }

  //TODO: Make a chain of events wait for each other, try to have a NextAction function
}
