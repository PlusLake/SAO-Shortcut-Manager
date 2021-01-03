using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using static sao_shortcut_manager.Clickables.Shortcut;
using Button = sao_shortcut_manager.Clickables.Button;
using Shortcut = sao_shortcut_manager.Clickables.Shortcut;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using System.IO;
using sao_shortcut_manager.Clickables;

namespace sao_shortcut_manager {
    [DesignerCategory("")]
    public class OptionWindow : BaseWindow {
        private static readonly Pen ARROW = new Pen(ColorPalette.LIGHT_BRUSH, 16);
        private const int WIDTH = 300;
        private const int HEIGHT = 300;
        private const int RIBBON_HEIGHT = 80;
        private const int BUTTON_SIZE = 50;
        private const int PADDING = 40;
        private const int ICON_SIZE = Shortcut.SIZE - Shortcut.PADDING * 2;
        private const int REPLACE_ICON_X = WIDTH - PADDING - Shortcut.SIZE + Shortcut.PADDING;
        private const int REPLACE_ICON_Y = (HEIGHT - Shortcut.SIZE) / 2 + Shortcut.PADDING;
        private const int ARROW_HALF_LENGTH = 32;
        private readonly Bitmap icon;
        private readonly Action<Bitmap> confirm;
        private readonly Action renderMainWindow;
        private Bitmap replaceIcon;

        public OptionWindow(Bitmap icon, Bitmap replaceIcon, Action<Bitmap> confirm, Action renderMainWindow) {
            this.ClientSize = new Size(WIDTH, HEIGHT);
            this.ShowInTaskbar = false;
            this.AllowDrop = true;
            this.StartPosition = FormStartPosition.CenterParent;
            this.icon = icon;
            this.replaceIcon = replaceIcon;
            this.confirm = confirm;
            this.renderMainWindow = renderMainWindow;
            this.Load += init;
            ARROW.EndCap = LineCap.ArrowAnchor;
        }

        private void init(object sender, EventArgs e) {
            initGraphics();
            new WindowStartupAnimation(this, WIDTH, HEIGHT, RIBBON_HEIGHT, initEvents);
            Func<SolidBrush, SolidBrush, Func<Button, SolidBrush>> center = (dark, light) => button => button.isDown ? dark : button.isHovered ? light : dark;
            Func<SolidBrush, SolidBrush, Func<Button, SolidBrush>> edge = (dark, light) => button => button.isHovered || button.isDown ? light : dark;
            Func<bool, Action> exit = save => () => {
                if (save) {
                    confirm.Invoke(replaceIcon);
                    renderMainWindow.Invoke();
                }
                new WindowExitAnimation(this, WIDTH, HEIGHT);
            };
            new Button(clickables, WIDTH / 4, HEIGHT - RIBBON_HEIGHT / 2, BUTTON_SIZE, exit.Invoke(true))
                .colorCenter(center.Invoke(ColorPalette.BUTTON_YES_NORMAL, ColorPalette.BUTTON_YES_HOVER))
                .colorEdge(edge.Invoke(ColorPalette.BUTTON_YES_NORMAL, ColorPalette.BUTTON_YES_HOVER));
            new Button(clickables, WIDTH / 4 * 3, HEIGHT - RIBBON_HEIGHT / 2, BUTTON_SIZE, exit.Invoke(false))
                .colorCenter(center.Invoke(ColorPalette.BUTTON_NO_NORMAL, ColorPalette.BUTTON_NO_HOVER))
                .colorEdge(edge.Invoke(ColorPalette.BUTTON_NO_NORMAL, ColorPalette.BUTTON_NO_HOVER));
            new ReplaceIcon(clickables, WIDTH - PADDING - Shortcut.SIZE, (HEIGHT - Shortcut.SIZE) / 2, () => replaceIcon = null);
        }

        private void initEvents() {
            this.MouseMove += (sender, e) => {
                bool changed = !clickables.All(clickable => clickable.region.IsVisible(e.Location) == clickable.isHovered);
                clickables.ForEach(clickable => clickable.isHovered = clickable.region.IsVisible(e.Location));
                if (changed) render();
            };
            this.MouseDown += (sender, e) => {
                bool changed = clickables.Any(clickable => !clickable.region.IsVisible(e.Location));
                clickables.ForEach(clickable => clickable.isDown = clickable.region.IsVisible(e.Location));
                if (changed) render();
            };
            this.MouseUp += (sender, e) => {
                clickables.ForEach(clickable => {
                    if (clickable.isDown && clickable.region.IsVisible(e.Location))
                        clickable.run(false);
                    clickable.isDown = false;
                });
                render();
            };
            Func<string, bool> pngTest = file => {
                byte[] header = new byte[8];
                byte[] png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
                using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    stream.Read(header, 0, header.Length);
                return Enumerable.Range(0, 8).All(i => header[i] == png[i]);
            };
            Func<Action<string, DragEventArgs>, DragEventHandler> dragEvent = action => (sender, e) => {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1 && pngTest.Invoke(files[0]))
                    action.Invoke(files[0], e);
            };
            this.DragEnter += dragEvent.Invoke((file, e) => e.Effect = DragDropEffects.Copy);
            this.DragDrop += dragEvent.Invoke((file, e) => replaceIcon = new Bitmap(new Bitmap(file), ICON_SIZE, ICON_SIZE));
        }

        public override void render() {
            graphics.Clear(ColorPalette.DARK);
            graphics.FillRectangle(ColorPalette.DARK_BRUSH, mainArea);
            graphics.DrawImage(icon, PADDING, (HEIGHT - Shortcut.SIZE) / 2 + Shortcut.PADDING, ICON_SIZE, ICON_SIZE);
            graphics.DrawLine(ARROW, WIDTH / 2 - ARROW_HALF_LENGTH, HEIGHT / 2, WIDTH / 2 + ARROW_HALF_LENGTH, HEIGHT / 2);
            clickables.Where(isButton(false)).ToList().ForEach(clickable => clickable.render(graphics));
            if (replaceIcon != null)
                graphics.DrawImage(replaceIcon, REPLACE_ICON_X, REPLACE_ICON_Y, ICON_SIZE, ICON_SIZE);
            renderRibbon(RIBBON_HEIGHT);
            clickables.FindAll(new Predicate<Clickable>(isButton(true))).ForEach(button => button.render(graphics));
            this.Invalidate();
        }
    }
}
