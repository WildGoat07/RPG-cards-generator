﻿using System;
using System.Collections.Generic;
using System.Text;
using SFML;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

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
        }

        public Drawable Background { get; set; }
        public IList<IDrawableWidget> ToDraw { get; set; }
        public RenderWindow Window { get; set; }

        public void Start()
        {
            while (Window.IsOpen)
            {
                Window.WaitAndDispatchEvents();
                Window.Clear(Color.White);

                if (Background != null)
                    Window.Draw(Background);
                foreach (var drawable in ToDraw)
                    drawable.DrawWidget(Window);

                Window.Display();
            }
        }
    }
}