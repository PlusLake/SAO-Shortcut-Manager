using System.Drawing;

namespace sao_shortcut_manager.Clickables {
    public abstract class Clickable {
        public Point location = new Point();
        public Region region = new Region();

        public virtual bool isHovered { get; set; }
        public bool isDown;
        public int state;

        public abstract void render(Graphics graphics);

        public abstract void run(bool isRightClick);
    }
}
