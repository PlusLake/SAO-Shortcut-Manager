using sao_shortcut_manager.Clickables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Button = sao_shortcut_manager.Clickables.Button;

namespace sao_shortcut_manager {
    [DesignerCategory("")]
    public class BaseWindow : Form {
        public List<Clickable> clickables = new List<Clickable>();
        public Rectangle mainArea { get; set; } = new Rectangle();
        public Graphics graphics { get; set; }

        public BaseWindow() {
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = ColorPalette.LIGHT;
            this.FormClosing += (sender, e) => e.Cancel = false; // todo: it will finally becomes true
        }

        public void initGraphics() {
            this.Opacity = 0;
            Bitmap bitmap = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            this.BackgroundImage = bitmap;
            graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.Bicubic;
        }
        public virtual void render() {}

        protected void renderRibbon(int height) {
            graphics.FillRectangle(ColorPalette.LIGHT_BRUSH, 0, mainArea.Y - height, this.Width, height);
            graphics.FillRectangle(ColorPalette.LIGHT_BRUSH, 0, mainArea.Y + mainArea.Height, this.Width, height);
            const int LINE_HEIGHT = 2;
            for (int i = 0; i < 6; i++) {
                int lineColor = 192 + i * 4;
                SolidBrush brush = new SolidBrush(Color.FromArgb(lineColor, lineColor, lineColor));
                graphics.FillRectangle(brush, mainArea.X, mainArea.Y + i * LINE_HEIGHT, mainArea.Width, LINE_HEIGHT);
                graphics.FillRectangle(brush, mainArea.X, mainArea.Y + mainArea.Height - i * LINE_HEIGHT, mainArea.Width, LINE_HEIGHT);
            }
        }

        protected Func<Clickable, bool> isButton(bool b) {
            return clickable => (clickable.GetType() == typeof(Button) == b);
        }
    }
}
