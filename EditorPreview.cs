using System;
using System.Collections.Generic;
using System.Text;
using SFML;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Linq;

namespace RPGcardsGenerator
{
    public class EditorPreview
    {
        public EditorPreview(float ratio)
        {
            var desktop = VideoMode.DesktopMode;
            VideoMode mode;
            if (ratio > 1)
                mode = new VideoMode(desktop.Width / 2, (uint)(desktop.Width / 2 / ratio));
            else
                mode = new VideoMode((uint)(desktop.Height / 1.5f * ratio), (uint)(desktop.Height / 1.5f));
            Window = new RenderWindow(mode, "aperçu", Styles.Close, new ContextSettings { AntialiasingLevel = 8 });
            Window.SetVerticalSyncEnabled(true);
            ToDraw = new IDrawableWidget[0];
            Window.SetView(new View(new FloatRect(0, 0, 1000 * ratio, 1000)));
        }

        public Texture Background { get; set; }
        public IList<IDrawableWidget> ToDraw { get; set; }
        public RenderWindow Window { get; set; }

        public void Start()
        {
            while (Window.IsOpen)
            {
                Window.DispatchEvents();
                Window.Clear(Color.White);

                if (Background != null)
                    Window.Draw(new RectangleShape(new Vector2f(1, 1).Multiply(Window.GetView().Size)) { Texture = Background });
                foreach (var drawable in ToDraw.Reverse())
                    drawable.DrawWidget(Window);

                Window.Display();
            }
        }
    }
}