using IWshRuntimeLibrary;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using PointDouble = System.Windows.Point;
using PointInt = System.Drawing.Point;
using System.Drawing.Imaging;
using File = System.IO.File;

namespace sao_shortcut_manager.Clickables {
    class Shortcut : Clickable {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern UInt32 PrivateExtractIcons(String lpszFile, int nIconIndex, int cxIcon, int cyIcon, IntPtr[] phicon, IntPtr[] piconid, UInt32 nIcons, UInt32 flags);

        public const int SIZE = 63;
        public const int PADDING = 8;
        public const int IMAGE_SIZE = SIZE - PADDING * 2;
        private static List<Shortcut> shortcuts = new List<Shortcut>();
        private static readonly string PATH = Path.GetDirectoryName(Application.ExecutablePath) + "\\shortcuts\\";
        private static readonly string REPLACE_ICON_PATH = String.Format("{0}/{1}", PATH, "replace.png");
        private static readonly Regex REGEX = new Regex(@"^_\[(\d{2}),(\d{2})\].*");
        private static Func<bool> isEditMode;
        private static Action turnOffKeyboardMode;
        private static Action renderMainWindow;
        private const int COL = 12;
        private const int ROW = 6;

        public static bool isKeydown;
        private static PointInt cursor = new PointInt(-1, -1);
        private bool _isHovered;
        public override bool isHovered {
            get => _isHovered;
            set {
                _isHovered = value;
                if (value && (icon != null || isEditMode.Invoke())) {
                    turnOffKeyboardMode.Invoke();
                    isKeydown = false;
                    cursor.X = -1;
                    cursor.Y = -1;
                } 
            }
        }
        private readonly int x;
        private readonly int y;
        private readonly Rectangle innerRect;
        private Bitmap icon;
        private Bitmap replaceIcon;
        private string executablePath;
        private string shortcutPath;
        private bool isSwapping;

        public Shortcut(List<Clickable> list, int x, int y) {
            this.x = x;
            this.y = y;
            GraphicsPath path = new GraphicsPath();
            location = new PointInt(22, 111);
            location.Offset((SIZE) * x, (SIZE) * y);
            innerRect = new Rectangle(location.X + PADDING, location.Y + PADDING, SIZE - PADDING * 2, SIZE - PADDING * 2);
            path.AddRectangle(new Rectangle(location, new Size(SIZE, SIZE)));
            region = new Region(path);
            list.Add(this);
        }
        public override void render(Graphics graphics) {
            SolidBrush brush = (isDown || isKeydown) ? ColorPalette.BUTTON_DOWN : ColorPalette.BUTTON_HOVER;
            bool isKeyboardModeHovering = cursor.X == x && cursor.Y == y;
            if (isKeyboardModeHovering || isSwapping || (isEditMode.Invoke() || icon != null) && (isHovered || isDown))
                graphics.FillRectangle(brush, location.X, location.Y, SIZE, SIZE);
            Bitmap bitmap = replaceIcon ?? icon;
            if (bitmap != null)
                graphics.DrawImage(bitmap, innerRect);
        }

        public override void run(bool isRightClick) {
            if (icon != null && !isEditMode.Invoke())
                Process.Start(executablePath);
            if (isEditMode.Invoke()) {
                if (isRightClick && icon != null) {
                    OptionWindow optionWindow = new OptionWindow(icon, replaceIcon, changeReplaceIcon, renderMainWindow);
                    optionWindow.ShowDialog();
                }
                if (!isRightClick) swapShortcut();
            }
        }

        private void changeReplaceIcon(Bitmap replaceIcon) {
            this.replaceIcon = replaceIcon;
            createReplaceIconsImage(true);
        }

        private void swapShortcut() {
            isSwapping = !isSwapping;
            Func<Shortcut, bool> isSwappingPredicate = shortcut => shortcut.isSwapping;
            if (shortcuts.Where(isSwappingPredicate).Count() >= 2) {
                List<Shortcut> list = shortcuts.Where(isSwappingPredicate).ToList();
                shortcuts.ForEach(shortcut => shortcut.isSwapping = false);
                if (list.Where(shortcut => shortcut.icon != null).Count() == 0)
                    return;
                Bitmap icon = list[0].icon;
                string path = list[0].executablePath;
                string shortcutPath = list[0].shortcutPath;
                list[0].icon = list[1].icon;
                list[0].executablePath = list[1].executablePath;
                list[0].shortcutPath = rename(list[1].shortcutPath, list[0].x, list[0].y);
                list[1].icon = icon;
                list[1].executablePath = path;
                list[1].shortcutPath = rename(shortcutPath, list[1].x, list[1].y);
                if (list[0].replaceIcon != null || list[1].replaceIcon != null) {
                    Bitmap replaceIcon = list[0].replaceIcon;
                    list[0].replaceIcon = list[1].replaceIcon;
                    list[1].replaceIcon = replaceIcon;
                    createReplaceIconsImage(true);
                }
            }
        }

