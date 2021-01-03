using System.Drawing;

namespace sao_shortcut_manager
{
    class ColorPalette
    {
        // Window
        public readonly static Color LIGHT = Color.FromArgb(251, 251, 251);
        public readonly static Color DARK = Color.FromArgb(215, 215, 215);
        public readonly static SolidBrush LIGHT_BRUSH = new SolidBrush(LIGHT);
        public readonly static SolidBrush DARK_BRUSH = new SolidBrush(DARK);

        // Button
        public readonly static SolidBrush BUTTON_NORMAL = brush(128, 128, 128);
        public readonly static SolidBrush BUTTON_HOVER = brush(235, 166, 1);
        public readonly static SolidBrush BUTTON_DOWN = brush(200, 140, 1);

        // ButtonYesNo (Yes: blue, No: red)
        public readonly static SolidBrush BUTTON_YES_NORMAL = brush(16, 128, 224);
        public readonly static SolidBrush BUTTON_YES_HOVER = brush(24, 196, 255);
        public readonly static SolidBrush BUTTON_NO_NORMAL = brush(224, 16, 16);
        public readonly static SolidBrush BUTTON_NO_HOVER = brush(240, 96, 96);

        private static SolidBrush brush(int r, int g, int b) {
            return new SolidBrush(Color.FromArgb(r, g, b));
        }
    }
}
