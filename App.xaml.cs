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
    public static partial class Utilities
    {
        public static Color ToSFML(this System.Drawing.Color c) => new Color(c.R, c.G, c.B, c.A);

        public static Vector2f ToSFML(this System.Numerics.Vector2 v) => new Vector2f(v.X, v.Y);
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Random Random = new Random();
        public static Font Roboto = new Font(RPGcardsGenerator.Properties.Resources.Roboto_Regular);
        public static Template CurrentFile { get; set; }
        public static Dictionary<string, Font> Fonts { get; set; }
        public static EditorPreview Preview { get; set; }
        public static RenderTexture VBO { get; set; }

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

        public static IDrawableWidget GetDrawable(Template.IWidget widget, bool preview = false)
        {
            if (widget is Template.Counter counter)
            {
                var result = new DrawableCounter();
                result.Counter = counter;
                foreach (var img in counter.Back)
                {
                    using var stream = new MemoryStream();
                    img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    result.Back.Add(new Texture(stream));
                }
                foreach (var img in counter.Icons)
                {
                    using var stream = new MemoryStream();
                    img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    result.Icons.Add(new Texture(stream));
                }
                result.InternalText.Font = Fonts[counter.Font];
                result.InternalText.DisplayedString = counter.Content;
                result.Value = preview ? (int)(Random.NextDouble() * counter.Max) : counter.Value;
                result.Max = counter.Max;
                result.Style = counter.Style;
                result.InternalText.FillColor = counter.InnerColor.ToSFML();
                result.Position = counter.Location.ToSFML();
                result.InternalText.OutlineColor = counter.OutsideColor.ToSFML();
                result.InternalText.OutlineThickness = counter.OutsideThickness;
                result.InternalText.CharacterSize = (uint)counter.Size;
                if (counter.TextImage != null)
                {
                    using var stream = new MemoryStream();
                    counter.TextImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    result.TextImage = new Texture(stream);
                }
                return result;
            }
            else if (widget is Template.Gauge gauge)
            {
                var result = new DrawableGauge();
                result.Gauge = gauge;
                {
                    using var stream = new MemoryStream();
                    gauge.Bar.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    result.Bar = new Texture(stream);
                }
                {
                    using var stream = new MemoryStream();
                    gauge.Back.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    result.Back = new Texture(stream);
                }
                result.InternalText.Font = Fonts[gauge.Font];
                result.InternalText.DisplayedString = gauge.Content;
                result.Value = preview ? (float)Random.NextDouble() * gauge.Max : gauge.Value;
                result.Max = gauge.Max;
                result.Style = gauge.Style;
                result.InternalText.FillColor = gauge.InnerColor.ToSFML();
                result.Position = gauge.Location.ToSFML();
                result.InternalText.OutlineColor = gauge.OutsideColor.ToSFML();
                result.InternalText.OutlineThickness = gauge.OutsideThickness;
                result.InternalText.CharacterSize = (uint)gauge.Size;
                if (gauge.TextImage != null)
                {
                    using var stream = new MemoryStream();
                    gauge.TextImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    result.TextImage = new Texture(stream);
                }
                return result;
            }
            else if (widget is Template.Field field)
            {
                var result = new DrawableField();
                result.Field = field;
                result.Font = Fonts[field.Font];
                result.DisplayedString = preview ? "<exemple>" : field.Content;
                result.FillColor = field.InnerColor.ToSFML();
                result.Position = field.Location.ToSFML();
                result.OutlineColor = field.OutsideColor.ToSFML();
                result.OutlineThickness = field.OutsideThickness;
                result.CharacterSize = (uint)field.Size;
                if (field.TextImage != null)
                {
                    using var stream = new MemoryStream();
                    field.TextImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    result.TextImage = new Texture(stream);
                }
                return result;
            }
            else if (widget is Template.Text text)
            {
                var result = new DrawableText();
                result.Text = text;
                result.Font = Fonts[text.Font];
                result.DisplayedString = text.Content;
                result.FillColor = text.InnerColor.ToSFML();
                result.Position = text.Location.ToSFML();
                result.OutlineColor = text.OutsideColor.ToSFML();
                result.OutlineThickness = text.OutsideThickness;
                result.CharacterSize = (uint)text.Size;
                if (text.TextImage != null)
                {
                    using var stream = new MemoryStream();
                    text.TextImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    result.TextImage = new Texture(stream);
                }
                return result;
            }
            else if (widget is Template.FieldList fieldList)
            {
                var result = new DrawableFieldList();
                result.FieldList = fieldList;
                result.Position = fieldList.Location.ToSFML();
                foreach (var subWidget in fieldList.Data)
                {
                    var drawableSubWidget = GetDrawable(fieldList.Model, preview);
                    result.ToDraw.Add(drawableSubWidget);
                    if (drawableSubWidget is DrawableCounter subCounter)
                    {
                        subCounter.InternalText.DisplayedString = subWidget.Item1;
                        subCounter.Value = subWidget.Item2;
                    }
                    else if (drawableSubWidget is DrawableGauge subGauge)
                    {
                        subGauge.InternalText.DisplayedString = subWidget.Item1;
                        subGauge.Value = subWidget.Item3;
                    }
                    else if (drawableSubWidget is DrawableField subField)
                        subField.DisplayedString = subWidget.Item1;
                }
                return result;
            }
            else if (widget is Template.Image image)
            {
                var result = new DrawableImage();
                result.Image = image;
                result.Position = image.Location.ToSFML();
                {
                    using var stream = new MemoryStream();
                    image.Data.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    result.Texture = new Texture(stream);
                }
                return result;
            }
            else
            {
                var graph = widget as Template.StatGraph;
                var result = new DrawableStatGraph();
                result.StatGraph = graph;
                result.Position = graph.Location.ToSFML();
                result.CharacterHeight = graph.CharacterHeight;
                result.HighGraphColor = graph.HighGraphColor.HasValue ? graph.HighGraphColor.Value.ToSFML() : null as Color?;
                result.LowGraphColor = graph.LowGraphColor.ToSFML();
                result.InnerColor = graph.InnerColor.ToSFML();
                result.Max = graph.Max;
                result.OutsideColor = graph.OutsideColor.ToSFML();
                result.OutsideThickness = graph.OutsideThickness;
                result.Size = graph.Size.ToSFML();
                {
                    using var stream = new MemoryStream();
                    graph.TextImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    result.TextImage = new Texture(stream);
                }
                foreach (var stat in graph.Statistics)
                {
                    var header = (new DrawableStatGraph.Header(), stat.Item2);
                    result.Statistics.Add(header);
                    if (stat.Item1.Image != null)
                    {
                        using var stream = new MemoryStream();
                        stat.Item1.Image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        stream.Seek(0, SeekOrigin.Begin);
                        header.Item1.Image = new Texture(stream);
                    }
                }
                return result;
            }
        }

        public static void StartEditor(Template file)
        {
            Fonts = new Dictionary<string, Font>();
            VBO = new RenderTexture(1500, 1500);
            CurrentFile = file;
            Task.Run(() =>
            {
                Preview = new EditorPreview((float)file.Background.Width / file.Background.Height);
                Preview.Start();
            });
            while (Preview == null) System.Threading.Thread.Sleep(100);
            var list = new List<IDrawableWidget>();
            Preview.ToDraw = list;
            /*Preview.Background = new RectangleShape(new Vector2f(Preview.Window.Size.X, Preview.Window.Size.Y))
            {
                Texture = new Texture(CreateSFMLImage(file.Background)) { Smooth = true }
            };*/
            Fonts.Add("Roboto", Roboto);
            foreach (var font in file.CustomFonts)
                Fonts.Add(font.Key, new Font(font.Value));
            foreach (var widget in file.Widgets)
                Preview.ToDraw.Add(GetDrawable(widget.Item2, true));
            var dialog = new EditorProperties();
            Preview.Window.Closed += (sender, e) => dialog.Dispatcher.Invoke(() => dialog.Close());
            dialog.ShowDialog();
        }
    }
}