using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RPGcardsGenerator
{
    /// <summary>
    /// Logique d'interaction pour NewWidget.xaml
    /// </summary>
    public partial class NewWidget : Window
    {
        private readonly float X;
        private readonly float Y;

        public NewWidget(SFML.System.Vector2f vec)
        {
            InitializeComponent();
            X = vec.X;
            Y = vec.Y;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (identifier.Text.Length > 0)
            {
                IDrawableWidget widget = null;
                if (fieldWidget.IsChecked.Value)
                {
                    var field = new DrawableField();
                    widget = field;
                    field.CharacterSize = 25;
                    field.DisplayedString = "<exemple>";
                    field.FillColor = SFML.Graphics.Color.Black;
                    field.OutlineColor = SFML.Graphics.Color.Black;
                    field.Font = App.Roboto;
                    field.Position = new SFML.System.Vector2f(X, Y);
                    field.Field = new Template.Field();
                    field.Field.Size = 25;
                    field.Field.InnerColor = System.Drawing.Color.Black;
                    field.Field.OutsideColor = System.Drawing.Color.Black;
                    field.Field.Content = "";
                    field.Field.Font = "Roboto";
                    field.Field.Location = new System.Numerics.Vector2(X, Y);
                }
                App.CurrentFile.Widgets.Insert(0, (identifier.Text, widget.Link));
                App.Preview.ToDraw.Insert(0, widget);
                EditorProperties.UpdateWidgetList();
                DialogResult = true;
            }
            else
            {
                identifierText.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 50, 50));
                identifier.BorderBrush = identifierText.Foreground;
            }
        }
    }
}