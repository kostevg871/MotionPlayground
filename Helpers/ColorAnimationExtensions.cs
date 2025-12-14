using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MotionPlayground.Helpers
{
    public static class ColorAnimationExtensions
    {
        public static void ColorTo(
            this VisualElement self,
            Color fromColor,
            Color toColor,
            Action<Color> callback,
            uint length = 250,
            Easing easing = null)
        {
            var transform = new Animation(v =>
            {
                var r = fromColor.Red + (toColor.Red - fromColor.Red) * v;
                var g = fromColor.Green + (toColor.Green - fromColor.Green) * v;
                var b = fromColor.Blue + (toColor.Blue - fromColor.Blue) * v;
                var a = fromColor.Alpha + (toColor.Alpha - fromColor.Alpha) * v;

                callback(Color.FromRgba((float)r, (float)g, (float)b, (float)a));
            });

            self.Animate("ColorTo", transform, 16, length, easing ?? Easing.Linear);
        }
    }
}
