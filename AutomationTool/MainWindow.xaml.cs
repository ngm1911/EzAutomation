using AutomationTool.DataSource;
using AutomationTool.Model;
using AutomationTool.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace AutomationTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private ObservableCollection<AutoGroup> _tabs = new();

        public MainWindow()
        {
            InitializeComponent();

            App.Bus.SubscribeUIThread<ShowMessage>(m =>
            {
                MessageBox.Show(this, m.Message, m.Title);
            });

            App.Bus.SubscribeUIThread<BeginEnqueueTask>(m =>
            {
                AddOrSelectTab(m.autoGroup);
            });

            App.Bus.SubscribeUIThread<CloseAllTabs>(m =>
            {
                while (_tabs.Count > 1)
                    _tabs.RemoveAt(0);
            });

            LeftSideBar.OpenTabRequested += (_, group) => AddOrSelectTab(group);
            MainContent.CloseTabRequested += (_, tabVm) => _tabs.Remove(tabVm);
            MainContent.CloseAllTabsRequested += (_, _) =>
            {
                while (_tabs.Any())
                    _tabs.RemoveAt(0);
            };

            MainContent.Tabs.ItemsSource = _tabs;
        }

        void AddOrSelectTab(AutoGroup viewModel)
        {
            var existing = _tabs.FirstOrDefault(t => t == viewModel);
            if (existing != null)
            {
                MainContent.Tabs.SelectedItem = existing;
            }
            else
            {
                _tabs.Add(viewModel);
                MainContent.Tabs.SelectedItem = viewModel;
            }
        }
    }
}
