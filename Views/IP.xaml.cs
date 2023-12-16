using HACS.WPF.Views;
using System.Windows;
using System.Windows.Input;

namespace Cegs12X.Views
{
    /// <summary>
    /// Interaction logic for IP.xaml
    /// </summary>
    public partial class IP : View
    {
        public IP()
        {
            InitializeComponent();
        }

        // TODO try using this?
        //protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        //{
        //    base.OnMouseDoubleClick(e);
        //}

        void EditSample(HACS.WPF.ViewModels.InletPort ip)
        {
            if (ip == null) return;
            var w = new Window()
            { 
                Content = new SampleEditor(ip.Component),  
                SizeToContent = SizeToContent.WidthAndHeight 
            };
            w.Show();
        }

        private void InletPort_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
                EditSample((thisIP.Component as HACS.WPF.ViewModels.InletPort) ??
                    GetComponent(sender as UIElement) as HACS.WPF.ViewModels.InletPort);
        }
    }
}
