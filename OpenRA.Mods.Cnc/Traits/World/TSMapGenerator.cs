#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OpenRA.Mods.Common.MapGenerator;
using OpenRA.Mods.Common.Terrain;
using OpenRA.Mods.Common.Traits;
using OpenRA.Support;
using OpenRA.Traits;
using static OpenRA.Mods.Common.Traits.ResourceLayerInfo;

namespace OpenRA.Mods.Cnc.Traits
{
	[TraitLocation(SystemActors.EditorWorld)]
	[Desc("A purpose-built Tiberian Sun map generator.")]
	public sealed class TSMapGeneratorInfo : TraitInfo, IEditorMapGeneratorInfo
	{
		[FieldLoader.Require]
		[Desc("Human-readable name this generator uses.")]
		[FluentReference]
		public readonly string Name = null;

		[FieldLoader.Require]
		[Desc("Internal id for this map generator.")]
		public readonly string Type = null;

		[FieldLoader.Require]
		[Desc("Tilesets that are compatible with this map generator.")]
		public readonly ImmutableArray<string> Tilesets = default;

		[FluentReference]
		[Desc("The title to use for generated maps.")]
		public readonly string MapTitle = "label-random-map";

		[Desc("The widget tree to open when the tool is selected.")]
		public readonly string PanelWidget = "MAP_GENERATOR_TOOL_PANEL";

		// This is purely of interest to the linter.
		[FieldLoader.LoadUsing(nameof(FluentReferencesLoader))]
		[FluentReference]
		public readonly ImmutableArray<string> FluentReferences = default;

		[FieldLoader.LoadUsing(nameof(SettingsLoader))]
		public readonly MiniYaml Settings;

		string IMapGeneratorInfo.Type => Type;
		string IMapGeneratorInfo.Name => Name;
		string IMapGeneratorInfo.MapTitle => MapTitle;
		ImmutableArray<string> IEditorMapGeneratorInfo.Tilesets => Tilesets;

		static MiniYaml SettingsLoader(MiniYaml my)
		{
			return my.NodeWithKey("Settings").Value;
		}

		static object FluentReferencesLoader(MiniYaml my)
		{
			return new MapGeneratorSettings(null, my.NodeWithKey("Settings").Value)
				.Options.SelectMany(o => o.GetFluentReferences()).ToImmutableArray();
		}

		const int FractionMax = Terraformer.FractionMax;
		const int EntityBonusMax = 1000000;

		[Flags]
		enum PavedRoadConnections
		{
			None = 0,
			NorthWest = 1,
			NorthEast = 2,
			SouthEast = 4,
			SouthWest = 8,
		}

		sealed class Parameters
		{
			[FieldLoader.Require]
			public readonly int Seed = default;
			[FieldLoader.Require]
			public readonly int Rotations = default;
			[FieldLoader.LoadUsing(nameof(MirrorLoader))]
			public readonly Symmetry.Mirror Mirror = default;
			[FieldLoader.Require]
			public readonly int Players = default;
			[FieldLoader.Require]
			public readonly int TerrainFeatureSize = default;
			[FieldLoader.Require]
			public readonly int RampFeatureSize = default;
			[FieldLoader.Require]
			public readonly int ForestFeatureSize = default;
			[FieldLoader.Require]
			public readonly int ResourceFeatureSize = default;
			[FieldLoader.Require]
			public readonly int CivilianBuildingsFeatureSize = default;
			[FieldLoader.Require]
			public readonly int Water = default;
			[FieldLoader.Require]
			public readonly int Mountains = default;
			[FieldLoader.Require]
			public readonly int Forests = default;
			[FieldLoader.Require]
			public readonly int ForestFloor = default;
			[FieldLoader.Require]
			public readonly int ForestCutout = default;
			public readonly int IceFields = 0;
			public readonly int IceFieldWaterBorder = 2;
			public readonly int RoughGroundPatches = 0;
			[FieldLoader.Require]
			public readonly int MaximumCutoutSpacing = default;
			[FieldLoader.Require]
			public readonly int TerrainSmoothing = default;
			[FieldLoader.Require]
			public readonly int SmoothingThreshold = default;
			public readonly int MinimumCoastStraight = 0;
			[FieldLoader.Require]
			public readonly int MinimumCliffStraight = default;
			[FieldLoader.Require]
			public readonly int MinimumLandSeaThickness = default;
			[FieldLoader.Require]
			public readonly int MinimumMountainThickness = default;
			[FieldLoader.Require]
			public readonly int MaximumAltitude = default;
			public readonly int BaseLandHeight = 0;
			public readonly int BaseLandHeightEdgeVariation = 0;
			[FieldLoader.Require]
			public readonly int RoughnessRadius = default;
			[FieldLoader.Require]
			public readonly int Roughness = default;
			[FieldLoader.Require]
			public readonly bool WaterCliffs = default;
			public readonly int WaterRoughness = 0;
			[FieldLoader.Require]
			public readonly int MinimumTerrainContourSpacing = default;
			[FieldLoader.Require]
			public readonly int MinimumBeachLength = default;
			public readonly int MinimumWaterCliffLength = 0;
			[FieldLoader.Require]
			public readonly int MinimumCliffLength = default;
			[FieldLoader.Require]
			public readonly int MinimumClearLength = default;
			public readonly int BeachSpreadWhenWaterCliffing = 0;
			[FieldLoader.Require]
			public readonly int RampSoften = default;
			[FieldLoader.Require]
			public readonly int ForestClumpiness = default;
			[FieldLoader.Require]
			public readonly bool DenyWalledAreas = default;
			[FieldLoader.Require]
			public readonly int EnforceSymmetry = default;
			public readonly bool Roads = false;
			public readonly int RoadSpacing = 6;
			public readonly int RoadShrink = 0;
			[FieldLoader.Require]
			public readonly bool CreateEntities = default;
			[FieldLoader.Require]
			public readonly int AreaEntityBonus = default;
			[FieldLoader.Require]
			public readonly int PlayerCountEntityBonus = default;
			[FieldLoader.Require]
			public readonly int CentralSpawnReservationFraction = default;
			[FieldLoader.Require]
			public readonly int ResourceSpawnReservation = default;
			[FieldLoader.Require]
			public readonly int SpawnRegionSize = default;
			[FieldLoader.Require]
			public readonly int SpawnBuildSize = default;
			[FieldLoader.Require]
			public readonly int MinimumSpawnRadius = default;
			[FieldLoader.Require]
			public readonly int SpawnResourceSpawns = default;
			[FieldLoader.Require]
			public readonly int SpawnReservation = default;
			[FieldLoader.Require]
			public readonly int SpawnResourceBias = default;
			[FieldLoader.Require]
			public readonly int ResourcesPerPlayer = default;
			[FieldLoader.Require]
			public readonly int OreUniformity = default;
			[FieldLoader.Require]
			public readonly int OreClumpiness = default;
			[FieldLoader.Require]
			public readonly int MaximumExpansionResourceSpawns = default;
			[FieldLoader.Require]
			public readonly int MaximumResourceSpawnsPerExpansion = default;
			[FieldLoader.Require]
			public readonly int MinimumExpansionSize = default;
			[FieldLoader.Require]
			public readonly int MaximumExpansionSize = default;
			[FieldLoader.Require]
			public readonly int ExpansionInner = default;
			[FieldLoader.Require]
			public readonly int ExpansionBorder = default;
			[FieldLoader.Require]
			public readonly int MinimumBuildings = default;
			[FieldLoader.Require]
			public readonly int MaximumBuildings = default;
			public readonly int MaximumBuildingsTotal = default;
			[FieldLoader.LoadUsing(nameof(BuildingWeightsLoader))]
			public readonly IReadOnlyDictionary<string, int> BuildingWeights = default;
			public readonly int MinimumVeinholes = 0;
			public readonly int MaximumVeinholes = 0;
			public readonly int DecorativeRocks = 0;
			public readonly int StrategicRocks = 0;
			public readonly int MinimumStrategicRockClusterActors = 2;
			public readonly int MaximumStrategicRockClusterActors = 5;
			public readonly int StrategicRockClusterInner = 1;
			public readonly int MinimumStrategicRockClusterRadius = 2;
			public readonly int MaximumStrategicRockClusterRadius = 5;
			public readonly int StrategicRockClusterBorder = 2;
			public readonly int RockReservation = 2;
			[FieldLoader.LoadUsing(nameof(RockWeightsLoader))]
			public readonly IReadOnlyDictionary<string, int> RockWeights = ImmutableDictionary<string, int>.Empty;
			[FieldLoader.Require]
			public readonly int CivilianBuildings = default;
			[FieldLoader.Require]
			public readonly int CivilianBuildingDensity = default;
			[FieldLoader.Require]
			public readonly int MinimumCivilianBuildingDensity = default;
			[FieldLoader.Require]
			public readonly int CivilianBuildingDensityRadius = default;

