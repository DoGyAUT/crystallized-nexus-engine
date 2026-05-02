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

using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[TraitLocation(SystemActors.World)]
	[Desc("Allows lobby options to be grouped under category headers.")]
	public class LobbyOptionCategoryInfo : TraitInfo<LobbyOptionCategory>
	{
		[FieldLoader.Require]
		[Desc("Category id referenced by lobby options.")]
		public readonly string Category = null;

		[Desc("Category title shown in the lobby.")]
		public readonly string Title = null;

		[Desc("Display order for this category in the lobby.")]
		public readonly int DisplayOrder = 0;
	}

	public class LobbyOptionCategory { }
}
