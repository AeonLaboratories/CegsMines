using AeonHacs.Wpf.Views;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Cegs6x1.Views
{
    /// <summary>
    /// Interaction logic for MCVertical.xaml
    /// </summary>
    public partial class MC : View
    {
        public static readonly DependencyProperty Stable_ugCProperty = DependencyProperty.Register(
            nameof(Stable_ugC), typeof(bool), typeof(MC), new PropertyMetadata(false));

        public bool Stable_ugC { get => (bool)GetValue(Stable_ugCProperty); set => SetValue(Stable_ugCProperty, value); }

        public MC()
        {
            InitializeComponent();
        }

        protected override void OnComponentUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AeonHacs.Components.Meter.Value))
                Stable_ugC = (sender as AeonHacs.Wpf.ViewModels.Meter)?.IsStable ?? false;

            base.OnComponentUpdated(sender, e);
        }
    }
}
