using AeonHacs;
using AeonHacs.Components;
using AeonHacs.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cegs12X.Views
{
    /// <summary>
    /// Interaction logic for ControlPanel.xaml
    /// </summary>
    public partial class ControlPanel : AeonHacs.Wpf.Views.ControlPanel<AeonHacs.Components.Cegs12X>
    {
        ResourceDictionary Preferences = (ResourceDictionary)Application.Current.Resources["PreferencesDictionary"];

        HacsBase Hacs => Bridge?.GetHacs();

        // Empty constructor required for the designer to work.
        public ControlPanel()
        {
            InitializeComponent();
        }

        // Parameterized constructor called by the application on startup.
        public ControlPanel(Action closeAction) : base(closeAction)
        {
            InitializeComponent();
            PopulateProcessSelector();

            if (Hacs is Cegs cegs)
                cegs.SelectSamples += OpenSampleSelector;

            StartUpdateCycle();
        }

        protected virtual List<ISample> OpenSampleSelector(bool selectAll)
        {
            if (!CheckAccess())
                return Dispatcher.Invoke(() => OpenSampleSelector(selectAll));

            var w = new Window
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Title = "Select Samples",
                Content = new SampleSelector(selectAll),
                SizeToContent = SizeToContent.WidthAndHeight
            };
            w.ContentRendered += (sender, e) =>
            {
                w.ClearValue(Window.SizeToContentProperty);
                w.MinWidth = w.ActualWidth;
                w.MinHeight = w.ActualHeight;
            };
            w.ShowDialog();
            return (w.Content as SampleSelector).SelectedSamples;
        }

        void PopulateProcessSelector()
        {
            if (Hacs is ProcessManager pm)
            {
                var items = new ObservableCollection<object>();

                for (int i = 0; i < pm.ProcessNames.Count; ++i)
                    items.Add(pm.ProcessNames[i]);

                for (int i = pm.Separators.Count - 1; i >= 0; --i)
                    items.Insert(pm.Separators[i], new Separator());

                ProcessSelector.ItemsSource = items;
            }
        }

        void StartUpdateCycle()
        {
            // 100 millisecond Update() timer
            new DispatcherTimer(new TimeSpan(10000 * 100), DispatcherPriority.Background, (sender, e) => Update(), Dispatcher.CurrentDispatcher);
        }

        void UpdateContent(ContentControl control) =>
            BindingOperations.GetBindingExpression(control, ContentProperty)?.UpdateTarget();

        void Update()
        {
            UpdateContent(Uptime);
            UpdateContent(ProcessTime);
            UpdateContent(ProcessStepTime);
            UpdateContent(ProcessSubstepTime);
        }

        private void StartButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Hacs is ProcessManager pm && !pm.Busy && ProcessSelector.SelectedItem is string processName)
                Task.Run(() => pm.RunProcess(processName));
        }
    }
}
