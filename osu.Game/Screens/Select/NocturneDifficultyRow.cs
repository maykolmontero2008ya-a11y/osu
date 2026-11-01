// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select
{
    /// <summary>
    /// NOCTURNE: a horizontal row of difficulty cards for the currently selected beatmap set.
    /// Clicking a card selects that difficulty through the existing song select selection flow.
    /// </summary>
    public partial class NocturneDifficultyRow : CompositeDrawable
    {
        public const float ROW_HEIGHT = 86;

        private const int max_visible = 5;

        private readonly Action<BeatmapInfo> onSelected;

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; } = null!;

        private FillFlowContainer flow = null!;
        private readonly List<DifficultyCard> cards = new List<DifficultyCard>();

        public NocturneDifficultyRow(Action<BeatmapInfo> onSelected)
        {
            this.onSelected = onSelected;

            RelativeSizeAxes = Axes.X;
            Height = ROW_HEIGHT;

            // Same un-shearing as the other left side content.
            Shear = -OsuGame.SHEAR;
            Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                Height = DifficultyCard.CARD_HEIGHT,
                Y = 10,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(10, 0),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            working.BindValueChanged(_ => refresh(), true);
        }

        private void refresh()
        {
            var current = working.Value.BeatmapInfo;

            var all = working.Value.BeatmapSetInfo.Beatmaps
                             .Where(b => !b.Hidden && b.Ruleset.ShortName == current.Ruleset.ShortName)
                             .OrderBy(b => b.StarRating)
                             .ToList();

            if (all.Count == 0)
            {
                flow.Clear();
                cards.Clear();
                return;
            }

            int selectedIndex = Math.Max(0, all.FindIndex(b => b.Equals(current)));
            int start = Math.Clamp(selectedIndex - max_visible / 2, 0, Math.Max(0, all.Count - max_visible));
            var visible = all.Skip(start).Take(max_visible).ToList();

            bool sameCards = cards.Count == visible.Count && cards.Select(c => c.Beatmap.ID).SequenceEqual(visible.Select(b => b.ID));

            if (sameCards)
            {
                foreach (var card in cards)
                    card.SetSelected(card.Beatmap.Equals(current), true);

                return;
            }

            flow.Clear();
            cards.Clear();

            foreach (var beatmap in visible)
            {
                var card = new DifficultyCard(beatmap, onSelected);
                card.SetSelected(beatmap.Equals(current), false);
                cards.Add(card);
                flow.Add(card);
            }
        }

        private partial class DifficultyCard : CompositeDrawable
        {
            public const float CARD_WIDTH = 124;
            public const float CARD_HEIGHT = 66;

            public readonly BeatmapInfo Beatmap;

            private readonly Action<BeatmapInfo> onClick;

            private Container body = null!;
            private Box dim = null!;
            private Box hover = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public DifficultyCard(BeatmapInfo beatmap, Action<BeatmapInfo> onClick)
            {
                Beatmap = beatmap;
                this.onClick = onClick;

                Size = new Vector2(CARD_WIDTH, CARD_HEIGHT);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                double stars = Beatmap.StarRating;
                Color4 diffColour = stars >= 0 ? colours.ForStarDifficulty(stars) : NocturneTheme.ACCENT_PURPLE;

                InternalChild = body = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 12,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = NocturneTheme.DEEP_VIOLET,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(diffColour.Opacity(0.55f), diffColour.Opacity(0.05f)),
                        },
                        dim = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = NocturneTheme.BASE,
                            Alpha = 0.45f,
                        },
                        hover = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                            Blending = BlendingParameters.Additive,
                            Alpha = 0,
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Position = new Vector2(10, 9),
                            Width = CARD_WIDTH - 20,
                            Truncate = true,
                            Text = Beatmap.DifficultyName,
                            Font = OsuFont.GetFont(size: 15, weight: FontWeight.SemiBold),
                            Colour = NocturneTheme.TEXT_PRIMARY,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Position = new Vector2(10, -9),
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(4, 0),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = stars >= 0 ? stars.ToString("0.00") : "-",
                                    Font = OsuFont.GetFont(size: 17, weight: FontWeight.Bold),
                                    Colour = NocturneTheme.TEXT_PRIMARY,
                                },
                                new SpriteIcon
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Icon = FontAwesome.Solid.Star,
                                    Size = new Vector2(11),
                                    Colour = NocturneTheme.PINK,
                                },
                            },
                        },
                    },
                };

                SetSelected(isSelected, false);
            }

            private bool isSelected;

            public void SetSelected(bool selected, bool animate)
            {
                isSelected = selected;

                // not loaded yet; `load` will apply the state.
                if (body == null)
                    return;

                double duration = animate ? NocturneTheme.SELECT_DURATION : 0;

                if (selected)
                {
                    body.BorderThickness = 2.5f;
                    body.BorderColour = ColourInfo.GradientHorizontal(NocturneTheme.ACCENT_PURPLE, NocturneTheme.PINK);
                    body.EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 14,
                        Colour = NocturneTheme.LAVENDER.Opacity(0.6f),
                        Hollow = true,
                    };
                }
                else
                {
                    body.BorderThickness = 1f;
                    body.BorderColour = NocturneTheme.ACCENT_PURPLE.Opacity(0.25f);
                    body.EdgeEffect = default;
                }

                body.ScaleTo(selected ? 1.07f : 1f, duration, Easing.OutQuint);
                body.MoveToY(selected ? -6 : 0, duration, Easing.OutQuint);
                dim.FadeTo(selected ? 0f : 0.45f, duration, Easing.OutQuint);
            }

            protected override bool OnClick(ClickEvent e)
            {
                onClick(Beatmap);
                return true;
            }

            protected override bool OnHover(HoverEvent e)
            {
                hover.FadeTo(0.12f, NocturneTheme.HOVER_DURATION, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                hover.FadeTo(0f, NocturneTheme.HOVER_DURATION * 2, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }
    }
}
