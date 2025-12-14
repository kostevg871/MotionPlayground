using System.Linq;
using Microsoft.Maui.Controls;
using MotionPlayground.Models;
using MotionPlayground.ViewModels;

namespace MotionPlayground
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            if (BindingContext is MainViewModel vm)
            {
                vm.Actor = ActorContainer;
                vm.Stage = Stage;

                if (vm.SelectedAnimation == null && vm.Animations.Count > 0)
                    vm.SelectedAnimation = vm.Animations[0];

                AnimationsCV.SelectedItem = vm.SelectedAnimation;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is MainViewModel vm)
            {
                vm.StopCommand.Execute(null);
                vm.StartCommand.Execute(null);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (BindingContext is MainViewModel vm)
                vm.StopCommand.Execute(null);
        }

        private void OnAnimationsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BindingContext is not MainViewModel vm) return;

            // ВАЖНО: тип берём Models.AnimationItem
            var picked = e.CurrentSelection?.FirstOrDefault() as AnimationItem;
            if (picked != null && picked != vm.SelectedAnimation)
                vm.SelectedAnimation = picked;
        }
    }
}
