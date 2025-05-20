using Game.Autoload;
using Game.Manager;
using Game.Resources.Buildings;
using Godot;

namespace Game.Component;

public partial class BuildingComponent : Node2D
{
	[Export(PropertyHint.File, "*.tres")] public string buildingResourcePath;

	public BuildingResource buildingResource;

	public override void _Ready()
	{
		if (buildingResourcePath != null)
		{
			buildingResource = GD.Load<BuildingResource>(buildingResourcePath);
		}

		AddToGroup(nameof(BuildingComponent));
		Callable.From(() => GameEvents.EmitBuildingPlaced(this)).CallDeferred();
	}

	public Vector2I GetGridCellPos()
	{
		var gridPos = GlobalPosition / GridManager.GRID_SIZE;
		return (Vector2I)gridPos.Floor();
	}
}
