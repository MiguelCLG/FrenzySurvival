using Godot;

public partial class ExperienceBar : ProgressBar
{
  private Label levelLbl;
  [Export] public float Experience { get; set; } = 0;
  [Export] public float MaxExperience { get; set; } = 100;
  public override void _Ready()
  {
    levelLbl = GetNode<Label>("LevelLbl");
    EventRegistry.RegisterEvent("OnExperienceChanged");
    EventSubscriber.SubscribeToEvent("OnExperienceChanged", OnExperienceChanged);
    EventRegistry.RegisterEvent("OnLevelUp");
    EventSubscriber.SubscribeToEvent("OnLevelUp", SetLevelLabel);
    EventRegistry.RegisterEvent("SetInitialExpValue");
    EventSubscriber.SubscribeToEvent("SetInitialExpValue", SetInitialExpValue);
  }

  private void SetLevelLabel(object sender, object[] args)
  {
    levelLbl.Text = args[0].ToString();
  }

  private void SetInitialExpValue(object sender, object[] args)
  {
    if (args[0] is int newExp && args[1] is int MaxExpLevelAnterior && args[2] is int MaxXpBarra)
    {
      Value = ConvertXPToPercentage(newExp, MaxExpLevelAnterior, MaxXpBarra);
      MaxValue = 100;
    }
  }

  private void OnExperienceChanged(object sender, object[] args)
  {
    // newExp, MaxExpLevelAnterior, MaxXpBarra
    if(args[0] is int newExp)
    {
      if(args[1] is int MaxExpLevelAnterior)
      {
          if(args[2] is int MaxXpBarra)
          {
            GD.Print($"newExp[{newExp}],  MaxExpLevelAnterior[{MaxExpLevelAnterior}], MaxXpBarra[{MaxXpBarra}]");
            //var percentage = ConvertXPToPercentage(newExp, MaxExpLevelAnterior, MaxXpBarra);
            var percentage = GetLevelProgress(newExp, MaxExpLevelAnterior, MaxXpBarra);
            GD.Print($"percentage[{percentage}]");
            Value = percentage;
          } 
      } 
    } 
    /* if (args[0] is int expValue)
    {
      GD.Print(expValue);
      Experience += expValue;
      UpdateUI();
    } */
  }

  public float ConvertXPToPercentage(int newExp, int MaxExpLevelAnterior, int MaxXpBarra)
  {
    //newExp, MaxExpLevelAnterior, MaxXpBarra
    //10 , 0, 20
    var xpRelativeToLevel = newExp - MaxExpLevelAnterior; // 10
    var BarMaxXp = MaxXpBarra - MaxExpLevelAnterior; // 20
    return xpRelativeToLevel <=0 ? 0: BarMaxXp / xpRelativeToLevel;
  }

  public float GetLevelProgress(int currentExp, int minExp, int maxExp)
  {
    // Check if the current experience falls within the level's range
    if (currentExp >= minExp && currentExp <= maxExp)
    {
        // Calculate the progress towards the next level
        int nextLevelMinExp = maxExp + 1;
        float progress = (currentExp - minExp) / (float)(nextLevelMinExp - minExp);
        return Mathf.Clamp(progress * 100, 0, 100); // Returns percentage
    }
    return 0; // Default if no level found (for below level 1)
  }

  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("OnExperienceChanged", OnExperienceChanged);
    EventSubscriber.UnsubscribeFromEvent("SetInitialExpValue", SetInitialExpValue);
    EventSubscriber.UnsubscribeFromEvent("OnLevelUp", SetLevelLabel);
    EventRegistry.UnregisterEvent("OnLevelUp");
    EventRegistry.UnregisterEvent("SetInitialExpValue");
    EventRegistry.UnregisterEvent("OnExperienceChanged");
  }
}
