
using Godot;
using System;

public partial class AudioOptionsScreen : PanelContainer
{
    [Export] private Slider SliderMaster;
    [Export] private Slider SliderMusic;
    [Export] private Slider SliderSFX;
    [Export] private Slider SliderUIFX;

	public override void _Ready()
	{
		SliderMaster.Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master"));
		SliderMusic.Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Music"));
		SliderSFX.Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Sound Effects"));
		SliderUIFX.Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Ui Effects"));
		
		// Connect slider value changes to a handler method
		SliderMaster.ValueChanged += OnSliderMasterValueChanged;
		SliderMusic.ValueChanged += OnSliderMusicValueChanged;
		SliderSFX.ValueChanged += OnSliderSFXValueChanged;
		SliderUIFX.ValueChanged += OnSliderUIFXValueChanged;
	}

    private void OnSliderMasterValueChanged(double value)
    {
		GD.Print($"Master changed to:[{value}]");
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), (float)value);
    }

    private void OnSliderMusicValueChanged(double value)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), (float)value);
    }

    private void OnSliderSFXValueChanged(double value)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Sound Effects"), (float)value);
    }

    private void OnSliderUIFXValueChanged(double value)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Ui Effects"), (float)value);
    }
}