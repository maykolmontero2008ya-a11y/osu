// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
using osu.Game.Screens.Footer;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select
{
    /// <summary>
    /// NOCTURNE: large gradient PLAY button showing the selected difficulty.
    /// Triggers the exact same start flow as the existing song select actions.
    /// </summary>
    public partial class NocturnePlayButton : CompositeDrawable
    {
        private readonly Action startAction;

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; } = null!;

        private TruncatingSpriteText detailText = null!;
        private Box hover = null!;
        private Container body = null!;

        public NocturnePlayButton(Action startAction)
        {
            this.startAction = startAction;

            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;
            Size = new Vector2(270, 66);

            // Sits just above the footer, to the left of the osu! logo.
            Margin = new MarginPadding { Right = 300, Bottom = ScreenFooter.HEIGHT + 16 };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = body = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 18,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 18,
                    Colour = NocturneTheme.LAVENDER.Opacity(0.5f),
                },
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientHorizontal(NocturneTheme.LAVENDER, NocturneTheme.PINK),
                    },
                    hover = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                        Blending = BlendingParameters.Additive,
                        Alpha = 0,
                    },
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        X = 22,
                        Size = new Vector2(24),
                        Icon = FontAwesome.Solid.Play,
                        Colour = NocturneTheme.BASE,
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        X = 62,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "PLAY",
                                Font = OsuFont.GetFont(size: 26, weight: FontWeight.Bold),
                                Colour = NocturneTheme.BASE,
                                Shadow = false,
                            },
                            detailText = new TruncatingSpriteText
                            {
                                Width = 190,
                                ShowTooltip = false,
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                                Colour = NocturneTheme.BASE.Opacity(0.85f),
                                Shadow = false,
                            },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            working.BindValueChanged(_ => updateText(), true);
        }

        private void updateText()
        {
            var beatmap = working.Value.BeatmapInfo;

            detailText.Text = beatmap.StarRating >= 0
                ? $"{beatmap.DifficultyName} - {beatmap.StarRating:0.00}"
                : beatmap.DifficultyName;
        }

        protected override bool OnClick(ClickEvent e)
        {
            startAction();
            return true;
        }

        protected override bool OnHover(HoverEvent e)
        {
            hover.FadeTo(0.18f, NocturneTheme.HOVER_DURATION, Easing.OutQuint);
            body.ScaleTo(1.03f, NocturneTheme.HOVER_DURATION, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hover.FadeTo(0f, NocturneTheme.HOVER_DURATION * 2, Easing.OutQuint);
            body.ScaleTo(1f, NocturneTheme.HOVER_DURATION * 2, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
