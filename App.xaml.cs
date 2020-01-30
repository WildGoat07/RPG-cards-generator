using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using SFML.Graphics;
using SFML.System;

namespace RPGcardsGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Font Roboto = new Font(RPGcardsGenerator.Properties.Resources.Roboto_Regular);
        public static Template CurrentFile { get; set; }
        public static EditorPreview Preview { get; set; }

        public static Image CreateSFMLImage(System.Drawing.Bitmap img)
        {
            var result = new Image((uint)img.Width, (uint)img.Height);
            for (int x = 0; x < img.Width; x++)
                for (int y = 0; y < img.Height; y++)
                {
                    var c = img.GetPixel(x, y);
                    result.SetPixel((uint)x, (uint)y, new Color(c.R, c.G, c.B, c.A));
                }
            return result;
        }

        public static void StartEditor(Template file)
        {
            CurrentFile = file;
            Task.Run(() =>
            {
                Preview = new EditorPreview((float)file.Background.Width / file.Background.Height);
                Preview.Start();
            });
            while (Preview == null) System.Threading.Thread.Sleep(100);
            var list = new List<Drawable>();
            Preview.ToDraw = list;
            list.Add(new RectangleShape(new Vector2f(Preview.Window.Size.X, Preview.Window.Size.Y))
            {
                Texture = new Texture(CreateSFMLImage(file.Background)) { Smooth = true }
            });
            var dialog = new EditorProperties();
            dialog.ShowDialog();
        }
    }
}