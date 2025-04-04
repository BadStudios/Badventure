using Godot;
using System;

public partial class Menu : Control
{
	public override void _Ready()
	{
		var playButton = GetNode<Button>("Buttons/PlayButton");
		var settingsButton = GetNode<Button>("Buttons/SettingsButton");
		var exitButton = GetNode<Button>("Buttons/ExitButton");

		playButton.Pressed += OnPlayPressed;
		settingsButton.Pressed += OnSettingsPressed;
		exitButton.Pressed += OnExitPressed;
	}

	private void OnPlayPressed()
	{
		var seedInput = GetNode<LineEdit>("Buttons/SeedContainer/Seed");
		GD.Print(seedInput);
		//GlobalData.Instance.Seed = Convert.ToInt32(seedInput.Text);
		GetTree().ChangeSceneToFile("res://Scenes/Game/MainScene.scn");
	}

	private void OnSettingsPressed()
	{
		GetTree().ChangeSceneToFile("res://path/to/SettingsScene.tscn");
	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}
}
