using AutomationTool.DataSource;
using System.Windows;
using System.Windows.Controls;

namespace AutomationTool.Views
{
    public partial class MainContentView : UserControl
    {
        public TabControl Tabs => tabControl;

        public event EventHandler<AutoGroup>? CloseTabRequested;

        public event EventHandler? CloseAllTabsRequested;

        public MainContentView()
        {
            InitializeComponent();
        }

        private void btnCloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: AutoGroup tabVm })
            {
                CloseTabRequested?.Invoke(this, tabVm);
            }
        }

        private void btnCloseAllTab_Click(object sender, RoutedEventArgs e)
        {
            CloseAllTabsRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
