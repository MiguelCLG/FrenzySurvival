using Godot;

public partial class PauseScreen : PanelContainer
{
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (Input.IsActionPressed("ui_cancel"))
        {
            GD.Print("Handle cancel");
            HandlePause();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionPressed("ui_cancel") && @event is InputEventJoypadButton)
        {
            HandlePause();
        }
    }

    private void HandlePause()
    {
        GetTree().Paused = !GetTree().Paused;
        this.Visible = !GetTree().Paused;
        if (GetTree().Paused)
        {
            this.Visible = true;
            GetNode<AudioManager>("/root/AudioManager").PauseAllSoundsFromBus("Sound Effects");
        }
        else
        {
            this.Visible = false;
            GetNode<AudioManager>("/root/AudioManager").UnpauseAllSounds();  // TODO: Unpause all from bus.
        }
    }
}
