using AutomationTool.DataSource;
using AutomationTool.Model;
using AutomationTool.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutomationTool.Views
{
    public partial class LeftSideBarView : UserControl
    {
        public event EventHandler<AutoGroup>? OpenTabRequested;

        public LeftSideBarView()
        {
            InitializeComponent();
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeView tv && tv.SelectedItem is AutoGroup data)
            {
                OpenTabRequested?.Invoke(this, data);
            }

            e.Handled = true;
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SelectedGroup = e.NewValue as AutoGroup;
            }
        }

        private void txtGroupName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter or Key.Escape
                && DataContext is MainWindowViewModel vm
                && vm.SelectedGroup is { } g)
            {
                g.IsEditing = false;
            }
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
                if (DataContext is MainWindowViewModel vm)
                {
                    ExpandedAll(vm.SelectedGroup);
                }
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }

            void ExpandedAll(AutoGroup? parent)
            {
                if (parent is null)
                {
                    return;
                }

                foreach (var item in parent.Children)
                {
                    ExpandedAll(item);
                }

                parent.IsExpanded = isExpanded;
            }
        }
    }
}
