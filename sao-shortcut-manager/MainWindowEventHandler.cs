using sao_shortcut_manager.Clickables;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Shortcut = sao_shortcut_manager.Clickables.Shortcut;

namespace sao_shortcut_manager {
    class MainWindowEventHandler {
        public static bool deactivated = false;
        private static bool[] keyState = new bool[256];
        private static Action<int>[] keyAction = new Action<int>[256];
        private MainWindow window;
        private Point location = new Point();
        private bool windowMovable;

        public MainWindowEventHandler(MainWindow window) {
            this.window = window;
            window.MouseDown += mouseDown;
            window.MouseUp += mouseUp;
            window.MouseMove += mouseMove;
            window.MouseLeave += (sender, e) => deactivated = true;
            window.MouseEnter += (sender, e) => deactivated = false;
            window.KeyDown += (sender, e) => changeKeyState(e.KeyValue, true);
            window.KeyUp += (sender, e) => changeKeyState(e.KeyValue, false);
            initKey();
        }

        private void initKey() {
            keyAction[17] = keyAction[112] = key => {
                if (keyState[17] && keyState[112]) {
                    window.isEditMode = !window.isEditMode;
                    Shortcut.clearSwapping();
                }
            };
            keyAction[37] = keyAction[38] = keyAction[39] = keyAction[40] = key => {
                if (!window.isKeyboardMode && !Shortcut.anyHovering()) {
                    window.isKeyboardMode = true;
                    Shortcut.focusLeftTop();
                } else if (window.isKeyboardMode) {
                    Shortcut.keyboardMove((key - 37 + 3) % 4);
                }
            };
        }

        private void changeKeyState(int keyCode, bool state) {
            if (keyCode >= 256 || keyCode < 0)
                return;
            bool previousState = keyState[keyCode];
            keyState[keyCode] = state;
            if (state && !previousState && keyAction[keyCode] != null)
                keyAction[keyCode].Invoke(keyCode);
            if (keyCode == 32 && window.isKeyboardMode) {
                Shortcut.isKeydown = state;
                if (!state)
                    Shortcut.spacebarPressed();
            }
        }

        private void mouseUp(object sender, MouseEventArgs e) {
            deactivated = windowMovable = false;
            Point point = new Point(Cursor.Position.X - window.Location.X, Cursor.Position.Y - window.Location.Y);
            Action action = () => { };
            window.clickables.ForEach(clickable => {
                if (clickable.isDown && clickable.region.IsVisible(point))
                    action = () => clickable.run(e.Button == MouseButtons.Right);
                clickable.isDown = false;
            });
            window.render();
            action.Invoke();
        }

        private void mouseDown(object sender, MouseEventArgs e) {
            deactivated = false;
            Point point = new Point(Cursor.Position.X - window.Location.X, Cursor.Position.Y - window.Location.Y);
            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(window.mainArea);
            windowMovable = !path.IsVisible(point, window.graphics);
            location = new Point(Cursor.Position.X - window.Left, Cursor.Position.Y - window.Top);
            window.clickables.ForEach(clickable => clickable.isDown = clickable.region.IsVisible(point));
        }
        private void mouseMove(object sender, MouseEventArgs e) {
            deactivated = false;
            if (windowMovable && window.clickables.TrueForAll(v => !v.isDown)) {
                window.Top = Cursor.Position.Y - location.Y;
                window.Left = Cursor.Position.X - location.X;
            }
            Clickable downed = window.clickables.Find(clickable => clickable.isDown);
            window.clickables.ForEach(clickable => {
                clickable.isHovered = (downed == null || downed == clickable) && clickable.region.IsVisible(e.Location);
            });
        }
    }
}
