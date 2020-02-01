using System;
using System.Collections.Generic;
using System.Text;
using SFML;
using SFML.Graphics;
using SFML.System;
using System.Linq;

namespace RPGcardsGenerator
{
    public interface IDrawableWidget : Drawable
    {
        FloatRect Bounds { get; }
        bool drawOutline { get; set; }
        int Height { get; }

        void DrawWidget(RenderTarget target, RenderStates states);
    }

    public static partial class Utilities
    {
        public static Drawable ApplyTexture(Drawable toDraw, FloatRect drawableBounds, Texture ToApply, Color background) =>
            ApplyTexture(toDraw, (IntRect)drawableBounds, ToApply, background, BlendMode.Multiply);

        public static Drawable ApplyTexture(Drawable toDraw, FloatRect drawableBounds, Texture ToApply, Color background, BlendMode blendMode) =>
            ApplyTexture(toDraw, (IntRect)drawableBounds, ToApply, background, blendMode);

        public static Drawable ApplyTexture(Drawable toDraw, IntRect drawableBounds, Texture ToApply, Color background) =>
            ApplyTexture(toDraw, drawableBounds, ToApply, background, BlendMode.Multiply);

        public static Drawable ApplyTexture(Drawable toDraw, IntRect drawableBounds, Texture ToApply, Color background, BlendMode blendMode)
        {
            var states = RenderStates.Default;
            states.Transform.Translate(450, 450);
            var position = new Vector2f(drawableBounds.Left, drawableBounds.Top);
            var size = (Vector2f)drawableBounds.Size();
            var rect = new RectangleShape(new Vector2f(1500, 1500));
            App.VBO.Clear(background);
            App.VBO.Draw(toDraw, states);
            states.BlendMode = blendMode;
            if (ToApply != null)
                App.VBO.Draw(new RectangleShape(((FloatRect)drawableBounds).Size()) { Texture = ToApply, Position = position }, states);
            App.VBO.Display();
            rect.Texture = App.VBO.Texture;
            rect.Position = -new Vector2f(450, 450);
            return rect;
        }

        public static void DrawWidget(this IDrawableWidget widget, RenderTarget target) => widget.DrawWidget(target, RenderStates.Default);

        public static Color GetBorderColor(this Text t) => t.OutlineThickness == 0 ? t.FillColor : t.OutlineColor;

        public static Color MakeTransparent(this Color c) => new Color(c.R, c.G, c.B, 0);

        public static Vector2f Size(this FloatRect rect) => new Vector2f(rect.Width, rect.Height);

        public static Vector2i Size(this IntRect rect) => new Vector2i(rect.Width, rect.Height);
    }

    public class DrawableCounter : Transformable, IDrawableWidget
    {
        public DrawableCounter() : base()
        {
            drawOutline = false;
            Back = new List<Texture>();
            Icons = new List<Texture>();
            InternalText = new Text();
        }

        public IList<Texture> Back { get; set; }

        public FloatRect Bounds
        {
            get
            {
                if ((Style & Template.Counter.STACKED) != 0)
                {
                    var result = new FloatRect();
                    result.Width = InternalText.GetGlobalBounds().Width + 5 + Icons.First().Size.X;
                    result.Height = Height;
                    return result;
                }
                else
                {
                    if ((Style & Template.Counter.VERTICAL) == 0)
                    {
                        var result = new FloatRect();
                        result.Width = InternalText.GetGlobalBounds().Width + 5;
                        result.Height = Height;
                        if ((Style & (Template.Counter.ALT1 | Template.Counter.ALT2)) != 0)
                            result.Width += (Icons.First().Size.X + 4) * (Max / 2f + .5f);
                        else
                            result.Width += (Icons.First().Size.X + 4) * Max;
                        return result;
                    }
                    else
                    {
                        var result = new FloatRect();
                        result.Width = InternalText.GetGlobalBounds().Width + 5;
                        result.Height = Height;
                        if ((Style & (Template.Counter.ALT1 | Template.Counter.ALT2)) != 0)
                            result.Width += (Icons.First().Size.X + 4) * 1.5f;
                        else
                            result.Width += Icons.First().Size.X;
                        return result;
                    }
                }
            }
        }

        public bool drawOutline { get; set; }

