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
using OpenRA.Graphics;
using OpenRA.Mods.Cnc.Traits;
using OpenRA.Primitives;

namespace OpenRA.Mods.Cnc.Graphics
{
	public class ModelRenderable : IPalettedRenderable, IModifyableRenderable
	{
		readonly ModelRenderer renderer;
		readonly IEnumerable<ModelAnimation> models;
		readonly WRot camera;
		readonly WRot lightSource;
		readonly ImmutableArray<float> lightAmbientColor;
		readonly ImmutableArray<float> lightDiffuseColor;
		readonly PaletteReference normalsPalette;
		readonly PaletteReference shadowPalette;
		readonly Func<int?> shadowGroundZFunc;
		readonly float scale;
		readonly bool isDecoration;
		readonly bool reflectZ;

		public ModelRenderable(
			ModelRenderer renderer, IEnumerable<ModelAnimation> models, WPos pos, int zOffset, in WRot camera, float scale,
			in WRot lightSource, ImmutableArray<float> lightAmbientColor, ImmutableArray<float> lightDiffuseColor,
			PaletteReference color, PaletteReference normals, PaletteReference shadow)
			: this(renderer, models, pos, zOffset, camera, scale,
				lightSource, lightAmbientColor, lightDiffuseColor,
				color, normals, shadow, 1f,
				float3.Ones, TintModifiers.None)
		{ }

		public ModelRenderable(
			ModelRenderer renderer, IEnumerable<ModelAnimation> models, WPos pos, int zOffset, in WRot camera, float scale,
			in WRot lightSource, ImmutableArray<float> lightAmbientColor, ImmutableArray<float> lightDiffuseColor,
			PaletteReference color, PaletteReference normals, PaletteReference shadow,
			float alpha, in float3 tint, TintModifiers tintModifiers, bool isDecoration = false, bool reflectZ = false)
		{
			this.renderer = renderer;
			this.models = models;
			Pos = pos;
			ZOffset = zOffset;
			this.scale = scale;
			this.camera = camera;
			this.lightSource = lightSource;
			this.lightAmbientColor = lightAmbientColor;
			this.lightDiffuseColor = lightDiffuseColor;
			Palette = color;
			normalsPalette = normals;
			shadowPalette = shadow;
			Alpha = alpha;
			Tint = tint;
			TintModifiers = tintModifiers;
			this.isDecoration = isDecoration;
			this.reflectZ = reflectZ;
		}

		public ModelRenderable(
			ModelRenderer renderer, IEnumerable<ModelAnimation> models, WPos pos, int zOffset, in WRot camera, float scale,
			in WRot lightSource, ImmutableArray<float> lightAmbientColor, ImmutableArray<float> lightDiffuseColor,
			PaletteReference color, PaletteReference normals, PaletteReference shadow,
			float alpha, in float3 tint, TintModifiers tintModifiers, Func<int?> shadowGroundZFunc,
			bool isDecoration = false, bool reflectZ = false)
			: this(renderer, models, pos, zOffset, camera, scale,
				lightSource, lightAmbientColor, lightDiffuseColor,
				color, normals, shadow, alpha, tint, tintModifiers, isDecoration, reflectZ)
		{
			this.shadowGroundZFunc = shadowGroundZFunc;
		}

		public WPos Pos { get; }
		public PaletteReference Palette { get; }
		public int ZOffset { get; }
		public bool IsDecoration => isDecoration;

		public float Alpha { get; }
		public float3 Tint { get; }
		public TintModifiers TintModifiers { get; }

		public IPalettedRenderable WithPalette(PaletteReference newPalette)
		{
			return new ModelRenderable(
				renderer, models, Pos, ZOffset, camera, scale,
				lightSource, lightAmbientColor, lightDiffuseColor,
				newPalette, normalsPalette, shadowPalette, Alpha, Tint, TintModifiers, shadowGroundZFunc, isDecoration, reflectZ);
		}

		public IRenderable WithZOffset(int newOffset)
		{
			return new ModelRenderable(
				renderer, models, Pos, newOffset, camera, scale,
				lightSource, lightAmbientColor, lightDiffuseColor,
				Palette, normalsPalette, shadowPalette, Alpha, Tint, TintModifiers, shadowGroundZFunc, isDecoration, reflectZ);
		}

		public IRenderable OffsetBy(in WVec vec)
		{
			return new ModelRenderable(
				renderer, models, Pos + vec, ZOffset, camera, scale,
				lightSource, lightAmbientColor, lightDiffuseColor,
				Palette, normalsPalette, shadowPalette, Alpha, Tint, TintModifiers, shadowGroundZFunc, isDecoration, reflectZ);
		}

		public IRenderable AsDecoration()
		{
			return new ModelRenderable(
				renderer, models, Pos, ZOffset, camera, scale,
				lightSource, lightAmbientColor, lightDiffuseColor,
				Palette, normalsPalette, shadowPalette, Alpha, Tint, TintModifiers, shadowGroundZFunc, true, reflectZ);
		}