        public static bool anyHovering() {
            return shortcuts.Any(shortcut => shortcut.isHovered && (shortcut.icon != null || isEditMode.Invoke()));
        }

        public static void focusLeftTop() {
            shortcuts
                .Where(shortcut => shortcut.icon != null)
                .Select(shortcut => (cursor.X = shortcut.x) + (cursor.Y = shortcut.y))
                .FirstOrDefault();
        }

        /// <summary>
        /// [direction] up: 0, right: 1, down: 2, left: 3
        /// </summary>
        public static void keyboardMove(int direction) {
            if (isKeydown)
                return;
            if (isEditMode.Invoke()) {
                Func<int, int, int> range = (i, max) => Math.Min(Math.Max(0, i), max - 1);
                if (direction % 2 == 0)
                    cursor.Y = range.Invoke(cursor.Y - 1 + direction, ROW);
                else
                    cursor.X = range.Invoke(cursor.X + 2 - direction, COL);
                return;
            }
            double directionAngle = direction * Math.PI / 2;
            Func<Shortcut, double> angleDiff = shortcut => {
                double relative = Math.Atan2(shortcut.x - cursor.X, -shortcut.y + cursor.Y);
                double temp = relative - directionAngle;
                return Math.Abs(Math.Atan2(Math.Sin(temp), Math.Cos(temp)));
            };
            List<Tuple<double, int, int>> sortedByNearestAngle = shortcuts
                .Where(shortcut => shortcut.icon != null)
                .Where(shortcut => !(shortcut.x == cursor.X && shortcut.y == cursor.Y))
                .Select(other => new Tuple<double, int, int>(angleDiff.Invoke(other), other.x, other.y))
                .Where(tuple => tuple.Item1 < Math.PI / 2)
                .OrderBy(tuple => tuple.Item1)
                .ToList();
            int[] nearest = sortedByNearestAngle
                .Where(sorted => sorted.Item1 == sortedByNearestAngle.First().Item1)
                .OrderBy(sorted => PointDouble.Subtract(new PointDouble(sorted.Item2, sorted.Item3), new PointDouble(cursor.X, cursor.Y)).Length)
                .Select(sorted => new int[] { sorted.Item2, sorted.Item3 })
                .FirstOrDefault();
            if (nearest != null)
                cursor = new PointInt(nearest[0], nearest[1]);
        }

        public static void spacebarPressed() {
            if (cursor.X != -1)
                shortcuts[cursor.X + cursor.Y * COL].run(false);
        }

        public static void clearSwapping() {
            shortcuts.ForEach(shortcut => shortcut.isSwapping = false);
        }

        private static Tuple<Bitmap, string> extractIcon(string linkPath) {
            string originPath = ((IWshShortcut) new WshShell().CreateShortcut(linkPath)).TargetPath;
            IntPtr[] phicon = new IntPtr[] { IntPtr.Zero };
            Func<string, Bitmap> extract = path => {
                PrivateExtractIcons(originPath = path, 0, 64, 64, phicon, new IntPtr[] { IntPtr.Zero }, 1, 0);
                return phicon[0] != IntPtr.Zero ? Icon.FromHandle(phicon[0]).ToBitmap() : null;
            };
            return new Tuple<Bitmap, string>(extract.Invoke(originPath) ?? extract.Invoke(originPath.Replace(" (x86)", "")), originPath);
        }

        private static string rename(string path, int x, int y) {
            if (path == null)
                return null;
            string filename = Path.GetFileName(path);
            if (REGEX.IsMatch(filename))
                path = removeLocationInfo(path);
            string newPath = Path.GetDirectoryName(path) + "\\" + String.Format("_[{0:D2},{1:D2}]{2}", x, y, Path.GetFileName(path));
            FileSystem.Rename(path, newPath);
            return newPath;
        }

        private static string removeLocationInfo(string path) {
            string filename = Path.GetFileName(path);
            if (path == null || !REGEX.IsMatch(filename))
                return path;
            string newPath = Path.GetDirectoryName(path) + "\\" + Strings.Right(filename, filename.Length - 8);
            FileSystem.Rename(path, newPath);
            return newPath;
        }