			[FieldLoader.Require]
			public readonly ushort LandTile = default;
			[FieldLoader.Require]
			public readonly ushort WaterTile = default;
			[FieldLoader.Ignore]
			public readonly IReadOnlyList<MultiBrush> SegmentedBrushes;
			[FieldLoader.Ignore]
			public readonly IReadOnlyList<MultiBrush> ForestObstacles;
			[FieldLoader.Ignore]
			public readonly IReadOnlyList<MultiBrush> UnplayableObstacles;
			[FieldLoader.Ignore]
			public readonly IReadOnlyList<MultiBrush> CivilianBuildingsObstacles;
			[FieldLoader.Ignore]
			public readonly IReadOnlyList<MultiBrush> RoughGroundPatchBrushes;
			[FieldLoader.Ignore]
			public readonly IReadOnlyList<MultiBrush> IceFieldBrushes;
			[FieldLoader.Require]
			public readonly ushort ForestFloorTile = default;
			public readonly ushort PavedRoadNorthWestSouthEastTile = 312;
			public readonly ushort PavedRoadNorthEastSouthWestTile = 313;
			public readonly ushort PavedRoadCrossingTile = 314;
			public readonly ushort PavedRoadTJunctionNorthEastTile = 317;
			public readonly ushort PavedRoadTJunctionSouthEastTile = 318;
			public readonly ushort PavedRoadTJunctionSouthWestTile = 319;
			public readonly ushort PavedRoadTJunctionNorthWestTile = 320;
			public readonly ushort PavedRoadClearNorthWestTile = 321;
			public readonly ushort PavedRoadClearSouthEastTile = 322;
			public readonly ushort PavedRoadDamagedNorthWestSouthEastTile = 323;
			public readonly ushort PavedRoadClearSouthWestTile = 324;
			public readonly ushort PavedRoadClearNorthEastTile = 325;
			public readonly ushort PavedRoadDamagedNorthEastSouthWestTile = 326;
			public readonly ushort PavedRoadEndSouthEastTile = 562;
			public readonly ushort PavedRoadEndNorthEastTile = 563;
			public readonly ushort PavedRoadEndNorthWestTile = 564;
			public readonly ushort PavedRoadEndSouthWestTile = 565;
			public readonly ushort PavedRoadCrosswalkNorthWestSouthEastTile = 624;
			public readonly ushort PavedRoadCrosswalkNorthEastSouthWestTile = 625;
			public readonly ushort PavedRoadSlopeNorthWestDownSouthEastUpTile = 672;
			public readonly ushort PavedRoadSlopeNorthEastDownSouthWestUpTile = 673;
			public readonly ushort PavedRoadSlopeNorthWestUpSouthEastDownTile = 674;
			public readonly ushort PavedRoadSlopeNorthEastUpSouthWestDownTile = 675;
			[FieldLoader.Ignore]
			public readonly IReadOnlyList<(ushort, int)> OtherGround;
			[FieldLoader.Ignore]
			public readonly IReadOnlyDictionary<ushort, IReadOnlyList<MultiBrush>> RepaintTiles;
			[FieldLoader.Ignore]
			public readonly IReadOnlyList<ushort> RampTiles;
			[FieldLoader.Ignore]
			public readonly LatTiler LatTiler;
			[FieldLoader.Ignore]
			public readonly LatTiler IceLatTiler = null;
			[FieldLoader.Require]
			public readonly bool UseIceLatTiler = false;

			[FieldLoader.Ignore]
			public readonly ResourceTypeInfo DefaultResource;
			[FieldLoader.Ignore]
			public readonly IReadOnlyDictionary<string, ResourceTypeInfo> ResourceSpawnSeeds;
			[FieldLoader.LoadUsing(nameof(ResourceSpawnWeightsLoader))]
			public readonly IReadOnlyDictionary<string, int> ResourceSpawnWeights = default;

			[FieldLoader.Ignore]
			public readonly IReadOnlySet<byte> ClearTerrain;
			[FieldLoader.Ignore]
			public readonly IReadOnlySet<byte> PlayableTerrain;
			[FieldLoader.Ignore]
			public readonly IReadOnlySet<byte> DominantTerrain;
			[FieldLoader.Ignore]
			public readonly IReadOnlySet<byte> ZoneableTerrain;
			[FieldLoader.Ignore]
			public readonly string ClearSegmentType;
			[FieldLoader.Ignore]
			public readonly string BeachSegmentType;
			[FieldLoader.Ignore]
			public readonly string WaterCliffSegmentType;
			[FieldLoader.Ignore]
			public readonly string CliffSegmentType;