		public ModelRenderable WithZReflection()
		{
			return new ModelRenderable(
				renderer, models, Pos, ZOffset, camera, scale,
				lightSource, lightAmbientColor, lightDiffuseColor,
				Palette, normalsPalette, shadowPalette, Alpha, Tint, TintModifiers, shadowGroundZFunc, isDecoration, true);
		}

		public IModifyableRenderable WithAlpha(float newAlpha)
		{
			return new ModelRenderable(
				renderer, models, Pos, ZOffset, camera, scale,
				lightSource, lightAmbientColor, lightDiffuseColor,
				Palette, normalsPalette, shadowPalette, newAlpha, Tint, TintModifiers, shadowGroundZFunc, isDecoration, reflectZ);
		}

		public IModifyableRenderable WithTint(in float3 newTint, TintModifiers newTintModifiers)
		{
			return new ModelRenderable(
				renderer, models, Pos, ZOffset, camera, scale,
				lightSource, lightAmbientColor, lightDiffuseColor,
				Palette, normalsPalette, shadowPalette, Alpha, newTint, newTintModifiers, shadowGroundZFunc, isDecoration, reflectZ);
		}

		public IFinalizedRenderable PrepareRender(WorldRenderer wr)
		{
			return new FinalizedModelRenderable(wr, this);
		}

		sealed class FinalizedModelRenderable : IFinalizedRenderable
		{
			readonly ModelRenderable model;
			readonly ModelRenderProxy renderProxy;

			public FinalizedModelRenderable(WorldRenderer wr, ModelRenderable model)
			{
				this.model = model;
				var draw = model.models.Where(v => v.IsVisible);

				var map = wr.World.Map;
				var groundOrientation = map.TerrainOrientation(map.CellContaining(model.Pos));
				renderProxy = model.renderer.RenderAsync(
					wr, draw, model.camera, model.scale, groundOrientation, model.lightSource,
					model.lightAmbientColor, model.lightDiffuseColor,
					model.Palette, model.normalsPalette, model.shadowPalette, model.reflectZ);
			}

			public void Render(WorldRenderer wr)
			{
				var map = wr.World.Map;
				var shadowGroundZ = model.shadowGroundZFunc?.Invoke() ?? model.Pos.Z - map.DistanceAboveTerrain(model.Pos).Length;
				var groundZ = (float)map.Rules.TerrainInfo.TileSize.Height * (shadowGroundZ - model.Pos.Z) / map.Grid.TileScale;
				var pxOrigin = wr.Screen3DPosition(model.Pos);

				// HACK: We don't have enough texture channels to pass the depth data to the shader
				// so for now just offset everything forward so that the back corner is rendered at pos.
				pxOrigin -= new float3(0, 0, Screen3DBounds(wr).Z.X);

				// HACK: The previous hack isn't sufficient for the ramp type that is half flat and half
				// sloped towards the camera. Offset it by another half cell to avoid clipping.
				var cell = map.CellContaining(model.Pos);
				if (map.Ramp.Contains(cell) && map.Ramp[cell] == 7)
					pxOrigin += new float3(0, 0, 0.5f * map.Rules.TerrainInfo.TileSize.Height);

				var shadowOrigin = pxOrigin - groundZ * new float2(renderProxy.ShadowDirection, 1);

				var psb = renderProxy.ProjectedShadowBounds;
				var sa = shadowOrigin + psb[0];
				var sb = shadowOrigin + psb[2];
				var sc = shadowOrigin + psb[1];
				var sd = shadowOrigin + psb[3];

				var wrsr = Game.Renderer.WorldRgbaSpriteRenderer;
				var t = model.Tint;
				if (wr.TerrainLighting != null && (model.TintModifiers & TintModifiers.IgnoreWorldTint) == 0)
					t *= wr.TerrainLighting.TintAt(model.Pos);

				// Shader interprets negative alpha as a flag to use the tint colour directly instead of multiplying the sprite colour
				var a = model.Alpha;
				if ((model.TintModifiers & TintModifiers.ReplaceColor) != 0)
					a *= -1;

				wrsr.DrawSprite(renderProxy.ShadowSprite, sa, sb, sc, sd, t, a);
				var spritePos = pxOrigin - 0.5f * renderProxy.Sprite.Size;
				wrsr.DrawSprite(renderProxy.Sprite, spritePos, 1f, t, a);
			}

