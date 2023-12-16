using HACS.WPF.Views;
using System.Windows;

namespace Cegs12X.Views
{
    /// <summary>
    /// Interaction logic for GM.xaml
    /// </summary>
    public partial class IM : View
    {
        public IM()
        {
            InitializeComponent();
        }
        private void InletPort_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (View.GetComponent(sender as UIElement) is HACS.WPF.ViewModels.InletPort ip &&
                e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
                e.ClickCount == 2)
            {
                var w = new Window();
                var se = new SampleEditor(ip.Component);
                w.Content = se;
                w.SizeToContent = SizeToContent.WidthAndHeight;
                w.Show();
            }
        }
    }
}
