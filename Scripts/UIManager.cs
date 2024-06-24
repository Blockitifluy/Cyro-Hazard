using System;
using Godot;

public partial class UIManager : Control
{
	private Player ConnectedPlayer;
	[Export] public Label StaminaLabel { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ConnectedPlayer = GetNode<Player>("/root/Main/Player");
		StaminaLabel = GetNode<Label>("StaminaLabel");
	}

	private void UpdateStamina()
	{
		if (StaminaLabel == null)
		{
			throw new Exception("StaminaLabel doesn't exist");
		}
		
		float stamina = Mathf.Round(ConnectedPlayer.Stamina),
		staminaMax = Mathf.Round(ConnectedPlayer.StaminaMax);

		StaminaLabel.Text = $"Stamina: {stamina} / {staminaMax}";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		UpdateStamina();
	}
}
