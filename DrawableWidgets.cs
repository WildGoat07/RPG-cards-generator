using System;
using System.Collections.Generic;
using System.Text;
using SFML;
using SFML.Graphics;
using SFML.System;
using System.Linq;

namespace RPGcardsGenerator
{
    public class DrawableCounter : Transformable, Drawable
    {
        public DrawableCounter()
        {
            Back = new List<Texture>();
            Icons = new List<Texture>();
            InternalText = new Text();
        }

        public IList<Texture> Back { get; set; }
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

    public class DrawableField : Text
    {
    }

    public class DrawableImage : RectangleShape
    {
    }

    public class DrawableText : Text
    {
    }
}