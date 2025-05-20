using System;
using Godot;

namespace Game.UI;

public partial class GameUI : MarginContainer
{
	[Signal] public delegate void PlaceTowerButtonPressedEventHandler();
	[Signal] public delegate void PlaceVillageButtonPressedEventHandler();

	[Export] private Button placeTowerButton;
	[Export] private Button placeVillageButton;

	public override void _Ready()
	{
		placeTowerButton.Pressed += OnPlaceTowerButtonPressed;
		placeVillageButton.Pressed += OnPlaceVillageButtonPressed;
	}

    private void OnPlaceVillageButtonPressed()
    {
		EmitSignal(SignalName.PlaceVillageButtonPressed);
    }


    private void OnPlaceTowerButtonPressed()
    {
		EmitSignal(SignalName.PlaceTowerButtonPressed);
    }
}
