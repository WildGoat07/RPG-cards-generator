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
        int Height { get; }
    }

    public class DrawableCounter : Transformable, IDrawableWidget
    {
        public DrawableCounter()
        {
            Back = new List<Texture>();
            Icons = new List<Texture>();
            InternalText = new Text();
        }

        public IList<Texture> Back { get; set; }

        public int Height => (Style & Template.Counter.VERTICAL) == 0 ?
            (int)Math.Max(InternalText.CharacterSize, (Style & (Template.Counter.ALT1 | Template.Counter.ALT2)) != 0 ? IconHeight * 1.5f : IconHeight) :
            (int)Math.Max(InternalText.CharacterSize, (Style & (Template.Counter.ALT1 | Template.Counter.ALT2)) != 0 ? (IconHeight + 4) * .5f * (Max - 1) : (IconHeight + 4) * (Max - 1));

        public int IconHeight { get; set; }
        public IList<Texture> Icons { get; set; }
        public Text InternalText { get; set; }
        public int Max { get; set; }
        public int Style { get; set; }
        public int Value { get; set; }

        public void Draw(RenderTarget target, RenderStates states)
        {
            int Hspace = (int)(IconHeight * (float)Icons.First().Size.X / Icons.First().Size.Y + 4);
            int Vspace = IconHeight + 4;
            InternalText.Origin = new Vector2f(0, InternalText.CharacterSize / 2);
            if ((Style & Template.Counter.VERTICAL) == 0)
                InternalText.Position = new Vector2f(0, (int)Math.Max(InternalText.CharacterSize, (Style & (Template.Counter.ALT1 | Template.Counter.ALT2)) != 0 ? IconHeight * 1.5f : IconHeight) / 2);
            else
                InternalText.Position = new Vector2f(0, (int)Math.Max(InternalText.CharacterSize, (Style & (Template.Counter.ALT1 | Template.Counter.ALT2)) != 0 ? Vspace * .5f * (Max - 1) : Vspace * (Max - 1)) / 2);
            states.Transform *= Transform;
            target.Draw(InternalText, states);
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
                var rect = new RectangleShape(new Vector2f(IconHeight * ratio, IconHeight)) { Texture = toDraw };
                target.Draw(rect, states);
            }
            else
            {
                void disp(Texture toDraw, Vector2f whereToDraw)
                {
                    target.Draw(new RectangleShape
                    {
                        Size = new Vector2f(IconHeight * (float)toDraw.Size.X / toDraw.Size.Y, IconHeight),
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
        }
    }

    public class DrawableField : Text, IDrawableWidget
    {
        public int Height => (int)CharacterSize;
    }

    public class DrawableFieldList : Transformable, IDrawableWidget
    {
        public int Height => throw new NotImplementedException();

        private List<IDrawableWidget> ToDraw { get; set; }

        public void Draw(RenderTarget target, RenderStates states)
        {
            states.Transform *= Transform;
            foreach (var item in ToDraw)
            {
                target.Draw(item, states);
                var tr = Transform.Identity;
                tr.Translate(0, item.Height + 5);
                states.Transform *= tr;
            }
        }
    }

    public class DrawableGauge : Transformable, IDrawableWidget
    {
        public DrawableGauge()
        {
            InternalText = new Text();
        }

        public Texture Back { get; set; }

        public Texture Bar { get; set; }

        public int Height => (int)Math.Max(InternalText.CharacterSize, (int)Bar.Size.Y);
        public Text InternalText { get; set; }

        public float Max { get; set; }

        public int Style { get; set; }

        public float Value { get; set; }

        public void Draw(RenderTarget target, RenderStates states)
        {
            states.Transform *= Transform;
            InternalText.Origin = new Vector2f(0, InternalText.CharacterSize / 2);
            InternalText.Position = new Vector2f(0, Bar.Size.Y / 2);
            target.Draw(InternalText, states);
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
            }
        }
    }

    public class DrawableImage : RectangleShape, IDrawableWidget
    {
        public int Height => throw new NotImplementedException();
    }

    public class DrawableStatGraph : Transformable, IDrawableWidget
    {
        public int Height => throw new NotImplementedException();

        public void Draw(RenderTarget target, RenderStates states)
        {
        }
    }

    public class DrawableText : Text, IDrawableWidget
    {
        public int Height => throw new NotImplementedException();
    }
}