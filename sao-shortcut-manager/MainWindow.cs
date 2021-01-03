using sao_shortcut_manager.Clickables;
using sao_shortcut_manager.Properties;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Button = sao_shortcut_manager.Clickables.Button;

namespace sao_shortcut_manager {
    [DesignerCategory("")]
    public class MainWindow : BaseWindow {
        private const int WIDTH = 800;
        private const int HEIGHT = 600;
        private const int RIBBON_HEIGHT = 100;
        private const int BOTTON_SIZE = 60;

        public bool isEditMode;
        public bool isKeyboardMode;
        private WindowStartupAnimation startupAnimation;

        public MainWindow() {
            this.ClientSize = new Size(WIDTH, HEIGHT);
            this.Icon = Resources.SAO;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "SAO Shortcut Manager";
            this.Load += init;
        }

        private void init(object sender, EventArgs e) {
            new Button(this.clickables, WIDTH - RIBBON_HEIGHT / 2, RIBBON_HEIGHT / 2, BOTTON_SIZE, () => new WindowExitAnimation(this, WIDTH, HEIGHT));
            Clickables.Shortcut.init(clickables, () => isEditMode, () => isKeyboardMode = false, render);
            startupAnimation = new WindowStartupAnimation(this, WIDTH, HEIGHT, RIBBON_HEIGHT, () => new MainWindowEventHandler(this));
            initGraphics();
            Timer timer = new Timer();
            timer.Interval = 20;
            timer.Tick += (_sender, _e) => {
                if (!MainWindowEventHandler.deactivated) this.render();
            };
            timer.Start();
        }

        public override void render() {
            graphics.Clear(ColorPalette.DARK);
            graphics.FillRectangle(ColorPalette.DARK_BRUSH, mainArea);
            clickables.Where(isButton(false)).ToList().ForEach(clickable => clickable.render(graphics));
            renderRibbon(100);
            clickables.Find(new Predicate<Clickable>(isButton(true))).render(graphics);
            if (isEditMode) {
                Point anchor = new Point(10, 577);
                graphics.FillRectangle(ColorPalette.DARK_BRUSH, anchor.X, anchor.Y, 16, 12);
                graphics.FillRectangle(ColorPalette.DARK_BRUSH, anchor.X + 2, anchor.Y - 10, 12, 15);
                graphics.FillRectangle(ColorPalette.LIGHT_BRUSH, anchor.X + 5, anchor.Y - 7, 6, 7);
                graphics.FillRectangle(ColorPalette.LIGHT_BRUSH, anchor.X + 10, anchor.Y - 3, 15, 3);
            }
            this.Invalidate();
        }
    }
}