			public Parameters(Map map, MiniYaml my)
			{
				FieldLoader.Load(this, my);

				var terrainInfo = (ITemplatedTerrainInfo)map.Rules.TerrainInfo;
				SegmentedBrushes = MultiBrush.LoadCollection(map, "Segmented");
				ForestObstacles = MultiBrush.LoadCollection(map, my.NodeWithKey("ForestObstacles").Value.Value);
				UnplayableObstacles = MultiBrush.LoadCollection(map, my.NodeWithKey("UnplayableObstacles").Value.Value);
				CivilianBuildingsObstacles = MultiBrush.LoadCollection(map, my.NodeWithKey("CivilianBuildingsObstacles").Value.Value);
				RoughGroundPatchBrushes = my.NodeWithKeyOrDefault("RoughGroundPatchBrushes") != null
					? MultiBrush.LoadCollection(map, my.NodeWithKey("RoughGroundPatchBrushes").Value.Value)
					: [];
				IceFieldBrushes = my.NodeWithKeyOrDefault("IceFieldBrushes") != null
					? MultiBrush.LoadCollection(map, my.NodeWithKey("IceFieldBrushes").Value.Value)
					: [];
				OtherGround = my.NodeWithKeyOrDefault("OtherGround")?.Value.Nodes.Select(
					n =>
					{
						if (!Exts.TryParseUshortInvariant(n.Key, out var tile))
							throw new YamlException($"OtherGround {n.Key} is not a ushort");

						if (!Exts.TryParseInt32Invariant(n.Value.Value, out var fraction))
							throw new YamlException($"OtherGround {n.Key} has invalid fraction (should be 0 to {FractionMax})");

						return (tile, fraction);
					}).ToList();
				RepaintTiles = my.NodeWithKeyOrDefault("RepaintTiles")?.Value.ToDictionary(
					k =>
					{
						if (Exts.TryParseUshortInvariant(k, out var tile))
							return tile;
						else
							throw new YamlException($"RepaintTile {k} is not a ushort");
					},
					v => MultiBrush.LoadCollection(map, v.Value) as IReadOnlyList<MultiBrush>);
				RepaintTiles ??= ImmutableDictionary<ushort, IReadOnlyList<MultiBrush>>.Empty;

				RampTiles = FieldLoader.GetValue<List<ushort>>(
					nameof(RampTiles),
					my.NodeWithKey(nameof(RampTiles)).Value.Value);
				LatTiler = new LatTiler(my.NodeWithKey("LatTiler").Value, terrainInfo);
				if (my.NodeWithKeyOrDefault("IceLatTiler") != null)
					IceLatTiler = new LatTiler(my.NodeWithKeyOrDefault("IceLatTiler").Value, terrainInfo);

				var resourceTypes = map.Rules.Actors[SystemActors.World].TraitInfoOrDefault<ResourceLayerInfo>().ResourceTypes;
				if (!resourceTypes.TryGetValue(my.NodeWithKey("DefaultResource").Value.Value, out DefaultResource))
					throw new YamlException("DefaultResource is not valid");
				var playerResourcesInfo = map.Rules.Actors[SystemActors.Player].TraitInfoOrDefault<PlayerResourcesInfo>();
				try
				{
					ResourceSpawnSeeds = my.NodeWithKey("ResourceSpawnSeeds").Value
						.ToDictionary(subMy => subMy.Value)
						.ToDictionary(kv => kv.Key, kv => resourceTypes[kv.Value]);
				}
				catch (KeyNotFoundException e)
				{
					throw new YamlException("Bad ResourceSpawnSeeds resource: " + e);
				}

				switch (Rotations)
				{
					case 1:
					case 2:
					case 4:
						break;
					default:
						EnforceSymmetry = 0;
						break;
				}

				IReadOnlySet<byte> ParseTerrainIndexes(string key)
				{
					return my.NodeWithKey(key).Value.Value
						.Split(',', StringSplitOptions.RemoveEmptyEntries)
						.Select(terrainInfo.GetTerrainIndex)
						.ToImmutableHashSet();
				}

				ClearTerrain = ParseTerrainIndexes("ClearTerrain");
				PlayableTerrain = ParseTerrainIndexes("PlayableTerrain");
				DominantTerrain = ParseTerrainIndexes("DominantTerrain");
				ZoneableTerrain = ParseTerrainIndexes("ZoneableTerrain");

				ClearSegmentType = my.NodeWithKey("ClearSegmentTypes").Value.Value;
				BeachSegmentType = my.NodeWithKey("BeachSegmentTypes").Value.Value;
				if (WaterCliffs)
					WaterCliffSegmentType = my.NodeWithKey("WaterCliffSegmentTypes").Value.Value;

				CliffSegmentType = my.NodeWithKey("CliffSegmentTypes").Value.Value;
			}

			static object MirrorLoader(MiniYaml my)
			{
				if (Symmetry.TryParseMirror(my.NodeWithKey("Mirror").Value.Value, out var mirror))
					return mirror;
				else
					throw new YamlException($"Invalid Mirror value `{my.NodeWithKey("Mirror").Value.Value}`");
			}

			static IReadOnlyDictionary<string, int> BuildingWeightsLoader(MiniYaml my)
			{
				return my.NodeWithKey("BuildingWeights").Value.ToDictionary(subMy =>
					{
						if (Exts.TryParseInt32Invariant(subMy.Value, out var f))
							return f;
						else
							throw new YamlException($"Invalid building weight `{subMy.Value}`");
					});
			}

			static IReadOnlyDictionary<string, int> ResourceSpawnWeightsLoader(MiniYaml my)
			{
				return my.NodeWithKey("ResourceSpawnWeights").Value.ToDictionary(subMy =>
					{
						if (Exts.TryParseInt32Invariant(subMy.Value, out var f))
							return f;
						else
							throw new YamlException($"Invalid resource spawn weight `{subMy.Value}`");
					});
			}

			static IReadOnlyDictionary<string, int> RockWeightsLoader(MiniYaml my)
			{
				return my.NodeWithKey("RockWeights").Value.ToDictionary(subMy =>
					{
						if (Exts.TryParseInt32Invariant(subMy.Value, out var f))
							return f;
						else
							throw new YamlException($"Invalid rock weight `{subMy.Value}`");
					});
			}
		}

		public IMapGeneratorSettings GetSettings()
		{
			return new MapGeneratorSettings(this, Settings);
		}

		static void SetBaseLandHeight(RampTiler.HeightMap heightMap, byte baseLandHeight, int edgeFlattening, Matrix<int> edgeDelay)
		{
			if (baseLandHeight == 0)
				return;

			var distanceToUntileable = new Matrix<int>(heightMap.Target.Size).Fill(int.MaxValue);
			var queue = new Queue<int2>();
			var adjacentCornerOffsets = new[] { new int2(0, -1), new int2(1, 0), new int2(0, 1), new int2(-1, 0) };
			for (var y = 0; y < distanceToUntileable.Size.Y; y++)
			{
				for (var x = 0; x < distanceToUntileable.Size.X; x++)
				{
					if (heightMap.Adjustable[x, y])
						continue;

					distanceToUntileable[x, y] = 0;
					queue.Enqueue(new int2(x, y));
				}
			}

			while (queue.Count > 0)
			{
				var xy = queue.Dequeue();
				var distance = distanceToUntileable[xy] + 1;
				foreach (var offset in adjacentCornerOffsets)
				{
					var next = xy + offset;
					if (!distanceToUntileable.ContainsXY(next) || !heightMap.Adjustable[next])
						continue;

					if (distance >= distanceToUntileable[next])
						continue;

					distanceToUntileable[next] = distance;
					queue.Enqueue(next);
				}
			}

			void RaiseCorner(int2 xy)
			{
				if (!heightMap.Target.ContainsXY(xy) || !heightMap.Adjustable[xy])
					return;

				var delayedDistance = distanceToUntileable[xy] - edgeFlattening - (edgeDelay?[xy] ?? 0);
				var height = (byte)Math.Clamp(delayedDistance, byte.MinValue, baseLandHeight);
				heightMap.Target[xy] = Math.Max(heightMap.Target[xy], height);
				heightMap.LowerBound[xy] = Math.Max(heightMap.LowerBound[xy], height);
			}

			foreach (var cpos in heightMap.Tileable.CellRegion)
			{
				if (!heightMap.Tileable[cpos])
					continue;

				var xy = heightMap.CPosToXy(cpos);
				RaiseCorner(xy);
				RaiseCorner(xy + new int2(1, 0));
				RaiseCorner(xy + new int2(0, 1));
				RaiseCorner(xy + new int2(1, 1));
			}
		}