			public void RenderDebugGeometry(WorldRenderer wr)
			{
				var map = wr.World.Map;
				var shadowGroundZ = model.shadowGroundZFunc?.Invoke() ?? model.Pos.Z - map.DistanceAboveTerrain(model.Pos).Length;
				var groundZ = (float)map.Rules.TerrainInfo.TileSize.Height * (shadowGroundZ - model.Pos.Z) / map.Grid.TileScale;
				var pxOrigin = wr.Screen3DPosition(model.Pos);
				var shadowOrigin = pxOrigin - groundZ * new float2(renderProxy.ShadowDirection, 1);

				// Draw sprite rect
				var offset = pxOrigin + renderProxy.Sprite.Offset - 0.5f * renderProxy.Sprite.Size;
				var tl = wr.Viewport.WorldToViewPx(offset.XY);
				var br = wr.Viewport.WorldToViewPx((offset + renderProxy.Sprite.Size).XY);
				Game.Renderer.RgbaColorRenderer.DrawRect(tl, br, 1, Color.Red);

				// Draw transformed shadow sprite rect
				var c = Color.Purple;
				var psb = renderProxy.ProjectedShadowBounds;

				Game.Renderer.RgbaColorRenderer.DrawPolygon(new float2[]
				{
					wr.Viewport.WorldToViewPx(shadowOrigin + psb[1]),
					wr.Viewport.WorldToViewPx(shadowOrigin + psb[3]),
					wr.Viewport.WorldToViewPx(shadowOrigin + psb[0]),
					wr.Viewport.WorldToViewPx(shadowOrigin + psb[2])
				}, 1, c);

				// Draw bounding box
				var draw = model.models.Where(v => v.IsVisible);
				var scaleTransform = Util.ScaleMatrix(model.scale, model.scale, model.scale);
				var cameraTransform = Util.MakeFloatMatrix(model.camera.AsMatrix());

				foreach (var v in draw)
				{
					var bounds = v.Model.Bounds(v.FrameFunc());
					var rotation = Util.MakeFloatMatrix(v.RotationFunc().AsMatrix());
					var worldTransform = Util.MatrixMultiply(scaleTransform, rotation);

					var pxPos = pxOrigin + wr.ScreenVectorComponents(v.OffsetFunc());
					var screenTransform = Util.MatrixMultiply(cameraTransform, worldTransform);
					DrawBoundsBox(wr, pxPos, screenTransform, bounds, 1, Color.Yellow);
				}
			}

			static readonly uint[] CornerXIndex = [0, 0, 0, 0, 3, 3, 3, 3];
			static readonly uint[] CornerYIndex = [1, 1, 4, 4, 1, 1, 4, 4];
			static readonly uint[] CornerZIndex = [2, 5, 2, 5, 2, 5, 2, 5];
			static void DrawBoundsBox(WorldRenderer wr, in float3 pxPos, float[] transform, float[] bounds, float width, Color c)
			{
				var cr = Game.Renderer.RgbaColorRenderer;
				var corners = new float2[8];
				for (var i = 0; i < 8; i++)
				{
					var vec = new[] { bounds[CornerXIndex[i]], bounds[CornerYIndex[i]], bounds[CornerZIndex[i]], 1 };
					var screen = Util.MatrixVectorMultiply(transform, vec);
					corners[i] = wr.Viewport.WorldToViewPx(pxPos + new float3(screen[0], screen[1], screen[2]));
				}

				// Front face
				cr.DrawPolygon(new[] { corners[0], corners[1], corners[3], corners[2] }, width, c);

				// Back face
				cr.DrawPolygon(new[] { corners[4], corners[5], corners[7], corners[6] }, width, c);

				// Horizontal edges
				cr.DrawLine(corners[0], corners[4], width, c);
				cr.DrawLine(corners[1], corners[5], width, c);
				cr.DrawLine(corners[2], corners[6], width, c);
				cr.DrawLine(corners[3], corners[7], width, c);
			}

			public Rectangle ScreenBounds(WorldRenderer wr)
			{
				return Screen3DBounds(wr).Bounds;
			}

			(Rectangle Bounds, float2 Z) Screen3DBounds(WorldRenderer wr)
			{
				var pxOrigin = wr.ScreenPosition(model.Pos);
				var draw = model.models.Where(v => v.IsVisible);
				var scaleTransform = Util.ScaleMatrix(model.scale, model.scale, model.scale);
				var cameraTransform = Util.MakeFloatMatrix(model.camera.AsMatrix());

				var minX = float.MaxValue;
				var minY = float.MaxValue;
				var minZ = float.MaxValue;
				var maxX = float.MinValue;
				var maxY = float.MinValue;
				var maxZ = float.MinValue;

				foreach (var v in draw)
				{
					var bounds = v.Model.Bounds(v.FrameFunc());
					var rotation = Util.MakeFloatMatrix(v.RotationFunc().AsMatrix());
					var worldTransform = Util.MatrixMultiply(scaleTransform, rotation);

					var pxPos = pxOrigin + wr.ScreenVectorComponents(v.OffsetFunc());
					var screenTransform = Util.MatrixMultiply(cameraTransform, worldTransform);

					for (var i = 0; i < 8; i++)
					{
						var vec = new float[] { bounds[CornerXIndex[i]], bounds[CornerYIndex[i]], bounds[CornerZIndex[i]], 1 };
						var screen = Util.MatrixVectorMultiply(screenTransform, vec);
						minX = Math.Min(minX, pxPos.X + screen[0]);
						minY = Math.Min(minY, pxPos.Y + screen[1]);
						minZ = Math.Min(minZ, pxPos.Z + screen[2]);
						maxX = Math.Max(maxX, pxPos.X + screen[0]);
						maxY = Math.Max(maxY, pxPos.Y + screen[1]);
						maxZ = Math.Max(minZ, pxPos.Z + screen[2]);
					}
				}

				return (Rectangle.FromLTRB((int)minX, (int)minY, (int)maxX, (int)maxY), new float2(minZ, maxZ));
			}
		}
	}
}
