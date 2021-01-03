using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace sao_shortcut_manager {
    class WindowExitAnimation {
        private BaseWindow window;
        private double opacityDiff = 0.15;
        private readonly int halfWidth;
        private readonly int width;
        private readonly int height;

        public WindowExitAnimation(BaseWindow window, int width, int height) {
            this.window = window;
            this.width = width;
            this.halfWidth = width / 2;
            this.height = height;
            Timer timer = new Timer();
            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            timer.Enabled = true;
        }

        private void Timer_Tick(object sender, EventArgs e) {
            window.Opacity -= opacityDiff;
            opacityDiff *= .85;

            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(new Rectangle(halfWidth - (int) (halfWidth * window.Opacity), 0, width - (int) (width * (1 - window.Opacity)), height));
            window.Region = new Region(path);
            if (window.Opacity < 0.01) {
                window.Close();
                if (this.GetType() == typeof(MainWindow)) Application.Exit();
            }
        }
    }
}