        public int Height => (Style & Template.Counter.VERTICAL) == 0 ?
                            (int)Math.Max(InternalText.CharacterSize, (Style & (Template.Counter.ALT1 | Template.Counter.ALT2)) != 0 ? (Icons.First().Size.Y + 4) * 1.5f : Icons.First().Size.Y) :
            (int)Math.Max(InternalText.CharacterSize, (Style & (Template.Counter.ALT1 | Template.Counter.ALT2)) != 0 ? (Icons.First().Size.Y + 4) * (Max / 2f + .5f) : (Icons.First().Size.Y + 4) * Max);

        public IList<Texture> Icons { get; set; }
        public Text InternalText { get; set; }
        public int Max { get; set; }
        public int Style { get; set; }
        public Texture TextImage { get; set; }
        public int Value { get; set; }

        public void Draw(RenderTarget target, RenderStates states)
        {
            int Hspace = (int)(Icons.First().Size.Y * (float)Icons.First().Size.X / Icons.First().Size.Y + 4);
            int Vspace = (int)Icons.First().Size.Y + 4;
            InternalText.Origin = new Vector2f(0, InternalText.CharacterSize / 2);
            if ((Style & Template.Counter.VERTICAL) == 0)
                InternalText.Position = new Vector2f(0, (int)Math.Max(InternalText.CharacterSize, (Style & (Template.Counter.ALT1 | Template.Counter.ALT2)) != 0 ? Icons.First().Size.Y * 1.5f : Icons.First().Size.Y) / 2);
            else
                InternalText.Position = new Vector2f(0, (int)Math.Max(InternalText.CharacterSize, (Style & (Template.Counter.ALT1 | Template.Counter.ALT2)) != 0 ? Vspace * .5f * (Max - 1) : Vspace * (Max - 1)) / 2);
            states.Transform *= Transform;
            var initialStates = states;
            target.Draw(Utilities.ApplyTexture(InternalText, InternalText.GetGlobalBounds(), TextImage, InternalText.GetBorderColor().MakeTransparent()), states);
            var tr = Transform.Identity;
            tr.Translate(InternalText.GetGlobalBounds().Width + 5, 0);
            states.Transform *= tr;
            if ((Style & Template.Counter.STACKED) != 0)
            {
                Texture toDraw = null;
                var index = Math.Min(Value - 1, Icons.Count);
                if (index == 0 && Back.Count > 0)
                    toDraw = Back.First();
                else if (index > 0)
                    toDraw = Icons[index];
                var ratio = (float)toDraw.Size.X / toDraw.Size.Y;
                var rect = new Sprite(toDraw);
                target.Draw(rect, states);
            }
            else
            {
                void disp(Texture toDraw, Vector2f whereToDraw)
                {
                    target.Draw(new Sprite
                    {
                        Position = whereToDraw,
                        Texture = toDraw
                    }, states);
                }
                for (int i = 1; i <= Max; i++)
                {
                    Vector2f position;
                    if ((Style & Template.Counter.VERTICAL) != 0)
                    {
                        if ((Style & Template.Counter.ALT1) != 0)
                            position = new Vector2f(Hspace / 2 * ((i + 1) % 2), (i - 1) * Vspace / 2);
                        else if ((Style & Template.Counter.ALT2) != 0)
                            position = new Vector2f(Hspace / 2 * (i % 2), (i - 1) * Vspace / 2);
                        else
                            position = new Vector2f(0, (i - 1) * Vspace);
                        if ((Style & Template.Counter.RIGHT) != 0)
                            position.Y = (Max - 1) * Vspace * ((Style & (Template.Counter.ALT1 | Template.Counter.ALT2)) != 0 ? .5f : 1) - position.Y;
                    }
                    else
                    {
                        if ((Style & Template.Counter.ALT1) != 0)
                            position = new Vector2f((i - 1) * Hspace / 2, Vspace / 2 * ((i + 1) % 2));
                        else if ((Style & Template.Counter.ALT2) != 0)
                            position = new Vector2f((i - 1) * Hspace / 2, Vspace / 2 * (i % 2));
                        else
                            position = new Vector2f((i - 1) * Hspace, 0);
                        if ((Style & Template.Counter.RIGHT) != 0)
                            position.X = (Max - 1) * Hspace * ((Style & (Template.Counter.ALT1 | Template.Counter.ALT2)) != 0 ? .5f : 1) - position.X;
                    }
                    if (Value >= i)
                        disp(Icons[Math.Min(Icons.Count - 1, i - 1)], position);
                    else if (Back.Count > 0)
                        disp(Back[Math.Min(Back.Count - 1, i - 1)], position);
                }
            }
            if (drawOutline)
                target.Draw(new RectangleShape(new Vector2f(Bounds.Width, Bounds.Height)) { OutlineColor = Color.Green, OutlineThickness = -1, FillColor = Color.Transparent }, initialStates);
        }

