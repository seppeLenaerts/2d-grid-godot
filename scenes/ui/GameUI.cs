using System;
using Game.Resources.Buildings;
using Godot;

namespace Game.UI;

public partial class GameUI : MarginContainer
{
	[Signal] public delegate void BuildingResourceSelectedEventHandler(BuildingResource buildingResource);

	[Export] private BuildingResource[] buildingResources;
	[Export] private HBoxContainer hBoxContainer;

	public override void _Ready()
	{
		CreateBuildingButtons();
	}

	private void CreateBuildingButtons()
	{
		foreach (var res in buildingResources)
		{
			var buildingButton = new Button();
			buildingButton.Text = $"Place {res.DisplayName}";
			hBoxContainer.AddChild(buildingButton);

			buildingButton.Pressed += () =>
			{
				EmitSignal(SignalName.BuildingResourceSelected, res);
			};
		}
	}
}
