using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MotionPlayground.Models;
using MotionPlayground.Views;

namespace MotionPlayground.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public VisualElement Actor { get; set; }
        public Frame Stage { get; set; }

        public Model3DView ModelView { get; set; }


        // ---- 2D / 3D ----
        private bool _use3D;
        public bool Use3D
        {
            get => _use3D;
            set
            {
                if (Set(ref _use3D, value))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Is2DVisible)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Is3DVisible)));

                    // 2D фигуры обновим (для 3D это просто скрыто)
                    ApplyShape();

                    // перезапуск анимации, чтобы сразу увидеть эффект
                    if (Actor != null) _ = StartAsync();
                }
            }
        }

        public bool Is2DVisible => !Use3D;
        public bool Is3DVisible => Use3D;

        public ObservableCollection<string> Themes { get; } = new() { "Dark", "Neon", "Pastel" };

        private string _selectedTheme = "Neon";
        public string SelectedTheme
        {
            get => _selectedTheme;
            set { if (Set(ref _selectedTheme, value)) ApplyTheme(); }
        }

        private Color _pageBackground;
        public Color PageBackground { get => _pageBackground; set => Set(ref _pageBackground, value); }

        private Color _foreground;
        public Color Foreground { get => _foreground; set => Set(ref _foreground, value); }

        private Color _accent;
        public Color Accent { get => _accent; set => Set(ref _accent, value); }

        private Color _accent2;
        public Color Accent2 { get => _accent2; set => Set(ref _accent2, value); }

        private double _speedMultiplier = 1.0;
        public double SpeedMultiplier { get => _speedMultiplier; set => Set(ref _speedMultiplier, value); }

        private bool _combineRotatePulse;
        public bool CombineRotatePulse { get => _combineRotatePulse; set => Set(ref _combineRotatePulse, value); }

        private bool _showCircle = true;
        public bool ShowCircle { get => _showCircle; set => Set(ref _showCircle, value); }

        private bool _showSquare;
        public bool ShowSquare { get => _showSquare; set => Set(ref _showSquare, value); }

        public ObservableCollection<AnimationItem> Animations { get; } = new()
        {
            new AnimationItem{ Key="pulse",  Name="Pulse",  PreviewColor=Color.FromArgb("#38BDF8"), StageBackground=Color.FromArgb("#0EA5E9"), ShapeKey="circle" },
            new AnimationItem{ Key="rotate", Name="Rotate", PreviewColor=Color.FromArgb("#A78BFA"), StageBackground=Color.FromArgb("#7C3AED"), ShapeKey="square" },
            new AnimationItem{ Key="slide",  Name="Slide",  PreviewColor=Color.FromArgb("#34D399"), StageBackground=Color.FromArgb("#10B981"), ShapeKey="square" },
            new AnimationItem{ Key="fade",   Name="Fade",   PreviewColor=Color.FromArgb("#FBBF24"), StageBackground=Color.FromArgb("#F59E0B"), ShapeKey="circle" },
            new AnimationItem{ Key="bounce", Name="Bounce", PreviewColor=Color.FromArgb("#F472B6"), StageBackground=Color.FromArgb("#DB2777"), ShapeKey="square" },
        };

        private AnimationItem _selectedAnimation;
        public AnimationItem SelectedAnimation
        {
            get => _selectedAnimation;
            set
            {
                if (Set(ref _selectedAnimation, value))
                {
                    ApplyStageBackground(animated: true);
                    ApplyShape();
                    if (Actor != null) _ = StartAsync();
                }
            }
        }

        public Command StartCommand { get; }
        public Command StopCommand { get; }

        private CancellationTokenSource _cts;

        public MainViewModel()
        {
            ApplyTheme();
            StartCommand = new Command(async () => await StartAsync());
            StopCommand = new Command(Stop);
        }

        private async Task StartAsync()
        {
            if (Actor == null) return;

            Stop();

            if (SelectedAnimation == null && Animations.Count > 0)
                SelectedAnimation = Animations[0];

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            await Task.Yield();

            try
            {
                ApplyStageBackground(animated: true);

                switch (SelectedAnimation?.Key)
                {
                    case "pulse": RunPulse(token); break;
                    case "rotate": RunRotate(token); break;
                    case "slide": RunSlide(token); break;
                    case "fade": RunFade(token); break;
                    case "bounce": RunBounce(token); break;
                }

                if (CombineRotatePulse)
                {
                    if (SelectedAnimation?.Key != "rotate") RunRotate(token);
                    if (SelectedAnimation?.Key != "pulse") RunPulse(token);
                }
            }
            catch (OperationCanceledException) { }
        }

        private void RunPulse(CancellationToken token)
        {
            if (Actor == null) return;

            var anim = new Microsoft.Maui.Controls.Animation();
            anim.Add(0, 0.5, new Microsoft.Maui.Controls.Animation(v => Actor.Scale = v, 1.0, 1.15));
            anim.Add(0.5, 1.0, new Microsoft.Maui.Controls.Animation(v => Actor.Scale = v, 1.15, 1.0));
            StartInfiniteAnimation("pulse", anim, (uint)(1200 / Math.Max(0.1, SpeedMultiplier)));
        }

        private void RunRotate(CancellationToken token)
        {
            if (Actor == null) return;

            // Если включен 3D — вращаем саму модель через JS
            if (Use3D && ModelView != null)
            {
                var anim3d = new Microsoft.Maui.Controls.Animation(v => ModelView.SetYaw(v), 0, 360, Easing.Linear);
                anim3d.Commit(
                    owner: Actor,
                    name: "rotate",
                    rate: 24,
                    length: (uint)(3000 / Math.Max(0.1, SpeedMultiplier)),
                    easing: Easing.Linear,
                    finished: (v, c) => { },
                    repeat: () => true
                );
                return;
            }

            // Иначе — старое вращение контейнера (2D)
            var anim = new Microsoft.Maui.Controls.Animation(v => Actor.Rotation = v, 0, 360, Easing.Linear);
            StartInfiniteAnimation("rotate", anim, (uint)(3000 / Math.Max(0.1, SpeedMultiplier)));
        }


        private void RunSlide(CancellationToken token)
        {
            if (Actor == null) return;

            var anim = new Microsoft.Maui.Controls.Animation();
            anim.Add(0.00, 0.25, new Microsoft.Maui.Controls.Animation(v => Actor.TranslationX = v, 0, 70, Easing.SinInOut));
            anim.Add(0.25, 0.50, new Microsoft.Maui.Controls.Animation(v => Actor.TranslationX = v, 70, 0, Easing.SinInOut));
            anim.Add(0.50, 0.75, new Microsoft.Maui.Controls.Animation(v => Actor.TranslationX = v, 0, -70, Easing.SinInOut));
            anim.Add(0.75, 1.00, new Microsoft.Maui.Controls.Animation(v => Actor.TranslationX = v, -70, 0, Easing.SinInOut));
            StartInfiniteAnimation("slide", anim, (uint)(2000 / Math.Max(0.1, SpeedMultiplier)));
        }

        private void RunFade(CancellationToken token)
        {
            if (Actor == null) return;

            var anim = new Microsoft.Maui.Controls.Animation();
            anim.Add(0, 0.5, new Microsoft.Maui.Controls.Animation(v => Actor.Opacity = v, 1.0, 0.25, Easing.CubicIn));
            anim.Add(0.5, 1.0, new Microsoft.Maui.Controls.Animation(v => Actor.Opacity = v, 0.25, 1.0, Easing.CubicOut));
            StartInfiniteAnimation("fade", anim, (uint)(1500 / Math.Max(0.1, SpeedMultiplier)));
        }

        private void RunBounce(CancellationToken token)
        {
            if (Actor == null) return;

            var anim = new Microsoft.Maui.Controls.Animation();
            anim.Add(0, 0.5, new Microsoft.Maui.Controls.Animation(v => Actor.TranslationY = v, 0, -50, Easing.CubicOut));
            anim.Add(0.5, 1.0, new Microsoft.Maui.Controls.Animation(v => Actor.TranslationY = v, -50, 0, Easing.BounceOut));
            StartInfiniteAnimation("bounce", anim, (uint)(1500 / Math.Max(0.1, SpeedMultiplier)));
        }

        private void Stop()
        {
            _cts?.Cancel();

            Actor?.AbortAnimation("pulse");
            Actor?.AbortAnimation("rotate");
            Actor?.AbortAnimation("slide");
            Actor?.AbortAnimation("fade");
            Actor?.AbortAnimation("bounce");
            Stage?.AbortAnimation("color");

            if (Actor != null)
            {
                Actor.Opacity = 1;
                Actor.Scale = 1;
                Actor.Rotation = 0;
                Actor.TranslationX = 0;
                Actor.TranslationY = 0;
            }

            if (Use3D && ModelView != null)
                ModelView.ResetYaw();

        }

        private void ApplyTheme()
        {
            switch (SelectedTheme)
            {
                case "Dark":
                    PageBackground = Color.FromArgb("#0B1020");
                    Foreground = Color.FromArgb("#E5E7EB");
                    Accent = Color.FromArgb("#60A5FA");
                    Accent2 = Color.FromArgb("#2563EB");
                    break;

                case "Pastel":
                    PageBackground = Color.FromArgb("#F7F7FB");
                    Foreground = Color.FromArgb("#1E293B");
                    Accent = Color.FromArgb("#8BD3DD");
                    Accent2 = Color.FromArgb("#F6C2D9");
                    break;

                default:
                    PageBackground = Color.FromArgb("#070711");
                    Foreground = Color.FromArgb("#F8FAFC");
                    Accent = Color.FromArgb("#22D3EE");
                    Accent2 = Color.FromArgb("#A78BFA");
                    break;
            }
        }

        private void ApplyStageBackground(bool animated = false)
        {
            var stage = Stage;
            var selected = SelectedAnimation;
            if (stage == null || selected == null) return;

            var to = selected.StageBackground ?? Colors.Transparent;
            var from = stage.BackgroundColor ?? Colors.Transparent;

            if (!animated)
            {
                stage.BackgroundColor = to;
                return;
            }

            var anim = new Microsoft.Maui.Controls.Animation(v =>
            {
                if (stage.Handler == null) return;

                stage.BackgroundColor = Color.FromRgba(
                    (float)(from.Red + (to.Red - from.Red) * v),
                    (float)(from.Green + (to.Green - from.Green) * v),
                    (float)(from.Blue + (to.Blue - from.Blue) * v),
                    (float)(from.Alpha + (to.Alpha - from.Alpha) * v)
                );
            });

            StartStageColorAnimation(anim, 350);
        }

        private void ApplyShape()
        {
            // Если включили 3D — 2D фигуры всё равно скрыты, но оставим логику корректной
            if (SelectedAnimation == null) return;

            ShowCircle = SelectedAnimation.ShapeKey == "circle";
            ShowSquare = SelectedAnimation.ShapeKey == "square";
        }

        private void StartInfiniteAnimation(string key, Microsoft.Maui.Controls.Animation animation, uint length = 1000, uint rate = 24)
        {
            if (Actor == null) return;

            animation.Commit(
                owner: Actor,
                name: key,
                rate: rate,
                length: length,
                easing: Easing.Linear,
                finished: (v, c) => { },
                repeat: () => true);
        }

        private void StartStageColorAnimation(Microsoft.Maui.Controls.Animation anim, uint length = 350)
        {
            if (Stage == null) return;

            anim.Commit(
                owner: Stage,
                name: "color",
                rate: 16,
                length: length,
                easing: Easing.CubicInOut,
                finished: (v, c) => { },
                repeat: () => false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
