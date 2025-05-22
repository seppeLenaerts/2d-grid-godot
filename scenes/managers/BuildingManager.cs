using Game.Resources.Buildings;
using Game.UI;
using Godot;

namespace Game.Manager;

public partial class BuildingManager : Node
{
	[Export] private GridManager gridManager;
	[Export] private Node2D ySortRoot;
	[ExportGroup("UI")]
	[Export] private GameUI gameUI;
	[Export] private Sprite2D cursorSprite;

	private Vector2I? hoveredGridCell;
	private BuildingResource buildingToPlaceResource;
	private bool multiBuild;

	private int resourceCount = 4;
	private int usedResourceCount;

	private int AvailableResourceCount => resourceCount - usedResourceCount;

	public override void _Ready()
	{
		gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
		gameUI.BuildingResourceSelected += OnBuildingResourceSelected;
		cursorSprite.Visible = false;
	}

	public override void _UnhandledInput(InputEvent evt)
	{
		if (
			hoveredGridCell.HasValue &&
			evt.IsActionPressed("left_click") &&
			gridManager.IsTilePosBuildable(hoveredGridCell.Value) &&
			AvailableResourceCount >= buildingToPlaceResource.ResourceCost
		)
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
		usedResourceCount += buildingToPlaceResource.ResourceCost;
	}

	private void OnResourceTilesUpdated(int count)
	{
		resourceCount += count;
	}
	
	private void OnBuildingResourceSelected(BuildingResource res)
	{
		buildingToPlaceResource = res;
		cursorSprite.Visible = true;
	}
	
}
