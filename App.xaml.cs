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
using SFML.Window;

namespace RPGcardsGenerator
{
    public enum RequestType
    {
        /// <summary>
        /// Change to a text field
        /// </summary>
        FIELD_CHANGE,

        /// <summary>
        /// Change to a counting gauge
        /// </summary>
        COUNTER_CHANGE,

        /// <summary>
        /// Removed an item
        /// </summary>
        FIELD_LIST_REMOVE,

        /// <summary>
        /// Added an item
        /// </summary>
        FIELD_LIST_ADD,

        /// <summary>
        /// Edited an item
        /// </summary>
        FIELD_LIST_EDIT,

        /// <summary>
        /// Change to a classic gauge
        /// </summary>
        GAUGE_CHANGE,

        /// <summary>
        /// Change to one of the stats
        /// </summary>
        STATS_CHANGE
    }

    public static partial class Utilities
    {
        public static Vector2f Multiply(this Vector2f left, Vector2f right) => new Vector2f(left.X * right.X, left.Y * right.Y);

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
        public static IDrawableWidget SelectedEditingWidget { get; set; }
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
                result.IconsSize = graph.IconsSize.ToSFML();
                if (graph.TextImage != null)
                {
                    using var stream = new MemoryStream();
                    graph.TextImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    result.TextImage = new Texture(stream);
                }
                foreach (var stat in graph.Statistics)
                {
                    var header = (new DrawableStatGraph.Header(), stat.Item2);
                    header.Item1.Text = new Text
                    {
                        CharacterSize = (uint)result.CharacterHeight,
                        DisplayedString = stat.Item1.Text,
                        FillColor = result.InnerColor,
                        Font = Fonts[graph.Font],
                        OutlineColor = graph.OutsideColor.ToSFML(),
                        OutlineThickness = graph.OutsideThickness
                    };
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

        /// <summary>
        /// Update to a db.
        /// </summary>
        /// <param name="type">Type of widget changed.</param>
        /// <param name="identifier">Identifier of the widget changed</param>
        /// <param name="data">New data changed</param>
        public static void SendData(RequestType type, string identifier, object data)
        {
            switch (type)
            {
                case RequestType.FIELD_CHANGE:
                    {
                        string newText = data as string;
                        ///stuff
                    }
                    break;

                case RequestType.COUNTER_CHANGE:
                    {
                        float newValue = (data as float?).Value;
                        ///stuff
                    }
                    break;

                case RequestType.FIELD_LIST_REMOVE:
                    {
                        int indexToRemove = (data as int?).Value;
                        ///stuff
                    }
                    break;

                case RequestType.FIELD_LIST_ADD:
                    {
                        string newItemCaption = (data as (string, float)?).Value.Item1;
                        float newItemValue = (data as (string, float)?).Value.Item2;
                        ///stuff
                    }
                    break;

                case RequestType.FIELD_LIST_EDIT:
                    {
                        int index = (data as (int, string, float)?).Value.Item1;
                        string newItemCaption = (data as (int, string, float)?).Value.Item2;
                        float newItemValue = (data as (int, string, float)?).Value.Item3;
                        ///stuff
                    }
                    break;

                case RequestType.GAUGE_CHANGE:
                    {
                        float newValue = (data as float?).Value;
                        ///stuff
                    }
                    break;

                case RequestType.STATS_CHANGE:
                    {
                        int index = (data as (int, float)?).Value.Item1;
                        float newValue = (data as (int, float)?).Value.Item2;
                        ///stuff
                    }
                    break;
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
                Preview.Window.MouseButtonPressed += (sender, e) =>
                {
                    if (e.Button == Mouse.Button.Right)
                        App.Current.Dispatcher.Invoke(() => new NewWidget(Preview.Window.MapPixelToCoords(new Vector2i(e.X, e.Y))).ShowDialog());
                };
                Preview.Start();
            });
            while (Preview == null) System.Threading.Thread.Sleep(100);
            var list = new List<IDrawableWidget>();
            Preview.ToDraw = list;
            Preview.Background = new Texture(CreateSFMLImage(file.Background)) { Smooth = true };
            Fonts.Add("Roboto", Roboto);
            foreach (var font in file.CustomFonts)
                Fonts.Add(font.Key, new Font(font.Value));
            foreach (var widget in file.Widgets)
                Preview.ToDraw.Add(GetDrawable(widget.Item2, true));
            var dialog = new EditorProperties();
            Preview.Window.Closed += (sender, e) => dialog.Dispatcher.Invoke(() => dialog.Close());
            dialog.ShowDialog();
        }