        public void DrawWidget(RenderTarget target, RenderStates states) => Draw(target, states);
    }

    public class DrawableField : Text, IDrawableWidget
    {
        public DrawableField() : base()
        {
            drawOutline = false;
        }

        public FloatRect Bounds => new FloatRect(0, 0, GetGlobalBounds().Width, CharacterSize);
        public bool drawOutline { get; set; }
        public int Height => (int)CharacterSize;
        public Texture TextImage { get; set; }

        public void DrawWidget(RenderTarget target, RenderStates states)
        {
            target.Draw(Utilities.ApplyTexture(this, GetGlobalBounds(), TextImage, this.GetBorderColor().MakeTransparent()), states);
            if (drawOutline)
                target.Draw(new RectangleShape(new Vector2f(Bounds.Width, Bounds.Height)) { OutlineColor = Color.Green, OutlineThickness = -1, FillColor = Color.Transparent }, states);
        }
    }

    public class DrawableFieldList : Transformable, IDrawableWidget
    {
        public DrawableFieldList() : base()
        {
            ToDraw = new List<IDrawableWidget>();
            drawOutline = false;
        }

        public FloatRect Bounds
        {
            get
            {
                var result = new FloatRect();
                foreach (var item in ToDraw)
                {
                    var curr = item.Bounds;
                    if (curr.Width > result.Width)
                        result.Width = curr.Width;
                    result.Height += 5 + curr.Height;
                }
                return result;
            }
        }

        public bool drawOutline { get; set; }
        public int Height => throw new NotImplementedException();
        public List<IDrawableWidget> ToDraw { get; set; }
        private IDrawableWidget Template { get; set; }

        public void Draw(RenderTarget target, RenderStates states)
        {
            states.Transform *= Transform;
            var intialStates = states;
            foreach (var item in ToDraw)
            {
                item.DrawWidget(target, states);
                var tr = Transform.Identity;
                tr.Translate(0, item.Height + 5);
                states.Transform *= tr;
            }
            if (drawOutline)
                target.Draw(new RectangleShape(new Vector2f(Bounds.Width, Bounds.Height)) { OutlineColor = Color.Green, OutlineThickness = -1, FillColor = Color.Transparent }, intialStates);
        }

        public void DrawWidget(RenderTarget target, RenderStates states) => Draw(target, states);
    }

    public class DrawableGauge : Transformable, IDrawableWidget
    {
        public DrawableGauge()
        {
            drawOutline = false;
            InternalText = new Text();
        }

        public Texture Back { get; set; }
        public Texture Bar { get; set; }

        public FloatRect Bounds
        {
            get
            {
                var result = new FloatRect();
                result.Width = InternalText.GetGlobalBounds().Width;
                result.Height = Height;
                result.Width += 5 + Bar.Size.X;
                return result;
            }
        }

        public bool drawOutline { get; set; }
        public int Height => (int)Math.Max(InternalText.CharacterSize, (int)Bar.Size.Y);
        public Text InternalText { get; set; }
        public float Max { get; set; }
        public int Style { get; set; }
        public Texture TextImage { get; set; }
        public float Value { get; set; }

