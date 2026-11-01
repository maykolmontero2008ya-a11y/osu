// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osuTK;

namespace osu.Game.Screens.Select
{
    /// <summary>
    /// NOCTURNE: optional decorative effects (floating particles and subtle background parallax).
    /// Everything here is disabled by the "reduced effects" setting.
    /// </summary>
    public partial class NocturneEffects : CompositeDrawable
    {
        private const int particle_count = 28;
        private const float parallax_range = 22;
        public const float PARALLAX_SCALE = 1.06f;

        private readonly Action<Vector2, bool> applyParallax;
        private readonly Func<bool> isScreenActive;

        private readonly Bindable<bool> reducedEffects = new Bindable<bool>();
        private readonly List<Particle> particles = new List<Particle>();
        private readonly Random random = new Random();

        private Container particleLayer = null!;
        private InputManager inputManager = null!;

        private Vector2 smoothedOffset;
        private Vector2 appliedOffset;
        private bool parallaxApplied;
        private bool wasScreenActive;

        public NocturneEffects(Action<Vector2, bool> applyParallax, Func<bool> isScreenActive)
        {
            this.applyParallax = applyParallax;
            this.isScreenActive = isScreenActive;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.NocturneReducedEffects, reducedEffects);

            AddInternal(particleLayer = new Container
            {
                RelativeSizeAxes = Axes.Both,
            });

            for (int i = 0; i < particle_count; i++)
            {
                float size = 2 + (float)random.NextDouble() * 5;

                var particle = new Particle
                {
                    Circle = new Circle
                    {
                        RelativePositionAxes = Axes.Both,
                        Size = new Vector2(size),
                        Colour = random.NextDouble() < 0.5 ? NocturneTheme.PINK : NocturneTheme.ACCENT_PURPLE,
                        Alpha = 0.15f + (float)random.NextDouble() * 0.35f,
                        Blending = BlendingParameters.Additive,
                    },
                };

                resetParticle(particle, true);
                particles.Add(particle);
                particleLayer.Add(particle.Circle);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager()!;

            reducedEffects.BindValueChanged(v => particleLayer.FadeTo(v.NewValue ? 0 : 1, 200, Easing.OutQuint), true);
        }

        private void resetParticle(Particle p, bool randomY)
        {
            p.BaseX = (float)random.NextDouble();
            p.Y = randomY ? (float)random.NextDouble() : 1.05f;
            p.Speed = 0.012f + (float)random.NextDouble() * 0.04f;
            p.Phase = (float)random.NextDouble() * 6.28f;
            p.Drift = 0.006f + (float)random.NextDouble() * 0.014f;
        }

        protected override void Update()
        {
            base.Update();

            bool screenActive = isScreenActive();

            // Force the parallax state to be re-applied whenever song select becomes active again.
            if (screenActive && !wasScreenActive)
                parallaxApplied = false;

            wasScreenActive = screenActive;

            if (!screenActive)
                return;

            if (reducedEffects.Value)
            {
                if (parallaxApplied)
                {
                    smoothedOffset = Vector2.Zero;
                    appliedOffset = Vector2.Zero;
                    parallaxApplied = false;
                    applyParallax(Vector2.Zero, false);
                }

                return;
            }

            // Particles.
            double elapsed = Time.Elapsed;

            foreach (var p in particles)
            {
                p.Y -= p.Speed * (float)(elapsed / 1000);

                if (p.Y < -0.05f)
                    resetParticle(p, false);

                p.Circle.X = p.BaseX + (float)Math.Sin(p.Phase + Time.Current * 0.0006) * p.Drift;
                p.Circle.Y = p.Y;
            }

            // Parallax.
            Vector2 mouse = ToLocalSpace(inputManager.CurrentState.Mouse.Position);
            Vector2 normalised = new Vector2(
                Math.Clamp(DrawWidth > 0 ? mouse.X / DrawWidth - 0.5f : 0, -0.5f, 0.5f),
                Math.Clamp(DrawHeight > 0 ? mouse.Y / DrawHeight - 0.5f : 0, -0.5f, 0.5f));

            Vector2 target = -normalised * (parallax_range * 2);
            double dt = Math.Abs(elapsed) / 1000;

            smoothedOffset = new Vector2(
                (float)Interpolation.Damp(smoothedOffset.X, target.X, 0.01, dt),
                (float)Interpolation.Damp(smoothedOffset.Y, target.Y, 0.01, dt));

            if (!parallaxApplied || (smoothedOffset - appliedOffset).LengthSquared > 0.25f)
            {
                appliedOffset = smoothedOffset;
                parallaxApplied = true;
                applyParallax(appliedOffset, true);
            }
        }

        private class Particle
        {
            public Circle Circle = null!;
            public float BaseX;
            public float Y;
            public float Speed;
            public float Phase;
            public float Drift;
        }
    }
}
