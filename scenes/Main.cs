using Game.Manager;
using Game.Resources.Buildings;
using Game.UI;
using Godot;

namespace Game;


public partial class Main : Node
{
	[Export] private GridManager gridManager;
	[Export] private Node2D ySortRoot;
	[Export] private BuildingResource towerResource, villageResource;
	[ExportGroup("UI")]
	[Export] private GameUI gameUI;
	[Export] private Sprite2D cursorSprite;

	private Vector2I? hoveredGridCell;
	private BuildingResource buildingToPlaceResource;
	private bool multiBuild = false;

	public override void _Ready()
	{
		cursorSprite.Visible = false;
		gameUI.placeTowerButton.Pressed += OnTowerButtonPressed;
		gameUI.placeVillageButton.Pressed += OnVillageButtonPressed;
		gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
	}

	public override void _UnhandledInput(InputEvent evt)
	{
		if (hoveredGridCell.HasValue && evt.IsActionPressed("left_click") && gridManager.IsTilePosBuildable(hoveredGridCell.Value))
		{
			PlaceBuildingAtHoveredCellPos();
			if (!multiBuild)
			{
				cursorSprite.Visible = false;
			}
		}

		if (evt.IsActionPressed("multi_build"))
		{
			multiBuild = true;
		}

		if (evt.IsActionReleased("multi_build"))
		{
			multiBuild = false;
		}
	}


	public override void _Process(double delta)
	{
		var mouseGridPos = gridManager.GetMouseGridCellPos();
		cursorSprite.GlobalPosition = mouseGridPos * GridManager.GRID_SIZE;

		if (cursorSprite.Visible && (!hoveredGridCell.HasValue || hoveredGridCell.Value != mouseGridPos))
		{
			hoveredGridCell = mouseGridPos;
			if (buildingToPlaceResource.ResourceRadius > 0)
			{
				gridManager.HighlightResourceTiles(hoveredGridCell.Value, buildingToPlaceResource.ResourceRadius);
			}
			else
			{
				gridManager.HighlightInfluenceTiles(hoveredGridCell.Value, buildingToPlaceResource.BuildableRadius);
			}
		}
	}

	private void PlaceBuildingAtHoveredCellPos()
	{
		if (!hoveredGridCell.HasValue)
		{
			return;
		}

		var building = buildingToPlaceResource.BuildingScene.Instantiate<Node2D>();
		building.GlobalPosition = hoveredGridCell.Value * 64;
		ySortRoot.AddChild(building);

		hoveredGridCell = null;
		gridManager.ClearHighlightedTiles();
	}

	private void OnTowerButtonPressed()
	{
		buildingToPlaceResource = towerResource;
		cursorSprite.Visible = true;
	}

	private void OnVillageButtonPressed()
	{
		buildingToPlaceResource = villageResource;
		cursorSprite.Visible = true;
	}
	
	private void OnResourceTilesUpdated(int count)
	{
		GD.Print(count);
	}
}
