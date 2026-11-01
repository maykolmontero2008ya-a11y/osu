// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Select
{
    /// <summary>
    /// NOCTURNE: music preview controls (play/pause, progress bar, elapsed / total time) for the current track.
    /// Only drives the existing global music controller; no audio logic is changed.
    /// </summary>
    public partial class NocturnePreviewBar : CompositeDrawable
    {
        [Resolved]
        private MusicController music { get; set; } = null!;

        private SpriteIcon icon = null!;
        private Box progress = null!;
        private OsuSpriteText timeText = null!;
        private Container buttonBody = null!;

        private bool lastPlaying;
        private int lastSecond = -1;

        public NocturnePreviewBar()
        {
            RelativeSizeAxes = Axes.X;
            Height = 30;
            Padding = new MarginPadding { Right = 40 };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                buttonBody = new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(28),
                    Masking = true,
                    CornerRadius = 14,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(NocturneTheme.PLAY_START, NocturneTheme.PLAY_END),
                        },
                        icon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(11),
                            Icon = FontAwesome.Solid.Play,
                            Colour = NocturneTheme.TEXT_PRIMARY,
                        },
                    },
                },
                new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 4,
                    Margin = new MarginPadding { Left = 38, Right = 110 },
                    Masking = true,
                    CornerRadius = 2,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = NocturneTheme.DEEP_VIOLET,
                        },
                        progress = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0,
                            Colour = ColourInfo.GradientHorizontal(NocturneTheme.ACCENT_PURPLE, NocturneTheme.PINK),
                        },
                    },
                },
                timeText = new OsuSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Font = OsuFont.GetFont(size: 13, weight: FontWeight.SemiBold),
                    Colour = NocturneTheme.TEXT_SECONDARY,
                    Text = "0:00 / 0:00",
                },
            };
        }

        protected override void Update()
        {
            base.Update();

            var track = music.CurrentTrack;

            double length = track.Length;
            double time = track.CurrentTime;

            progress.Width = length > 0 ? (float)Math.Clamp(time / length, 0, 1) : 0;

            bool playing = music.IsPlaying;

            if (playing != lastPlaying)
            {
                lastPlaying = playing;
                icon.Icon = playing ? FontAwesome.Solid.Pause : FontAwesome.Solid.Play;
            }

            int second = (int)(time / 1000);

            if (second != lastSecond)
            {
                lastSecond = second;
                timeText.Text = $"{format(time)} / {format(length)}";
            }
        }

        private static string format(double milliseconds) => TimeSpan.FromMilliseconds(Math.Max(0, milliseconds)).ToString(@"m\:ss");

        protected override bool OnClick(ClickEvent e)
        {
            music.TogglePause();
            return true;
        }

        protected override bool OnHover(HoverEvent e)
        {
            buttonBody.ScaleTo(1.1f, NocturneTheme.HOVER_DURATION, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            buttonBody.ScaleTo(1f, NocturneTheme.HOVER_DURATION * 2, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
