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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RPGcardsGenerator
{
    /// <summary>
    /// Logique d'interaction pour WidgetElement.xaml
    /// </summary>
    public partial class WidgetElement : UserControl
    {
        public IDrawableWidget Link;

        public WidgetElement(IDrawableWidget widget)
        {
            InitializeComponent();
            Link = widget;
            var index = App.CurrentFile.Widgets.FindIndex((element) => element.Item2 == widget.Link);
            if (index == 0)
                upButton.IsEnabled = false;
            if (index == App.CurrentFile.Widgets.Count - 1)
                downButton.IsEnabled = false;
            id.Text = App.CurrentFile.Widgets[index].Item1;
        }

        private void downButton_Click(object sender, RoutedEventArgs e)
        {
            {
                var position = App.CurrentFile.Widgets.FindIndex(set => set.Item2 == Link.Link);
                var curr = App.CurrentFile.Widgets[position];
                App.CurrentFile.Widgets.RemoveAt(position);
                App.CurrentFile.Widgets.Insert(position + 1, curr);
            }
            {
                var position = App.Preview.ToDraw.IndexOf(Link);
                App.Preview.ToDraw.RemoveAt(position);
                App.Preview.ToDraw.Insert(position + 1, Link);
            }
            {
                var panel = Parent as StackPanel;
                var location = panel.Children.IndexOf(this);
                panel.Children.Remove(this);
                panel.Children.Insert(location + 1, this);
                if (location == 0)
                {
                    upButton.IsEnabled = true;
                    (panel.Children[0] as WidgetElement).upButton.IsEnabled = false;
                }
                if (location == App.CurrentFile.Widgets.Count - 2)
                {
                    downButton.IsEnabled = false;
                    (panel.Children[App.CurrentFile.Widgets.Count - 2] as WidgetElement).downButton.IsEnabled = true;
                }
            }
        }

        private void upButton_Click(object sender, RoutedEventArgs e)
        {
            {
                var position = App.CurrentFile.Widgets.FindIndex(set => set.Item2 == Link.Link);
                var curr = App.CurrentFile.Widgets[position];
                App.CurrentFile.Widgets.RemoveAt(position);
                App.CurrentFile.Widgets.Insert(position - 1, curr);
            }
            {
                var position = App.Preview.ToDraw.IndexOf(Link);
                App.Preview.ToDraw.RemoveAt(position);
                App.Preview.ToDraw.Insert(position - 1, Link);
            }
            {
                var panel = Parent as StackPanel;
                var location = panel.Children.IndexOf(this);
                panel.Children.Remove(this);
                panel.Children.Insert(location - 1, this);
                if (location == 1)
                {
                    upButton.IsEnabled = false;
                    (panel.Children[1] as WidgetElement).upButton.IsEnabled = true;
                }
                if (location == App.CurrentFile.Widgets.Count - 1)
                {
                    downButton.IsEnabled = true;
                    (panel.Children[App.CurrentFile.Widgets.Count - 1] as WidgetElement).downButton.IsEnabled = false;
                }
            }
        }
    }
}