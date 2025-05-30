using Godot;

namespace Game.Resources.Buildings;

[GlobalClass]
public partial class BuildingResource : Resource
{
	[Export] public string DisplayName { get; private set; }
	[Export] public int ResourceCost { get; private set; }
	[Export] public int BuildableRadius { get; private set; }
	[Export] public int ResourceRadius { get; private set; }
	[Export] public PackedScene BuildingScene { get; private set; }
}
