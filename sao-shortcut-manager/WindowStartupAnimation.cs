using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace sao_shortcut_manager {
    class WindowStartupAnimation {
        private const int PADDING = 5;

        private BaseWindow window;
        private int state = 0;
        private int count = 0;
        private double acceralation = 10;
        private double opacityDiff = 0.1;

        private readonly int width;
        private readonly int height;
        private readonly int initY;
        private readonly int initHeight;
        private readonly int initialX;
        private readonly int ribbonHeight;
        private readonly Action animationEnded;

        public WindowStartupAnimation(BaseWindow window, int width, int height, int ribbonHeight, Action animationEnded) {
            this.window = window;
            this.width = width;
            this.height = height;
            this.initY = height / 2 - PADDING - ribbonHeight;
            this.ribbonHeight = ribbonHeight;
            this.initHeight = (ribbonHeight + PADDING) * 2;
            this.initialX = width / 2 - 5;
            this.animationEnded = animationEnded;
            Timer timer = new Timer();
            timer.Interval = 16;
            timer.Tick += timerTick;
            timer.Enabled = true;
        }

        private void timerTick(object sender, EventArgs e) {
            GraphicsPath path = new GraphicsPath();
            if (window.Opacity < 0.95) {
                window.Opacity = Math.Min(window.Opacity + opacityDiff, .95);
                opacityDiff *= .895;
            }
            switch (state) {
                case 0:
                    window.mainArea = new Rectangle(0, height / 2 - PADDING, width, PADDING * 2);
                    count = Math.Min(count + 8, 100);
                    window.render();
                    path.AddRectangle(new Rectangle((int)(initialX - count / 100.0 * initialX), initY, (int) (count / 100.0 * width), initHeight));
                    window.Region = new Region(path);
                    state = count >= 100 ? (count = 0) + 1 : 0;
                    break;
                case 1:
                    const int upperLimit = 150;
                    acceralation *= .975;
                    count = Math.Min((int) acceralation + count, upperLimit);
                    int y = (int) (height / 2 - PADDING - (double) count * initY / upperLimit);
                    window.mainArea = new Rectangle(0, y, width, height - 2 * y);
                    window.render();
                    path.AddRectangle(new Rectangle(0, (int) (initY - (double) count * initY / upperLimit), width, height - 2 * y + ribbonHeight * 2));
                    window.Region = new Region(path);
                    if (count >= upperLimit) {
                        count = 0;
                        ((Timer) sender).Enabled = false;
                        animationEnded.Invoke();
                    }
                    break;
            }
        }
    }
}