        public void Draw(RenderTarget target, RenderStates states)
        {
            states.Transform *= Transform;
            InternalText.Origin = new Vector2f(0, InternalText.CharacterSize / 2);
            InternalText.Position = new Vector2f(0, Bar.Size.Y / 2);
            target.Draw(Utilities.ApplyTexture(InternalText, InternalText.GetGlobalBounds(), TextImage, InternalText.GetBorderColor().MakeTransparent()), states);
            var tr = Transform.Identity;
            tr.Translate(InternalText.GetGlobalBounds().Width + 5, 0);
            states.Transform *= tr;
            target.Draw(new RectangleShape { Size = (Vector2f)Bar.Size, Texture = Back }, states);
            if ((Style & Template.Gauge.VERTICAL) == 0)
            {
                var rect = new RectangleShape(new Vector2f(Bar.Size.X * Value / Max, Bar.Size.Y));
                rect.Texture = Bar;
                if ((Style & Template.Gauge.LEFT) != 0)
                    rect.TextureRect = new IntRect(0, 0, (int)(Bar.Size.X * Value / Max), (int)Bar.Size.Y);
                else if ((Style & Template.Gauge.RIGHT) != 0)
                {
                    rect.TextureRect = new IntRect((int)(Bar.Size.X * (Max - Value) / Max), 0, (int)(Bar.Size.X * Value / Max), (int)Bar.Size.Y);
                    rect.Position = new Vector2f(Back.Size.X, 0);
                    rect.Origin = new Vector2f(rect.Size.X, 0);
                }
                else
                {
                    rect.TextureRect = new IntRect((int)(Bar.Size.X / 2 - Bar.Size.X * Value / Max / 2), 0, (int)(Bar.Size.X * Value / Max), (int)Bar.Size.Y);
                    rect.Position = new Vector2f(Back.Size.X / 2, 0);
                    rect.Origin = new Vector2f(rect.Size.X / 2, 0);
                }
                target.Draw(rect, states);
            }
            else
            {
                var rect = new RectangleShape(new Vector2f(Bar.Size.X, Bar.Size.Y * Value / Max));
                rect.Texture = Bar;
                if ((Style & Template.Gauge.LEFT) != 0)
                    rect.TextureRect = new IntRect(0, 0, (int)Bar.Size.X, (int)(Bar.Size.Y * Value / Max));
                else if ((Style & Template.Gauge.RIGHT) != 0)
                {
                    rect.TextureRect = new IntRect(0, (int)(Bar.Size.Y * (Max - Value) / Max), (int)Bar.Size.X, (int)(Bar.Size.Y * Value / Max));
                    rect.Position = new Vector2f(0, Back.Size.Y);
                    rect.Origin = new Vector2f(0, rect.Size.Y);
                }
                else
                {
                    rect.TextureRect = new IntRect(0, (int)(Bar.Size.Y / 2 - Bar.Size.Y * Value / Max / 2), (int)Bar.Size.X, (int)(Bar.Size.Y * Value / Max));
                    rect.Position = new Vector2f(0, Back.Size.Y / 2);
                    rect.Origin = new Vector2f(0, rect.Size.Y / 2);
                }
                target.Draw(rect, states);
                if (drawOutline)
                    target.Draw(new RectangleShape(new Vector2f(Bounds.Width, Bounds.Height)) { OutlineColor = Color.Green, OutlineThickness = -1, FillColor = Color.Transparent }, states);
            }
        }

        public void DrawWidget(RenderTarget target, RenderStates states) => Draw(target, states);
    }

    public class DrawableImage : RectangleShape, IDrawableWidget
    {
        public DrawableImage() : base()
        {
            drawOutline = false;
        }

        public FloatRect Bounds => GetGlobalBounds();

        public bool drawOutline { get; set; }

        public int Height => throw new NotImplementedException();

        public void DrawWidget(RenderTarget target, RenderStates states)
        {
            Draw(target, states);
            if (drawOutline)
                target.Draw(new RectangleShape(new Vector2f(Bounds.Width, Bounds.Height)) { OutlineColor = Color.Green, OutlineThickness = -1, FillColor = Color.Transparent }, states);
        }
    }

    public class DrawableStatGraph : Transformable, IDrawableWidget
    {
        public DrawableStatGraph() : base()
        {
            drawOutline = false;
            Statistics = new List<(Header, int)>();
        }

        public FloatRect Bounds => new FloatRect(new Vector2f(), Size);
        public int CharacterHeight { get; set; }
        public bool drawOutline { get; set; }
        public int Height => throw new NotImplementedException();
        public Color? HighGraphColor { get; set; }
        public Color InnerColor { get; set; }
        public Color LowGraphColor { get; set; }
        public int Max { get; set; }
        public Color OutsideColor { get; set; }
        public float OutsideThickness { get; set; }
        public Vector2f Size { get; set; }
        public List<(Header, int)> Statistics { get; set; }
        public Texture TextImage { get; set; }

