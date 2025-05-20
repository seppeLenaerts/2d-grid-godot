using System;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Component;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
	public const int GRID_SIZE = 64;
	private const string IS_BUILDABLE = "buildable";
	private const string IS_WOOD = "wood";

	[Signal] public delegate void ResourceTilesUpdatedEventHandler(int count);

	[Export] private TileMapLayer highlightTileMapLayer;
	[Export] private TileMapLayer baseTerrainTileMapLayer;

	private List<TileMapLayer> tileMapLayers = [];
	private HashSet<Vector2I> validBuildableTiles = [], collectedResourceTiles = [];

	public override void _Ready()
	{
		GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
		tileMapLayers = GetAllTileMapLayers(baseTerrainTileMapLayer);
	}

	public bool IsTilePosValid(Vector2I cell, string condition)
	{
		foreach (var layer in tileMapLayers)
		{
			var tileData = layer.GetCellTileData(cell);
			if (tileData == null || collectedResourceTiles.Contains(cell))
				continue;
			return (bool)tileData.GetCustomData(condition);
		}

		return false;
	}

	public bool IsTilePosBuildable(Vector2I cell)
	{
		return validBuildableTiles.Contains(cell);
	}

	public void HighlightBuildableTiles()
	{
		foreach (var tilePos in validBuildableTiles)
		{
			highlightTileMapLayer.SetCell(
				tilePos, 0, Vector2I.Zero
			);
		}
	}

	public void HighlightInfluenceTiles(Vector2I cell, int radius)
	{
		ClearHighlightedTiles();
		HighlightBuildableTiles();

		if (validBuildableTiles.Contains(cell))
		{
			var validTiles = GetValidTilesInRadius(cell, radius, IS_BUILDABLE)
								.ToHashSet()
								.Except(validBuildableTiles)
								.Except(GetOccupiedTiles());
			HighlightCellsInList(validTiles);
		}
	}

	public void HighlightResourceTiles(Vector2I cell, int radius)
	{
		ClearHighlightedTiles();
		HighlightBuildableTiles();
		if (validBuildableTiles.Contains(cell))
			HighlightCellsInList(GetValidTilesInRadius(cell, radius, IS_WOOD));
	}

	public void ClearHighlightedTiles()
	{
		highlightTileMapLayer.Clear();
	}

	public Vector2I GetMouseGridCellPos()
	{
		var mousePos = highlightTileMapLayer.GetGlobalMousePosition();
		var gridPos = mousePos / GRID_SIZE;
		return (Vector2I)gridPos.Floor();
	}

	private List<TileMapLayer> GetAllTileMapLayers(TileMapLayer baseLayer)
	{
		var result = new List<TileMapLayer>();
		var children = baseLayer.GetChildren();
		children.Reverse();
		foreach (var child in children)
		{
			if (child is TileMapLayer childLayer)
			{
				result.AddRange(GetAllTileMapLayers(childLayer));
			}
		}
		result.Add(baseLayer);
		return result;
	}

	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		var cell = buildingComponent.GetGridCellPos();
		var radius = buildingComponent.buildingResource.BuildableRadius;
		validBuildableTiles.UnionWith(GetValidTilesInRadius(cell, radius, IS_BUILDABLE));
		validBuildableTiles.ExceptWith(GetOccupiedTiles());
	}

	private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
	{
		var cell = buildingComponent.GetGridCellPos();
		var radius = buildingComponent.buildingResource.ResourceRadius;
		var resourceTiles = GetValidTilesInRadius(cell, radius, IS_WOOD);
		resourceTiles = [.. resourceTiles.Except(collectedResourceTiles)];
		collectedResourceTiles.UnionWith(resourceTiles);
		EmitSignal(SignalName.ResourceTilesUpdated, resourceTiles.Count);
	}

	private IEnumerable<Vector2I> GetOccupiedTiles()
	{
		return GetTree()
				.GetNodesInGroup(nameof(BuildingComponent))
				.Cast<BuildingComponent>()
				.Select(b => b.GetGridCellPos());
	}

	private List<Vector2I> GetValidTilesInRadius(Vector2I cell, int radius, string condition)
	{
		var result = new List<Vector2I>();
		for (var x = cell.X - radius; x <= cell.X + radius; x++)
		{
			for (var y = cell.Y - radius; y <= cell.Y + radius; y++)
			{
				var currCell = new Vector2I(x, y);
				if (!IsTilePosValid(currCell, condition)) continue;
				result.Add(currCell);
			}
		}
		return result;
	}

	private void HighlightCellsInList(IEnumerable<Vector2I> cells)
	{
		foreach (var tilePos in cells)
		{
			highlightTileMapLayer.SetCell(
				tilePos, 0, new Vector2I(1, 0)
			);
		}
	}

	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		if (buildingComponent.buildingResource.ResourceRadius > 0)
		{
			UpdateCollectedResourceTiles(buildingComponent);
		}

		UpdateValidBuildableTiles(buildingComponent);
	}
}
