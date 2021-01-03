using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace sao_shortcut_manager.Clickables {
    class Button : Clickable {
        private const int WHITE_DISTANCE = 3;
        private const int WHITE_WIDTH = 4;
        private const int WHITE_TOTAL = WHITE_DISTANCE + WHITE_WIDTH;
        private Func<Button, SolidBrush> colorCenterCallback = button => 
            button.isDown ? ColorPalette.BUTTON_DOWN : 
            button.isHovered ? ColorPalette.BUTTON_HOVER :
            ColorPalette.BUTTON_NORMAL;
        private Func<Button, SolidBrush> colorEdgeCallback = button => ColorPalette.BUTTON_NORMAL;
        private readonly Action callback;
        private readonly int size;
        private readonly int radius;

        public Button(List<Clickable> list, int x, int y, int size, Action callback) {
            this.callback = callback;
            this.location.X = x;
            this.location.Y = y;
            this.size = size;
            this.radius = size / 2;
            GraphicsPath path = new GraphicsPath();
            path.AddPie(location.X + WHITE_WIDTH - radius, location.Y + WHITE_WIDTH - radius, size - WHITE_WIDTH * 2, size - WHITE_WIDTH * 2, 0, 360);
            region = new Region(path);
            list.Add(this);
        }
        public override void render(Graphics graphics) {
            graphics.FillPie(colorEdgeCallback.Invoke(this), location.X - radius, location.Y - radius, size, size, 0, 360);
            graphics.FillPie(ColorPalette.LIGHT_BRUSH, location.X + WHITE_DISTANCE - radius, location.Y + WHITE_DISTANCE - radius, size - 2 * WHITE_DISTANCE, size - 2 * WHITE_DISTANCE, 0, 360);
            graphics.FillPie(colorCenterCallback.Invoke(this), location.X + WHITE_TOTAL - radius, location.Y + WHITE_TOTAL - radius, size - 2 * WHITE_TOTAL, size - 2 * WHITE_TOTAL, 0, 360);
        }

        public override void run(bool isRightClick) {
            callback.Invoke();
        }

        public Button colorCenter(Func<Button, SolidBrush> colorCenter) {
            this.colorCenterCallback = colorCenter;
            return this;
        }

        public Button colorEdge(Func<Button, SolidBrush> colorEdge) {
            this.colorEdgeCallback = colorEdge;
            return this;
        }
    }
}
