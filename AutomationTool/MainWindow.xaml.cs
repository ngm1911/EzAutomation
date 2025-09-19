using AutomationTool.DataSource;
using AutomationTool.Model;
using AutomationTool.ViewModel;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using HandyControl.Tools.Extension;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Application = FlaUI.Core.Application;

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
    }
}