        public static void UpdateWidgets()
        {
            //the key is the identifier of the widgets
            //the data given must match the one in the following ifs statements
            Dictionary<string, object> toUpdate = null;
            //initialize the dictionnary here, with a db request or anything
            foreach (var entry in toUpdate)
            {
                var widget = Preview.ToDraw.FirstOrDefault(dr => CurrentFile.Widgets.Find(d => d.Item2 == dr.Link).Item1 == entry.Key);
                if (widget is DrawableCounter counter)
                {
                    //here it must be float?
                    var newValues = entry.Value as float?;

                    counter.Value = (int)newValues.Value;
                    counter.Counter.Value = (int)newValues.Value;
                }
                else if (widget is DrawableField field)
                {
                    //here it must be string
                    var newValues = entry.Value as string;

                    field.DisplayedString = newValues;
                    field.Field.Content = newValues;
                }
                else if (widget is DrawableFieldList list)
                {
                    //here it must be List<(string, int, float)>
                    //a list of every element in the field list.
                    //the string is the text associated with the field
                    //the float is the value associated with the field (in case it is a gauge, or a counter)
                    var newValues = entry.Value as List<(string, float)>;

                    list.ToDraw.Clear();
                    list.FieldList.Data.Clear();
                    foreach (var value in newValues)
                    {
                        if (list.Template is DrawableGauge gaugeModel)
                        {
                            list.FieldList.Data.Add((value.Item1, 0, value.Item2));
                            list.ToDraw.Add(new DrawableGauge
                            {
                                Back = gaugeModel.Back,
                                Bar = gaugeModel.Bar,
                                InternalText = new Text(gaugeModel.InternalText)
                                {
                                    DisplayedString = value.Item1
                                },
                                Max = gaugeModel.Max,
                                Style = gaugeModel.Style,
                                TextImage = gaugeModel.TextImage,
                                Value = value.Item2
                            });
                        }
                        else if (list.Template is DrawableCounter counterModel)
                        {
                            list.FieldList.Data.Add((value.Item1, 0, value.Item2));
                            list.ToDraw.Add(new DrawableCounter
                            {
                                Back = counterModel.Back,
                                Icons = counterModel.Icons,
                                InternalText = new Text(counterModel.InternalText)
                                {
                                    DisplayedString = value.Item1
                                },
                                Max = counterModel.Max,
                                Style = counterModel.Style,
                                TextImage = counterModel.TextImage,
                                Value = (int)value.Item2
                            });
                        }
                        else if (list.Template is DrawableField fieldModel)
                        {
                            list.FieldList.Data.Add((value.Item1, 0, value.Item2));
                            list.ToDraw.Add(new DrawableField
                            {
                                CharacterSize = fieldModel.CharacterSize,
                                DisplayedString = value.Item1,
                                FillColor = fieldModel.FillColor,
                                Font = fieldModel.Font,
                                OutlineColor = fieldModel.OutlineColor,
                                OutlineThickness = fieldModel.OutlineThickness
                            });
                        }
                    }
                }
                else if (widget is DrawableGauge gauge)
                {
                    //here it must be float?
                    var newValues = entry.Value as float?;

                    gauge.Value = (int)newValues.Value;
                    gauge.Gauge.Value = (int)newValues.Value;
                }
                else if (widget is DrawableStatGraph graph)
                {
                    //here it must be List<int>
                    //a list of the all values of the graph
                    var newValues = entry.Value as List<int>;

                    for (int i = 0; i < newValues.Count; i++)
                        graph.Statistics[i] = (graph.Statistics[i].Item1, newValues[i]);
                }
            }
        }
    }
}