		public Map Generate(ModData modData, MapGenerationArgs args)
		{
			var terrainInfo = modData.DefaultTerrainInfo[args.Tileset];
			var size = args.Size;

			var map = new Map(modData, terrainInfo, size);
			var actorPlans = new List<ActorPlan>();

			var param = new Parameters(map, args.Settings);

			var terraformer = new Terraformer(args, map, modData, actorPlans, param.Mirror, param.Rotations);

			CellLayer<MultiBrush.Replaceability> PlayableToReplaceable()
			{
				var playable = terraformer.CheckSpace(param.PlayableTerrain, true);
				var basicLand = terraformer.CheckSpace(param.LandTile);
				var replace = new CellLayer<MultiBrush.Replaceability>(map);
				foreach (var mpos in map.AllCells.MapCoords)
					if (playable[mpos])
					{
						if (basicLand[mpos])
							replace[mpos] = MultiBrush.Replaceability.Any;
						else
							replace[mpos] = MultiBrush.Replaceability.Actor;
					}
					else
					{
						replace[mpos] = MultiBrush.Replaceability.None;
					}

				return replace;
			}

			var cellBounds = CellLayerUtils.CellBounds(map);

			// Use `random` to derive separate independent random number generators.
			//
			// This prevents changes in one part of the algorithm from affecting randomness in
			// other parts.
			//
			// In order to maintain stability, additions should be appended only. Disused
			// derivatives may be deleted but should be replaced with their unused call to
			// random.Next(). All derived RNGs should be created unconditionally.
			var random = new MersenneTwister(param.Seed);

			var pickAnyRandom = new MersenneTwister(random.Next());
			var elevationRandom = new MersenneTwister(random.Next());
			var coastTilingRandom = new MersenneTwister(random.Next());
			var cliffTilingRandom = new MersenneTwister(random.Next());
			var rampTilingRandom = new MersenneTwister(random.Next());
			var forestRandom = new MersenneTwister(random.Next());
			var forestTilingRandom = new MersenneTwister(random.Next());
			var symmetryTilingRandom = new MersenneTwister(random.Next());
			var debrisTilingRandom = new MersenneTwister(random.Next());
			var resourceRandom = new MersenneTwister(random.Next());
			var playerRandom = new MersenneTwister(random.Next());
			var expansionRandom = new MersenneTwister(random.Next());
			var buildingRandom = new MersenneTwister(random.Next());
			var topologyRandom = new MersenneTwister(random.Next());
			var repaintRandom = new MersenneTwister(random.Next());
			var decorationRandom = new MersenneTwister(random.Next());
			var decorationTilingRandom = new MersenneTwister(random.Next());
			var heightMapNoiseRandom = new MersenneTwister(random.Next());
			var groundTypeNoiseRandom = new MersenneTwister(random.Next());
			var baseLandHeightRandom = new MersenneTwister(random.Next());
			var rockRandom = new MersenneTwister(random.Next());

			terraformer.InitMap();

			var rampTiler = new RampTiler(map, param.RampTiles);

			var clearZone = new Terraformer.PathPartitionZone()
			{
				ShouldTile = false,
				SegmentType = param.ClearSegmentType,
				MinimumLength = param.MinimumClearLength,
			};
			var beachZone = new Terraformer.PathPartitionZone()
			{
				SegmentType = param.BeachSegmentType,
				MinimumLength = param.MinimumBeachLength,
				MaximumDeviation = param.MinimumLandSeaThickness - 1,
			};
			var cliffZone = new Terraformer.PathPartitionZone()
			{
				SegmentType = param.CliffSegmentType,
				MinimumLength = param.MinimumCliffLength,
				MaximumDeviation = param.MinimumMountainThickness - 1,
			};

			foreach (var mpos in map.AllCells.MapCoords)
				map.Tiles[mpos] = terraformer.PickTile(pickAnyRandom, param.LandTile);

			var elevation = terraformer.ElevationNoiseMatrix(
				elevationRandom,
				param.TerrainFeatureSize,
				param.TerrainSmoothing);
			var roughnessMatrix = MatrixUtils.GridVariance(
				elevation,
				param.RoughnessRadius);

			var landPlan = terraformer.SliceElevation(elevation, null, FractionMax - param.Water);
			landPlan = MatrixUtils.BooleanBlotch(
				landPlan,
				param.TerrainSmoothing,
				param.SmoothingThreshold, /*smoothingThresholdOutOf=*/FractionMax,
				param.MinimumLandSeaThickness,
				/*bias=*/param.Water <= FractionMax / 2);
			var elevationPlan = landPlan;

			var elevationCalibration =
				Enumerable.Zip(
					landPlan.Data,
					elevation.Data,
					(p, e) => p ? e : int.MaxValue)
				.Min();
			elevation = elevation.Map(v => v - elevationCalibration);

			var heightMap = new RampTiler.HeightMap(map);

			var coast = MatrixUtils.BordersToPoints(landPlan);
			List<TilingPath> coastPaths;
			if (param.WaterCliffs)
			{
				var waterCliffZone = new Terraformer.PathPartitionZone()
				{
					SegmentType = param.WaterCliffSegmentType,
					MinimumLength = param.MinimumWaterCliffLength,
					MaximumDeviation = param.MinimumLandSeaThickness - 1,
				};
				var waterCliffMask = MatrixUtils.CalibratedBooleanThreshold(
					roughnessMatrix,
					param.WaterRoughness, FractionMax);
				var partitionMask = waterCliffMask.Map(masked => masked ? waterCliffZone : beachZone);
				coastPaths = terraformer.PartitionPaths(
					coast,
					[beachZone, waterCliffZone],
					partitionMask,
					param.SegmentedBrushes,
					param.MinimumCoastStraight);

				foreach (var coastPath in coastPaths)
					coastPath
						.OptimizeLoop()
						.ExtendEdge(4);
			}
			else
			{
				coastPaths = CellLayerUtils.FromMatrixPoints(coast, map.Tiles)
					.Select(beach =>
						TilingPath.QuickCreate(
								map,
								param.SegmentedBrushes,
								beach,
								param.MinimumLandSeaThickness - 1,
								param.BeachSegmentType,
								param.BeachSegmentType)
									.ExtendEdge(4))
					.ToList();
			}

			var landCoastWater = terraformer.PaintLoopsAndFill(
				coastTilingRandom,
				coastPaths,
				landPlan[0] ? Terraformer.Side.In : Terraformer.Side.Out,
				[new MultiBrush().WithTemplate(map, param.WaterTile, CVec.Zero)],
				null,
				null,
				0)
					?? throw new MapGenerationException("Could not fit tiles for coast");

			var cliffHeight = MultiBrush.MaxHeightOfSegmentType(
				param.CliffSegmentType,
				param.SegmentedBrushes);

			if (param.WaterCliffs)
			{
				var waterCliffHeight = MultiBrush.MaxHeightOfSegmentType(
					param.WaterCliffSegmentType,
					param.SegmentedBrushes);

				elevationPlan = terraformer.SliceElevation(
					elevation,
					elevationPlan,
					FractionMax,
					param.MinimumTerrainContourSpacing);

				heightMap.SetCellHeights(
					waterCliffHeight,
					CellLayerUtils.Map(landCoastWater, v => v == Terraformer.Side.In));
				heightMap.MarkUntileable(
					CellLayerUtils.Map(map.Height, v => v != 0));
				heightMap.SeedHeights(
					heightMap.Target.Enumerate()
						.Where(v => v.Value == 0)
						.Select(v => (v.Xy, param.BeachSpreadWhenWaterCliffing, (byte)0)));
			}

			heightMap.MarkUntileable(
				CellLayerUtils.Map(landCoastWater, v => v != Terraformer.Side.In));

			Matrix<int> baseLandHeightEdgeDelay = null;
			if (param.BaseLandHeightEdgeVariation > 0)
			{
				baseLandHeightEdgeDelay = NoiseUtils.SymmetricFractalNoise(
					baseLandHeightRandom,
					heightMap.Target.Size,
					terraformer.Rotations,
					terraformer.WMirror.ForCPos(),
					Math.Max(4096, param.RampFeatureSize / 2),
					NoiseUtils.PinkAmplitude);
				baseLandHeightEdgeDelay = MatrixUtils.BinomialBlur(baseLandHeightEdgeDelay, 1);
				baseLandHeightEdgeDelay = MatrixUtils
					.NormalizeRangeInPlace(baseLandHeightEdgeDelay, param.BaseLandHeightEdgeVariation)
					.Map(v => Math.Max(0, v));
			}

			SetBaseLandHeight(
				heightMap,
				(byte)Math.Clamp(param.BaseLandHeight, 0, map.Grid.MaximumTerrainHeight),
				param.WaterCliffs ? 0 : 1,
				baseLandHeightEdgeDelay);

			if (param.Mountains > 0)
			{
				var cliffMask = MatrixUtils.CalibratedBooleanThreshold(
					roughnessMatrix,
					param.Roughness, FractionMax);

				for (var altitude = 0; altitude < param.MaximumAltitude; altitude++)
				{
					elevationPlan = terraformer.SliceElevation(
						elevation,
						elevationPlan,
						param.Mountains,
						param.MinimumTerrainContourSpacing);
					elevationPlan = MatrixUtils.BooleanBlotch(
						elevationPlan,
						param.TerrainSmoothing,
						param.SmoothingThreshold, /*smoothingThresholdOutOf=*/FractionMax,
						param.MinimumMountainThickness,
						/*bias=*/false);

					var planCellLayer = new CellLayer<bool>(map);
					CellLayerUtils.FromMatrix(planCellLayer, elevationPlan);

					var contours = MatrixUtils.BordersToPoints(elevationPlan);
					var partitionMask = cliffMask.Map(masked => masked ? cliffZone : clearZone);

					var shortContours = new List<int2[]>();
					var tallContours = new List<int2[]>();

					foreach (var contour in contours)
					{
						var tilingPaths = terraformer.PartitionPath(
							contour,
							[cliffZone, clearZone],
							partitionMask,
							param.SegmentedBrushes,
							param.MinimumCliffStraight);

						if (tilingPaths.Count > 0)
							tallContours.Add(contour);
						else
							shortContours.Add(contour);

						var baseHeight = contour.Max(xy => heightMap.Target[xy]);

						foreach (var tilingPath in tilingPaths)
						{
							var brush = tilingPath
								.OptimizeLoop()
								.ExtendEdge(4)
								.SetAutoEndDeviation()
								.Tile(cliffTilingRandom)
									?? throw new MapGenerationException("Could not fit tiles for sand-sand cliffs");

							terraformer.PaintTiling(pickAnyRandom, brush, baseHeight);

							heightMap.MarkUntileable(brush.Shape.Select(cvec => CPos.Zero + cvec));
						}
					}

					var shortMask = new CellLayer<bool>(map);
					var tallMask = new CellLayer<bool>(map);

					var shortChirality = MatrixUtils.PointsChirality(cellBounds.Size.ToInt2(), shortContours);
					if (shortChirality != null)
					{
						CellLayerUtils.FromMatrix(
							shortMask,
							shortChirality.Map(v => v > 0));
					}

					var tallChirality = MatrixUtils.PointsChirality(cellBounds.Size.ToInt2(), tallContours);
					if (tallChirality != null)
					{
						CellLayerUtils.FromMatrix(
							tallMask,
							tallChirality.Map(v => v > 0));
					}

					heightMap.AdjustCellHeights(1, shortMask);
					heightMap.AdjustCellHeights(cliffHeight, tallMask);
				}
			}

			{
				rampTiler.PullHeightMap(heightMap);

				var noise = NoiseUtils.SymmetricFractalNoise(
					heightMapNoiseRandom,
					heightMap.Target.Size,
					terraformer.Rotations,
					terraformer.WMirror.ForCPos(),
					param.RampFeatureSize,
					NoiseUtils.PinkAmplitude);
				noise = MatrixUtils.BinomialBlur(noise, 1);
				noise = MatrixUtils.NormalizeRangeInPlace(noise, 3);
				for (var i = 0; i < noise.Data.Length; i++)
					heightMap.Target[i] = (byte)Math.Clamp(noise[i] + heightMap.Target[i], byte.MinValue, byte.MaxValue);

				heightMap.Soften(param.RampSoften);
				if (!heightMap.Constrain(RampTiler.AdjustmentMode.LowerMiddle))
					throw new MapGenerationException("created unfixable heightmap");

				var brush = rampTiler.TileHeightMap(heightMap, rampTilingRandom)
					?? throw new MapGenerationException("created invalid heightmap");
				terraformer.PaintTiling(rampTilingRandom, brush, 0);
			}

			CellLayer<bool> forestPlan = null;
			if (param.Forests > 0)
			{
				var space = terraformer.CheckSpace(param.ClearTerrain);
				var passages = terraformer.PlanPassages(
					topologyRandom,
					terraformer.ImproveSymmetry(space, true, (a, b) => a && b),
					param.ForestCutout,
					param.MaximumCutoutSpacing);
				forestPlan = terraformer.BooleanNoise(
					forestRandom,
					param.ForestFeatureSize,
					param.Forests,
					param.ForestClumpiness);
				var replace = PlayableToReplaceable();
				foreach (var mpos in map.AllCells.MapCoords)
					if (!forestPlan[mpos] || !space[mpos] || passages[mpos])
						replace[mpos] = MultiBrush.Replaceability.None;
				terraformer.PaintArea(forestTilingRandom, replace, param.ForestObstacles);
			}

			if (param.EnforceSymmetry != 0)
			{
				var asymmetries = terraformer.FindAsymmetries(param.DominantTerrain, true, param.EnforceSymmetry == 2);
				terraformer.PaintActors(symmetryTilingRandom, asymmetries, param.ForestObstacles);
			}

			CellLayer<bool> playable;
			{
				playable = terraformer.ChoosePlayableRegion(
					terraformer.CheckSpace(param.PlayableTerrain, true, false, true),
					null)
						?? throw new MapGenerationException("could not find a playable region");

				var minimumPlayableSpace = (int)(param.Players * Math.PI * param.SpawnBuildSize * param.SpawnBuildSize);
				if (playable.Count(p => p) < minimumPlayableSpace)
					throw new MapGenerationException("playable space is too small");

				if (param.DenyWalledAreas)
				{
					var replace = PlayableToReplaceable();
					foreach (var mpos in map.AllCells.MapCoords)
						if (playable[mpos] || !map.Contains(mpos))
							replace[mpos] = MultiBrush.Replaceability.None;

					terraformer.PaintArea(debrisTilingRandom, replace, param.UnplayableObstacles);
				}
			}

			var zoneable = terraformer.GetZoneable(param.ZoneableTerrain, playable);

			if (param.Roads)
			{
				var roadCells = PlanPavedRoadGrid();
				var roadConnections = ConnectPavedRoadGrid(roadCells);
				PaintPavedRoads(roadConnections);
			}

			if (param.CreateEntities)
			{
				var zoneableArea = zoneable.Count(v => v);
				var symmetryCount = Symmetry.RotateAndMirrorProjectionCount(param.Rotations, param.Mirror);
				var entityMultiplier =
					(long)zoneableArea * param.AreaEntityBonus +
					(long)param.Players * param.PlayerCountEntityBonus;
				var perSymmetryEntityMultiplier = entityMultiplier / symmetryCount;

				// Spawn generation
				var symmetryPlayers = param.Players / symmetryCount;
				for (var iteration = 0; iteration < symmetryPlayers; iteration++)
				{
					var chosenCPos = terraformer.ChooseSpawnInZoneable(
						playerRandom,
						zoneable,
						param.CentralSpawnReservationFraction,
						param.MinimumSpawnRadius,
						param.SpawnRegionSize,
						param.SpawnReservation)
							?? throw new MapGenerationException("Not enough room for player spawns");

					var spawn = new ActorPlan(map, "mpspawn")
					{
						Location = chosenCPos,
					};

					var resourceSpawnPreferences = terraformer.TargetWalkingDistance(
						terraformer.CheckSpace(param.PlayableTerrain, true),
						terraformer.ErodeZones(zoneable, 1),
						[chosenCPos],
						new WDist((param.SpawnBuildSize + param.SpawnRegionSize * 2) * 512),
						new WDist(param.SpawnRegionSize * 1024));
					terraformer.AddDistributedActors(
						playerRandom,
						zoneable,
						resourceSpawnPreferences,
						param.ResourceSpawnWeights,
						param.SpawnResourceSpawns,
						false,
						new WDist(param.ResourceSpawnReservation * 1024));

					terraformer.ProjectPlaceDezoneActor(spawn, zoneable, new WDist(param.SpawnReservation * 1024));
				}

				// Expansions
				{
					var resourceSpawnsRemaining = (int)(param.MaximumExpansionResourceSpawns * perSymmetryEntityMultiplier / EntityBonusMax);
					while (resourceSpawnsRemaining > 0)
					{
						var added = terraformer.AddActorCluster(
							expansionRandom,
							zoneable,
							param.ResourceSpawnWeights,
							Math.Min(resourceSpawnsRemaining, expansionRandom.Next(param.MaximumResourceSpawnsPerExpansion) + 1),
							param.ExpansionInner,
							param.MinimumExpansionSize,
							param.MaximumExpansionSize,
							param.ExpansionBorder,
							true,
							new WDist(param.ResourceSpawnReservation * 1024));
						resourceSpawnsRemaining -= added;
						if (added == 0)
							break;
					}
				}

				// Neutral buildings
				{
					var (buildingTypes, buildingWeights) = Terraformer.SplitDictionary(param.BuildingWeights);
					var targetBuildingCount =
						(param.MaximumBuildings != 0)
							? buildingRandom.Next(
								(int)(param.MinimumBuildings * perSymmetryEntityMultiplier / EntityBonusMax),
								(int)(param.MaximumBuildings * perSymmetryEntityMultiplier / EntityBonusMax) + 1)
							: 0;
					if (param.MaximumBuildingsTotal > 0)
					{
						var maximumBuildingGroups = Math.Max(1, param.MaximumBuildingsTotal / symmetryCount);
						targetBuildingCount = Math.Min(targetBuildingCount, maximumBuildingGroups);
					}

					for (var i = 0; i < targetBuildingCount; i++)
						terraformer.AddActor(
							buildingRandom,
							zoneable,
							buildingTypes[buildingRandom.PickWeighted(buildingWeights)]);
				}

				// Veinholes
				{
					var targetVeinholeCount =
						(param.MaximumVeinholes != 0)
							? buildingRandom.Next(
								(int)(param.MinimumVeinholes * perSymmetryEntityMultiplier / EntityBonusMax),
								(int)(param.MaximumVeinholes * perSymmetryEntityMultiplier / EntityBonusMax) + 1)
							: 0;
					for (var i = 0; i < targetVeinholeCount; i++)
						terraformer.AddActor(
							buildingRandom,
							zoneable,
							"veinhole");
				}

				// Rocks
				if (param.RockWeights.Count > 0)
				{
					var rockDezoneRadius = new WDist(param.RockReservation * 1024);
					var targetDecorativeRockCount = (int)(param.DecorativeRocks * perSymmetryEntityMultiplier / EntityBonusMax);
					if (targetDecorativeRockCount > 0)
					{
						var rockDistribution = CellLayerUtils.Create(map, (MPos mpos) => zoneable[mpos] ? 1 : 0);
						terraformer.AddDistributedActors(
							rockRandom,
							zoneable,
							rockDistribution,
							param.RockWeights,
							targetDecorativeRockCount,
							true,
							rockDezoneRadius);
					}

					var strategicRocksRemaining = (int)(param.StrategicRocks * perSymmetryEntityMultiplier / EntityBonusMax);
					while (strategicRocksRemaining > 0)
					{
						var clusterSize = Math.Min(
							strategicRocksRemaining,
							rockRandom.Next(
								param.MinimumStrategicRockClusterActors,
								param.MaximumStrategicRockClusterActors + 1));
						var added = terraformer.AddActorCluster(
							rockRandom,
							zoneable,
							param.RockWeights,
							clusterSize,
							param.StrategicRockClusterInner,
							param.MinimumStrategicRockClusterRadius,
							param.MaximumStrategicRockClusterRadius,
							param.StrategicRockClusterBorder,
							true,
							rockDezoneRadius);
						strategicRocksRemaining -= added;
						if (added == 0)
							break;
					}
				}

				// Grow resources
				var targetResourceValue = param.ResourcesPerPlayer * entityMultiplier / EntityBonusMax;
				if (targetResourceValue > 0)
				{
					var resourcePattern = terraformer.ResourceNoise(
						resourceRandom,
						param.ResourceFeatureSize,
						param.OreClumpiness,
						param.OreUniformity * 1024 / FractionMax);

					var resourceBiases = new List<Terraformer.ResourceBias>();
					var wSpawnBuildSizeSq = (long)param.SpawnBuildSize * param.SpawnBuildSize * 1024 * 1024;

					// Bias towards resource spawns
					foreach (var (actorType, resourceType) in param.ResourceSpawnSeeds.OrderBy(kv => kv.Key))
					{
						resourceBiases.AddRange(
							terraformer.ActorsOfType(actorType)
								.Select(a => new Terraformer.ResourceBias(a)
								{
									BiasRadius = new WDist(16 * 1024),
									Bias = (value, rSq) => value + (int)(1024 * 1024 / (1024 + Exts.ISqrt(rSq))),
									ResourceType = resourceType,
								}));
					}

					// Give veinholes even more bias. (Note: they don't consume resource quota.)
					resourceBiases.AddRange(
						terraformer.ActorsOfType("veinhole")
							.Select(a => new Terraformer.ResourceBias(a)
							{
								BiasRadius = new WDist(16 * 1024),
								Bias = (value, rSq) => value + (int)(512 * 1024 / (1024 + Exts.ISqrt(rSq))),
							}));

					// Bias towards player spawns, but also reserve an area for base building.
					resourceBiases.AddRange(
						terraformer.ActorsOfType("mpspawn")
							.Select(a => new Terraformer.ResourceBias(a)
							{
								ExclusionRadius = new WDist(param.SpawnBuildSize * 1024),
								BiasRadius = new WDist(param.SpawnRegionSize * 2 * 1024),
								Bias = (value, rSq) => value + (int)(value * param.SpawnResourceBias * wSpawnBuildSizeSq / Math.Max(rSq, 1024 * 1024) / FractionMax),
							}));

					var resourceMask = CellLayerUtils.Clone(playable);
					terraformer.ZoneFromActors(resourceMask, false);
					terraformer.ZoneFromNonCardinalRamps(resourceMask, false);

					var (plan, typePlan) = terraformer.PlanResources(
						resourcePattern,
						resourceMask,
						param.DefaultResource,
						resourceBiases);
					terraformer.GrowResources(
						plan,
						typePlan,
						targetResourceValue,
						Terraformer.ResourceDensityMode.BakedAdjacency);
					terraformer.ZoneFromResources(zoneable, false);

					// Veins should be max density.
					var veinType = map.Rules.Actors[SystemActors.World].TraitInfoOrDefault<ResourceLayerInfo>().ResourceTypes["Veins"];
					var veinResourceTile = new ResourceTile(veinType.ResourceIndex, veinType.MaxDensity);
					foreach (var mpos in map.Resources.CellRegion.MapCoords)
						if (map.Resources[mpos].Type == veinType.ResourceIndex)
							map.Resources[mpos] = veinResourceTile;
				}

				// CivilianBuildings
				if (param.CivilianBuildings > 0)
				{
					var decorationNoise = terraformer.DecorationPattern(
						decorationRandom,
						terraformer.CheckSpace(param.PlayableTerrain, true),
						CellLayerUtils.Intersect([zoneable, terraformer.CheckSpace(param.LandTile)]),
						param.CivilianBuildings,
						param.CivilianBuildingsFeatureSize,
						param.CivilianBuildingDensity,
						param.MinimumCivilianBuildingDensity,
						param.CivilianBuildingDensityRadius);
					terraformer.PaintActors(
						decorationTilingRandom,
						decorationNoise,
						param.CivilianBuildingsObstacles,
						alwaysPreferLargerBrushes: true);
				}
			}

			HashSet<CPos> PlanPavedRoadGrid()
			{
				const int MinimumRoadSegmentLength = 10;
				var roadable = terraformer.CheckSpace(param.ClearTerrain, true, false, false, true);
				var roadCells = new HashSet<CPos>();
				var margin = Math.Max(0, param.RoadShrink);
				var minX = cellBounds.TopLeft.X + margin;
				var minY = cellBounds.TopLeft.Y + margin;
				var maxX = cellBounds.BottomRight.X - margin;
				var maxY = cellBounds.BottomRight.Y - margin;
				var spacing = Math.Max(8, param.RoadSpacing);
				if (maxX < minX || maxY < minY)
					return roadCells;

				var xStart = minX + spacing / 2;
				var yStart = minY + spacing / 2;
				for (var x = xStart; x <= maxX; x += spacing)
					AddPavedRoadGridRun(
						roadCells,
						Enumerable.Range(minY, maxY - minY + 1).Select(y => new CPos(x, y)),
						roadable,
						MinimumRoadSegmentLength);

				for (var y = yStart; y <= maxY; y += spacing)
					AddPavedRoadGridRun(
						roadCells,
						Enumerable.Range(minX, maxX - minX + 1).Select(x => new CPos(x, y)),
						roadable,
						MinimumRoadSegmentLength);

				return roadCells;
			}

			void AddPavedRoadGridRun(HashSet<CPos> roadCells, IEnumerable<CPos> line, CellLayer<bool> roadable, int minimumLength)
			{
				var run = new List<CPos>();

				foreach (var cpos in line)
				{
					if (IsPavedRoadGridCell(cpos, roadable))
					{
						run.Add(cpos);
						continue;
					}

					FlushPavedRoadGridRun(roadCells, run, minimumLength);
				}

				FlushPavedRoadGridRun(roadCells, run, minimumLength);
			}

			bool IsPavedRoadGridCell(CPos cpos, CellLayer<bool> roadable) =>
				map.Contains(cpos) && playable[cpos] && roadable[cpos] && HasFlatPavedRoadFootprint(cpos);

			bool HasFlatPavedRoadFootprint(CPos cpos)
			{
				var height = map.Height[cpos];
				for (var y = -1; y <= 1; y++)
					for (var x = -1; x <= 1; x++)
					{
						var other = cpos + new CVec(x, y);
						if (!map.Contains(other) || map.Height[other] != height)
							return false;
					}

				return true;
			}

			static void FlushPavedRoadGridRun(HashSet<CPos> roadCells, List<CPos> run, int minimumLength)
			{
				if (run.Count >= minimumLength)
					foreach (var cpos in run)
						roadCells.Add(cpos);

				run.Clear();
			}

			Dictionary<CPos, PavedRoadConnections> ConnectPavedRoadGrid(HashSet<CPos> roadCells)
			{
				var roads = new Dictionary<CPos, PavedRoadConnections>();

				foreach (var cpos in roadCells)
				{
					var connections = PavedRoadConnections.None;
					if (roadCells.Contains(cpos + new CVec(0, -1)))
						connections |= PavedRoadConnections.NorthWest;

					if (roadCells.Contains(cpos + new CVec(1, 0)))
						connections |= PavedRoadConnections.NorthEast;

					if (roadCells.Contains(cpos + new CVec(0, 1)))
						connections |= PavedRoadConnections.SouthEast;

					if (roadCells.Contains(cpos + new CVec(-1, 0)))
						connections |= PavedRoadConnections.SouthWest;

					if (connections != PavedRoadConnections.None)
						roads.Add(cpos, connections);
				}

				return roads;
			}

			void PaintPavedRoads(IReadOnlyDictionary<CPos, PavedRoadConnections> roads)
			{
				foreach (var run in PavedRoadRuns(roads.Keys, c => c.X, c => c.Y))
				{
					var perp = new CVec(1, 0);
					foreach (var sub in SplitRoadRun(run, perp))
					{
						PaintPavedRoadRun(sub, param.PavedRoadNorthEastSouthWestTile, perp);
						var cx = sub[0].X;
						var h = map.Height[sub[0]];
						StampRoadEnd(new CPos(cx - 2, sub[0].Y - 3), param.PavedRoadClearNorthWestTile, 4, 3, h);
						StampRoadEnd(new CPos(cx - 1, sub[^1].Y + 1), param.PavedRoadClearSouthEastTile, 4, 3, h);
					}
				}

				foreach (var run in PavedRoadRuns(roads.Keys, c => c.Y, c => c.X))
				{
					var perp = new CVec(0, 1);
					foreach (var sub in SplitRoadRun(run, perp))
					{
						PaintPavedRoadRun(sub, param.PavedRoadNorthWestSouthEastTile, perp);
						var cy = sub[0].Y;
						var h = map.Height[sub[0]];
						StampRoadEnd(new CPos(sub[0].X - 3, cy - 1), param.PavedRoadClearSouthWestTile, 3, 4, h);
						StampRoadEnd(new CPos(sub[^1].X + 1, cy - 1), param.PavedRoadClearNorthEastTile, 3, 4, h);
					}
				}

				foreach (var kv in roads)
					PaintPavedRoadJunction(kv.Key, kv.Value);
			}

			IEnumerable<List<CPos>> SplitRoadRun(IReadOnlyList<CPos> run, CVec perp)
			{
				var sub = new List<CPos>();
				foreach (var cpos in run)
				{
					if (IsRoadCellValid(cpos - perp) && IsRoadCellValid(cpos) && IsRoadCellValid(cpos + perp))
					{
						sub.Add(cpos);
					}
					else
					{
						if (sub.Count > 1)
							yield return sub;
						sub = [];
					}
				}

				if (sub.Count > 1)
					yield return sub;
			}

			bool IsRoadCellValid(CPos cpos)
			{
				if (!map.Contains(cpos) || !playable[cpos])
					return false;
				var t = map.Tiles[cpos].Type;
				return !(t > 0 && param.RampTiles.Contains(t));
			}

			void StampRoadEnd(CPos origin, ushort template, int width, int height, byte roadHeight)
			{
				for (var y = 0; y < height; y++)
					for (var x = 0; x < width; x++)
					{
						var cpos = origin + new CVec(x, y);
						if (!map.Contains(cpos) || !playable[cpos])
							continue;
						if (map.Height[cpos] != roadHeight)
							continue;
						if (param.RampTiles.Contains(map.Tiles[cpos].Type))
							continue;
						if (!HasFlatPavedRoadFootprint(cpos))
							continue;
						map.Tiles[cpos] = new TerrainTile(template, (byte)(y * width + x));
					}
			}

			static IEnumerable<List<CPos>> PavedRoadRuns(
				IEnumerable<CPos> cells,
				Func<CPos, int> groupBy,
				Func<CPos, int> orderBy)
			{
				foreach (var group in cells.GroupBy(groupBy))
				{
					var run = new List<CPos>();
					var previous = int.MinValue;
					foreach (var cpos in group.OrderBy(orderBy))
					{
						var current = orderBy(cpos);
						if (run.Count > 0 && current != previous + 1)
						{
							if (run.Count > 1)
								yield return run;

							run = [];
						}

						run.Add(cpos);
						previous = current;
					}

					if (run.Count > 1)
						yield return run;
				}
			}

			void PaintPavedRoadRun(IReadOnlyList<CPos> run, ushort template, CVec perp)
			{
				foreach (var cpos in run)
				{
					SetRoadTile(cpos - perp, template, 0);
					SetRoadTile(cpos, template, 1);
					SetRoadTile(cpos + perp, template, 2);
				}
			}

			void PaintPavedRoadJunction(CPos cpos, PavedRoadConnections connections)
			{
				var connected =
					((connections & PavedRoadConnections.NorthWest) != 0 ? 1 : 0) +
					((connections & PavedRoadConnections.NorthEast) != 0 ? 1 : 0) +
					((connections & PavedRoadConnections.SouthEast) != 0 ? 1 : 0) +
					((connections & PavedRoadConnections.SouthWest) != 0 ? 1 : 0);

				if (connected >= 4)
				{
					StampSquarePavedRoad(cpos, param.PavedRoadCrossingTile);
					return;
				}

				if (connected == 1)
					return;

				if (connected == 3)
				{
					var missing = (~connections) &
						(PavedRoadConnections.NorthWest | PavedRoadConnections.NorthEast |
							PavedRoadConnections.SouthEast | PavedRoadConnections.SouthWest);
					StampSquarePavedRoad(cpos, PavedRoadTJunctionTile(missing));
					return;
				}

				if (connected == 2 &&
					(connections & (PavedRoadConnections.NorthWest | PavedRoadConnections.SouthEast)) != 0 &&
					(connections & (PavedRoadConnections.NorthEast | PavedRoadConnections.SouthWest)) != 0)
					StampSquarePavedRoad(cpos, param.PavedRoadCrossingTile);
			}

			ushort PavedRoadTJunctionTile(PavedRoadConnections missing)
			{
				return missing switch
				{
					PavedRoadConnections.NorthEast => param.PavedRoadTJunctionNorthEastTile,
					PavedRoadConnections.SouthEast => param.PavedRoadTJunctionSouthEastTile,
					PavedRoadConnections.SouthWest => param.PavedRoadTJunctionSouthWestTile,
					PavedRoadConnections.NorthWest => param.PavedRoadTJunctionNorthWestTile,
					_ => param.PavedRoadCrossingTile,
				};
			}

			void StampSquarePavedRoad(CPos center, ushort template)
			{
				for (var y = -1; y <= 1; y++)
					for (var x = -1; x <= 1; x++)
						SetRoadTile(center + new CVec(x, y), template, (byte)((y + 1) * 3 + x + 1));
			}

			void SetRoadTile(CPos cpos, ushort template, byte index)
			{
				if (!map.Contains(cpos) || !playable[cpos])
					return;
				var tileType = map.Tiles[cpos].Type;
				if (tileType > 0 && param.RampTiles.Contains(tileType))
					return;
				map.Tiles[cpos] = new TerrainTile(template, index);
			}

			void DecorateFloorTiles(ushort tile, int fraction, CellLayer<bool> addIn = null)
			{
				var tileable = terraformer.CheckSpace(param.LandTile);
				var noise = terraformer.BooleanNoise(groundTypeNoiseRandom, 10240, fraction);
				noise = CellLayerUtils.Intersect([noise, zoneable]);
				if (addIn != null)
					noise = CellLayerUtils.Union([noise, addIn]);

				noise = CellLayerUtils.Intersect([noise, tileable]);
				noise = terraformer.ImproveSymmetry(noise, true, (a, b) => a && b);
				foreach (var cpos in map.Tiles.CellRegion)
					if (noise[cpos])
						map.Tiles[cpos] = new TerrainTile(tile, 0);
			}

			void DecorateBrushes(IReadOnlyList<MultiBrush> brushes, int fraction, CellLayer<bool> paintable)
			{
				if (fraction <= 0 || brushes.Count == 0)
					return;

				var noise = terraformer.BooleanNoise(groundTypeNoiseRandom, 10240, fraction);
				noise = CellLayerUtils.Intersect([noise, paintable]);
				noise = terraformer.ImproveSymmetry(noise, true, (a, b) => a && b);

				var replace = new CellLayer<MultiBrush.Replaceability>(map);
				foreach (var cpos in map.Tiles.CellRegion)
					if (noise[cpos])
						replace[cpos] = MultiBrush.Replaceability.Any;

				terraformer.PaintArea(groundTypeNoiseRandom, replace, brushes, true);
			}

			DecorateBrushes(
				param.IceFieldBrushes,
				param.IceFields,
				terraformer.ErodeZones(terraformer.CheckSpace(param.WaterTile), param.IceFieldWaterBorder));
			DecorateBrushes(
				param.RoughGroundPatchBrushes,
				param.RoughGroundPatches,
				CellLayerUtils.Intersect([zoneable, terraformer.CheckSpace(param.LandTile)]));
			DecorateFloorTiles(param.ForestFloorTile, param.ForestFloor, forestPlan);
			foreach (var (tile, fraction) in param.OtherGround)
				DecorateFloorTiles(tile, fraction);

			// Cosmetically repaint tiles
			terraformer.PaintTiling(pickAnyRandom, param.LatTiler.OfferReplacements(map, pickAnyRandom), 0);
			if (param.IceLatTiler != null && (param.UseIceLatTiler || (param.IceFields > 0 && param.IceFieldBrushes.Count > 0)))
				terraformer.PaintTiling(pickAnyRandom, param.IceLatTiler.OfferReplacements(map, pickAnyRandom), 0);

			terraformer.RepaintTiles(repaintRandom, param.RepaintTiles);

			terraformer.ReorderPlayerSpawns();
			terraformer.BakeMap();

			return map;
		}

		public bool TryGenerateMetadata(ModData modData, MapGenerationArgs args, out MapPlayers players, out Dictionary<string, MiniYaml> ruleDefinitions)
		{
			try
			{
				var playerCount = FieldLoader.GetValue<int>("Players", args.Settings.NodeWithKey("Players").Value.Value);

				// Generated maps use the default ruleset
				ruleDefinitions = [];
				players = new MapPlayers(modData.DefaultRules, playerCount);

				return true;
			}
			catch
			{
				players = null;
				ruleDefinitions = null;
				return false;
			}
		}

		public override object Create(ActorInitializer init)
		{
			return new TSMapGenerator(this);
		}
	}

	public class TSMapGenerator : IEditorTool
	{
		public string Label { get; }
		public string PanelWidget { get; }
		public TraitInfo TraitInfo { get; }
		public bool IsEnabled => true;

		public TSMapGenerator(TSMapGeneratorInfo info)
		{
			Label = info.Name;
			PanelWidget = info.PanelWidget;
			TraitInfo = info;
		}
	}
}
