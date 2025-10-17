using SFML.Window;
using SFML.Graphics;

namespace Quicksilver {
    public abstract class WindowScene {
        // Normally I would call this the Update function or something, but since it combines Update and Draw I'm just calling it Run.
        public abstract void Run(RenderWindow window);
        // Might as well make the rest of these virtual since some scenes won't need to use them.
        public virtual void OnClose() {}
        public virtual void OnResize(SizeEventArgs e) {}
        public virtual void OnMousePressed(MouseButtonEventArgs e) {}
        public virtual void OnMouseReleased(MouseButtonEventArgs e) {}
        public virtual void OnKeyPressed(KeyEventArgs e) {}
    }
}