        public void Draw(RenderTarget target, RenderStates states)
        {
            var highColor = HighGraphColor != null ? HighGraphColor.Value : LowGraphColor;
            states.Transform *= Transform;
            var lines = new List<Vertex>();
            var shape = new List<Vertex>() { new Vertex(Size / 2, LowGraphColor) };
            var icons = new List<Drawable>();
            float map(float value, float min, float max, float outMin, float outMax) => (value - min) * (outMax - outMin) / (max - min) + outMin;
            Color colorMap(float value, float min, float max, Color outMin, Color outMax) => new Color(
                (byte)map(value, min, max, outMin.R, outMax.R),
                (byte)map(value, min, max, outMin.G, outMax.G),
                (byte)map(value, min, max, outMin.B, outMax.B));
            int min = Max, max = 0;
            foreach (var item in Statistics)
            {
                if (item.Item2 > max)
                    max = item.Item2;
                if (item.Item2 < min)
                    min = item.Item2;
            }
            for (int i = 0; i < Statistics.Count; i++)
            {
                lines.Add(new Vertex(Size / 2, Color.Black));
                lines.Add(new Vertex(Size / 2 + new Vector2f((float)(Size.X / 2 * Math.Cos((double)i / Statistics.Count * 2 * Math.PI - Math.PI / 2)),
                                                              (float)(Size.Y / 2 * Math.Sin((double)i / Statistics.Count * 2 * Math.PI - Math.PI / 2))), Color.Black));
                lines.Add(lines.Last());
                lines.Add(new Vertex(Size / 2 + new Vector2f((float)(Size.X / 2 * Math.Cos((double)(i + 1) / Statistics.Count * 2 * Math.PI - Math.PI / 2)),
                                                              (float)(Size.Y / 2 * Math.Sin((double)(i + 1) / Statistics.Count * 2 * Math.PI - Math.PI / 2))), Color.Black));
                lines.Add(new Vertex(Size / 2 + new Vector2f((float)(map(Statistics[i].Item2, 0, Max, 0, Size.X / 2) * Math.Cos((double)i / Statistics.Count * 2 * Math.PI - Math.PI / 2)),
                                                     (float)(map(Statistics[i].Item2, 0, Max, 0, Size.Y / 2) * Math.Sin((double)i / Statistics.Count * 2 * Math.PI - Math.PI / 2))), Color.Black));
                lines.Add(new Vertex(Size / 2 + new Vector2f((float)(map(Statistics[(i + 1) % Statistics.Count].Item2, 0, Max, 0, Size.X / 2) * Math.Cos((double)(i + 1) / Statistics.Count * 2 * Math.PI - Math.PI / 2)),
                                                              (float)(map(Statistics[(i + 1) % Statistics.Count].Item2, 0, Max, 0, Size.Y / 2) * Math.Sin((double)(i + 1) / Statistics.Count * 2 * Math.PI - Math.PI / 2))), Color.Black));
                var position = new Vector2f((float)((Size.X / 2 + 5) * Math.Cos((double)i / Statistics.Count * 2 * Math.PI - Math.PI / 2)),
                                                              (float)((Size.Y / 2 + 5) * Math.Sin((double)i / Statistics.Count * 2 * Math.PI - Math.PI / 2)));
                position = (Vector2f)(Vector2i)position;
                if (Math.Abs(position.Y) < 1)
                {
                    if (Math.Abs(position.X) < 1)
                    {
                        var width = ((Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0) + Statistics[i].Item1.Text.GetGlobalBounds().Width) / 2;
                        if (Statistics[i].Item1.Image != null)
                            icons.Add(new Sprite(Statistics[i].Item1.Image) { Origin = new Vector2f(0, Statistics[i].Item1.Image.Size.Y / 2), Position = position + Size / 2 - new Vector2f(width, 0) });
                        Statistics[i].Item1.Text.Origin = new Vector2f(0, Statistics[i].Item1.Text.CharacterSize / 2);
                        Statistics[i].Item1.Text.Position = new Vector2f((Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0) - width, 0) + position + Size / 2;
                        target.Draw(Utilities.ApplyTexture(Statistics[i].Item1.Text, Statistics[i].Item1.Text.GetGlobalBounds(), TextImage, Statistics[i].Item1.Text.GetBorderColor().MakeTransparent()), states);
                    }
                    else if (position.X > -1)
                    {
                        if (Statistics[i].Item1.Image != null)
                            icons.Add(new Sprite(Statistics[i].Item1.Image) { Origin = new Vector2f(0, Statistics[i].Item1.Image.Size.Y / 2), Position = position + Size / 2 });
                        Statistics[i].Item1.Text.Origin = new Vector2f(0, Statistics[i].Item1.Text.CharacterSize / 2);
                        Statistics[i].Item1.Text.Position = new Vector2f((Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0), 0) + position + Size / 2;
                        target.Draw(Utilities.ApplyTexture(Statistics[i].Item1.Text, Statistics[i].Item1.Text.GetGlobalBounds(), TextImage, Statistics[i].Item1.Text.GetBorderColor().MakeTransparent()), states);
                    }
                    else
                    {
                        var width = (Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0) + Statistics[i].Item1.Text.GetGlobalBounds().Width;
                        if (Statistics[i].Item1.Image != null)
                            icons.Add(new Sprite(Statistics[i].Item1.Image) { Origin = new Vector2f(0, Statistics[i].Item1.Image.Size.Y / 2), Position = position + Size / 2 - new Vector2f(width, 0) });
                        Statistics[i].Item1.Text.Origin = new Vector2f(0, Statistics[i].Item1.Text.CharacterSize / 2);
                        Statistics[i].Item1.Text.Position = new Vector2f((Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0) - width, 0) + position + Size / 2;
                        target.Draw(Utilities.ApplyTexture(Statistics[i].Item1.Text, Statistics[i].Item1.Text.GetGlobalBounds(), TextImage, Statistics[i].Item1.Text.GetBorderColor().MakeTransparent()), states);
                    }
                }
                else if (position.Y < 0)
                {
                    if (Math.Abs(position.X) < 1)
                    {
                        var width = ((Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0) + Statistics[i].Item1.Text.GetGlobalBounds().Width) / 2;
                        if (Statistics[i].Item1.Image != null)
                            icons.Add(new Sprite(Statistics[i].Item1.Image) { Origin = new Vector2f(0, Statistics[i].Item1.Image.Size.Y), Position = position + Size / 2 - new Vector2f(width, 0) });
                        Statistics[i].Item1.Text.Origin = new Vector2f(0, Statistics[i].Item1.Text.CharacterSize);
                        Statistics[i].Item1.Text.Position = new Vector2f((Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0) - width, 0) + position + Size / 2;
                        target.Draw(Utilities.ApplyTexture(Statistics[i].Item1.Text, Statistics[i].Item1.Text.GetGlobalBounds(), TextImage, Statistics[i].Item1.Text.GetBorderColor().MakeTransparent()), states);
                    }
                    else if (position.X > -1)
                    {
                        if (Statistics[i].Item1.Image != null)
                            icons.Add(new Sprite(Statistics[i].Item1.Image) { Origin = new Vector2f(0, Statistics[i].Item1.Image.Size.Y), Position = position + Size / 2 });
                        Statistics[i].Item1.Text.Origin = new Vector2f(0, Statistics[i].Item1.Text.CharacterSize);
                        Statistics[i].Item1.Text.Position = new Vector2f((Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0), 0) + position + Size / 2;
                        target.Draw(Utilities.ApplyTexture(Statistics[i].Item1.Text, Statistics[i].Item1.Text.GetGlobalBounds(), TextImage, Statistics[i].Item1.Text.GetBorderColor().MakeTransparent()), states);
                    }
                    else
                    {
                        var width = (Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0) + Statistics[i].Item1.Text.GetGlobalBounds().Width;
                        if (Statistics[i].Item1.Image != null)
                            icons.Add(new Sprite(Statistics[i].Item1.Image) { Origin = new Vector2f(0, Statistics[i].Item1.Image.Size.Y), Position = position + Size / 2 - new Vector2f(width, 0) });
                        Statistics[i].Item1.Text.Origin = new Vector2f(0, Statistics[i].Item1.Text.CharacterSize);
                        Statistics[i].Item1.Text.Position = new Vector2f((Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0) - width, 0) + position + Size / 2;
                        target.Draw(Utilities.ApplyTexture(Statistics[i].Item1.Text, Statistics[i].Item1.Text.GetGlobalBounds(), TextImage, Statistics[i].Item1.Text.GetBorderColor().MakeTransparent()), states);
                    }
                }
                else
                {
                    if (Math.Abs(position.X) < 1)
                    {
                        var width = ((Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0) + Statistics[i].Item1.Text.GetGlobalBounds().Width) / 2;
                        if (Statistics[i].Item1.Image != null)
                            icons.Add(new Sprite(Statistics[i].Item1.Image) { Position = position - new Vector2f(width, 0) + Size / 2 });
                        Statistics[i].Item1.Text.Position = new Vector2f((Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0) - width, 0) + position + Size / 2;
                        target.Draw(Utilities.ApplyTexture(Statistics[i].Item1.Text, Statistics[i].Item1.Text.GetGlobalBounds(), TextImage, Statistics[i].Item1.Text.GetBorderColor().MakeTransparent()), states);
                    }
                    else if (position.X > -1)
                    {
                        if (Statistics[i].Item1.Image != null)
                            icons.Add(new Sprite(Statistics[i].Item1.Image) { Position = position + Size / 2 });
                        Statistics[i].Item1.Text.Position = new Vector2f((Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0), 0) + position + Size / 2;
                        target.Draw(Utilities.ApplyTexture(Statistics[i].Item1.Text, Statistics[i].Item1.Text.GetGlobalBounds(), TextImage, Statistics[i].Item1.Text.GetBorderColor().MakeTransparent()), states);
                    }
                    else
                    {
                        var width = (Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0) + Statistics[i].Item1.Text.GetGlobalBounds().Width;
                        if (Statistics[i].Item1.Image != null)
                            icons.Add(new Sprite(Statistics[i].Item1.Image) { Position = position - new Vector2f(width, 0) + Size / 2 });
                        Statistics[i].Item1.Text.Position = new Vector2f((Statistics[i].Item1.Image != null ? Statistics[i].Item1.Image.Size.X + 5 : 0) - width, 0) + position + Size / 2;
                        target.Draw(Utilities.ApplyTexture(Statistics[i].Item1.Text, Statistics[i].Item1.Text.GetGlobalBounds(), TextImage, Statistics[i].Item1.Text.GetBorderColor().MakeTransparent()), states);
                    }
                }
            }
            for (int i = 0; i <= Statistics.Count; i++)
            {
                shape.Add(new Vertex(Size / 2 + new Vector2f((float)(map(Statistics[i % Statistics.Count].Item2, 0, Max, 0, Size.X / 2) * Math.Cos((double)i / Statistics.Count * 2 * Math.PI - Math.PI / 2)),
                                                              (float)(map(Statistics[i % Statistics.Count].Item2, 0, Max, 0, Size.Y / 2) * Math.Sin((double)i / Statistics.Count * 2 * Math.PI - Math.PI / 2))),
                                                              colorMap(Statistics[i % Statistics.Count].Item2, min, max, LowGraphColor, highColor)));
            }
            target.Draw(shape.ToArray(), PrimitiveType.TriangleFan, states);
            target.Draw(lines.ToArray(), PrimitiveType.Lines, states);
            foreach (var item in icons)
                target.Draw(item, states);
            if (drawOutline)
                target.Draw(new RectangleShape(new Vector2f(Bounds.Width, Bounds.Height)) { OutlineColor = Color.Green, OutlineThickness = -1, FillColor = Color.Transparent }, states);
        }

        public void DrawWidget(RenderTarget target, RenderStates states) => Draw(target, states);

        public class Header
        {
            public Header()
            {
                Text = new Text();
            }

            public Texture Image { get; set; }
            public Text Text { get; set; }
        }
    }

    public class DrawableText : Text, IDrawableWidget
    {
        public DrawableText() : base()
        {
            drawOutline = false;
        }

        public FloatRect Bounds => new FloatRect(0, 0, GetGlobalBounds().Width, CharacterSize);
        public bool drawOutline { get; set; }
        public int Height => throw new NotImplementedException();
        public Texture TextImage { get; set; }

        public void DrawWidget(RenderTarget target, RenderStates states)
        {
            target.Draw(Utilities.ApplyTexture(this, GetGlobalBounds(), TextImage, this.GetBorderColor().MakeTransparent()), states);
            if (drawOutline)
                target.Draw(new RectangleShape(new Vector2f(Bounds.Width, Bounds.Height)) { OutlineColor = Color.Green, OutlineThickness = -1, FillColor = Color.Transparent }, states);
        }
    }
}