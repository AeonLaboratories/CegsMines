using AeonHacs.Wpf.Views;
using System.Windows;
using System.Windows.Input;

namespace CegsMines.Views
{
    /// <summary>
    /// Interaction logic for SerialTubeFurnace.xaml
    /// </summary>
    public partial class TubeFurnace : View
	{
		public TubeFurnace()
		{
			InitializeComponent();
		}
		private void InletPort_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (GetComponent(sender as UIElement) is AeonHacs.Wpf.ViewModels.InletPort ip &&
				e.LeftButton == MouseButtonState.Pressed &&
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
