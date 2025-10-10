using AutomationTool.DataSource;
using AutomationTool.Model;
using AutomationTool.ViewModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;

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

            tabControl.ItemsSource = _tabs;
        }

        private void TreeView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var treeView = sender as System.Windows.Controls.TreeView;
            if (treeView?.SelectedItem != null)
            {
                var data = treeView.SelectedItem as AutoGroup;
                AddOrSelectTab(data);
            }

            e.Handled = true;
        }

        void AddOrSelectTab(AutoGroup viewModel)
        {
            var existing = _tabs.FirstOrDefault(t => t == viewModel);
            if (existing != null)
            {
                tabControl.SelectedItem = existing;
            }
            else
            {
                _tabs.Add(viewModel);
                tabControl.SelectedItem = viewModel;
            }
        }

        private void btnCloseTab_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button?.DataContext is AutoGroup tabVm)
            {
                _tabs.Remove(tabVm);
            }
        }

        private void btnCloseAllTab_Click(object sender, RoutedEventArgs e)
        {
            while (_tabs.Any())
                _tabs.RemoveAt(0);
        }

        private void btnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            ExpandedAll(false);
        }

        private void btnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            ExpandedAll(true);
        }

        private void ExpandedAll(bool isExpanded)
        {
            try
            {
                ExpandedAll((DataContext as MainWindowViewModel).SelectedGroup);
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }

            void ExpandedAll(AutoGroup? parent)
            {
                if (parent != null)
                {
                    foreach (var item in parent.Children)
                    {
                        ExpandedAll(item);
                    }
                    parent.IsExpanded = isExpanded;
                }
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            (DataContext as MainWindowViewModel).SelectedGroup = e.NewValue as AutoGroup;
        }

        private void txtGroupName_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Escape)
            {
                (DataContext as MainWindowViewModel).SelectedGroup.IsEditing = false;
            }
        }
    }
}