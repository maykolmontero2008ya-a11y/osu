// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;

namespace osu.Game.Screens.Select
{
    /// <summary>
    /// Colour palette and shared timings for the NOCTURNE song select redesign.
    /// </summary>
    public static class NocturneTheme
    {
        public static readonly Color4 BASE = Color4Extensions.FromHex(@"0B0B1B");
        public static readonly Color4 DEEP_VIOLET = Color4Extensions.FromHex(@"292044");
        public static readonly Color4 ACCENT_PURPLE = Color4Extensions.FromHex(@"A78BFA");
        public static readonly Color4 LAVENDER = Color4Extensions.FromHex(@"C084FC");
        public static readonly Color4 PINK = Color4Extensions.FromHex(@"F0ABFC");
        public static readonly Color4 TEXT_PRIMARY = Color4Extensions.FromHex(@"F8F5FF");
        public static readonly Color4 TEXT_SECONDARY = Color4Extensions.FromHex(@"C4B5D5");

        /// <summary>Duration of the selection highlight transition (spec: 120-200 ms).</summary>
        public const double SELECT_DURATION = 180;

        /// <summary>Duration of hover transitions (spec: 80-150 ms).</summary>
        public const double HOVER_DURATION = 100;
    }
}
