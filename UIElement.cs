using SFML.System;
using SFML.Window;
using SFML.Graphics;

namespace Quicksilver {
    public enum UIAlignment {
        TopLeft,
        TopCenter,
        TopRight,
        CenterLeft,
        Center,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    // I really like doing my own manual system design in case you couldn't tell.
    public abstract class UIElement {
        public FloatRect Bounds {get; set;}
        public UIAlignment Alignment {get; set;}
        protected FloatRect TrueBounds {get; private set;}

        public virtual void Run(RenderWindow window, Vector2f mousePos) {
            TrueBounds = Bounds;
            float width = window.GetView().Size.X;
            float height = window.GetView().Size.Y;
            if ((int)Alignment % 3 == 1) { TrueBounds = new FloatRect(TrueBounds.Left+width/2-TrueBounds.Width/2, TrueBounds.Top, TrueBounds.Width, TrueBounds.Height); }
            if ((int)Alignment % 3 == 2) { TrueBounds = new FloatRect(TrueBounds.Left+width-TrueBounds.Width, TrueBounds.Top, TrueBounds.Width, TrueBounds.Height); }
            if ((int)Alignment / 3 == 1) { TrueBounds = new FloatRect(TrueBounds.Left, TrueBounds.Top+height/2-TrueBounds.Height/2, TrueBounds.Width, TrueBounds.Height); }
            if ((int)Alignment / 3 == 2) { TrueBounds = new FloatRect(TrueBounds.Left, TrueBounds.Top+height-TrueBounds.Height, TrueBounds.Width, TrueBounds.Height); }
        }

        public virtual void OnClose() {}
        public virtual void OnResize(SizeEventArgs e) {}
        public virtual void OnMousePressed(Vector2f mousePos, MouseButtonEventArgs e) {}
        public virtual void OnMouseReleased(Vector2f mousePos, MouseButtonEventArgs e) {}
        public virtual void OnKeyPressed(KeyEventArgs e) {}
    }
}