        private static void createReplaceIconsImage(bool render) {
            Bitmap bitmap = new Bitmap(IMAGE_SIZE * COL, IMAGE_SIZE * ROW, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.Bicubic;
            if (render) {
                shortcuts.ForEach(shortcut => {
                    if (shortcut.replaceIcon != null)
                        graphics.DrawImage(shortcut.replaceIcon, shortcut.x * IMAGE_SIZE, shortcut.y * IMAGE_SIZE);
                });
            }
            bitmap.Save(REPLACE_ICON_PATH);
        }

        private static void loadReplaceIcons() {
            string path = REPLACE_ICON_PATH;
            if (!File.Exists(path)) {
                createReplaceIconsImage(false);
                return;
            }
            using (Bitmap temp = new Bitmap(path)) {
                if (temp.Width != IMAGE_SIZE * COL || temp.Height != IMAGE_SIZE * ROW) {
                    createReplaceIconsImage(false);
                    return;
                }
            }
            using (Bitmap bitmap = new Bitmap(REPLACE_ICON_PATH)) {
                Action<int, int> checkEmpty = (x, y) => {
                    for (int yy = 0; yy < IMAGE_SIZE; yy++) {
                        for (int xx = 0; xx < IMAGE_SIZE; xx++) {
                            if (bitmap.GetPixel(x * IMAGE_SIZE + xx, y * IMAGE_SIZE + yy).A != 0) {
                                shortcuts[y * COL + x].replaceIcon = new Bitmap(bitmap).Clone(new Rectangle(x * IMAGE_SIZE, y * IMAGE_SIZE, IMAGE_SIZE, IMAGE_SIZE), PixelFormat.Format32bppArgb);
                                return;
                            }
                        }
                    }
                };
                for (int y = 0; y < ROW; y++)
                    for (int x = 0; x < COL; x++)
                        checkEmpty(x, y);
            }
        }

        public static void init(List<Clickable> list, Func<bool> isEditMode, Action turnOffKeyboardMode, Action renderMainWindow) {
            Directory.CreateDirectory(PATH);
            Shortcut.isEditMode = isEditMode;
            Shortcut.turnOffKeyboardMode = turnOffKeyboardMode;
            Shortcut.renderMainWindow = renderMainWindow;
            for (int y = 0; y < ROW; y++) 
                for (int x = 0; x < COL; x++) 
                    shortcuts.Add(new Shortcut(list, x, y));
            loadReplaceIcons();
            Func<int, int, Shortcut> get = (x, y) => shortcuts[y * COL + x];
            Func<string, bool> isLink = path => Strings.Right(path, 3) == "lnk";
            Func<string, bool, bool> isValidFilename = (path, mode) => REGEX.IsMatch(Path.GetFileName(path)) == mode;
            new List<string>(Directory.GetFiles(PATH))
                .Where(isLink)
                .Where(path => isValidFilename.Invoke(path, true))
                .Select(path => {
                    Match match = REGEX.Match(Path.GetFileName(path));
                    return new Tuple<string, int, int>(path, int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
                })
                .ToList()
                .ForEach(tuple => {
                    string path = tuple.Item1;
                    string filename = Path.GetFileName(path);
                    Shortcut shortcut = get.Invoke(tuple.Item2, tuple.Item3);
                    if (shortcut.icon == null) {
                        Tuple<Bitmap, string> shortcutInfo = extractIcon(path);
                        if (shortcutInfo.Item1 != null) {
                            shortcut.icon = shortcutInfo.Item1;
                            shortcut.executablePath = shortcutInfo.Item2;
                            shortcut.shortcutPath = path;
                        } else {
                            // could not extract icon. should be logged to somewhere.
                        }
                    } else {
                        removeLocationInfo(path);
                    }
                });
            new List<string>(Directory.GetFiles(PATH))
                .Where(isLink)
                .Where(path => isValidFilename.Invoke(path, false))
                .ToList()
                .ForEach(path => {
                    Shortcut empty = shortcuts.Find(shortcut => shortcut.icon == null);
                    if (empty == null)
                        return;
                    Tuple<Bitmap, string> shortcutInfo = extractIcon(path);
                    if (shortcutInfo.Item1 != null) {
                        empty.icon = shortcutInfo.Item1;
                        empty.executablePath = shortcutInfo.Item2;
                        empty.shortcutPath = rename(path, empty.x, empty.y);
                    } else {
                        // could not extract icon. should be logged to somewhere.
                    }
                });
        }

        public class ReplaceIcon : Clickable {
            private readonly Action action;

            public ReplaceIcon(List<Clickable> list, int x, int y, Action action) {
                location = new PointInt(x, y);
                GraphicsPath path = new GraphicsPath();
                path.AddRectangle(new Rectangle(location, new Size(SIZE, SIZE)));
                region = new Region(path);
                list.Add(this);
                this.action = action;
            }
            public override void render(Graphics graphics) {
                SolidBrush brush = (isDown || isKeydown) ? ColorPalette.BUTTON_DOWN : ColorPalette.BUTTON_HOVER;
                if (isHovered || isDown)
                    graphics.FillRectangle(brush, location.X, location.Y, SIZE, SIZE);
            }

            public override void run(bool isRightClick) {
                action.Invoke();
            }
        }
    }
}
