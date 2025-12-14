using Microsoft.Maui.Graphics;

namespace MotionPlayground.Models
{
    public class AnimationItem
    {
        public string Key { get; set; }
        public string Name { get; set; }

        public Color PreviewColor { get; set; }
        public Color StageBackground { get; set; }

        // "circle" или "square"
        public string ShapeKey { get; set; }
    }
}
