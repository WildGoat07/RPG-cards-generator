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
        public WidgetElement(IDrawableWidget widget)
        {
            InitializeComponent();
            var index = App.CurrentFile.Widgets.FindIndex((element) => element.Item2 == widget.Link);
            if (index == 0)
                upButton.IsEnabled = false;
            if (index == App.CurrentFile.Widgets.Count - 1)
                downButton.IsEnabled = false;
            id.Text = App.CurrentFile.Widgets[index].Item1;
        }
    }
}