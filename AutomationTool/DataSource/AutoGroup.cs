using AutomationTool.Helper;
using AutomationTool.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using HandyControl.Tools.Extension;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Application = FlaUI.Core.Application;

namespace AutomationTool.DataSource
{
    public partial class AutoGroup : ObservableObject
    {
        public AutoGroup()
        {
            App.Bus.Subscribe<FinishEnqueueTask>(m =>
            {
                if (m.guid == this.Guid || m.guid == System.Guid.Empty.ToString())
                {
                    if (Children.All(x => x.Status == "Passed")
                        && Steps.All(x => x.Status == "Passed"))
                    {
                        Status = "Passed";
                    }
                    else
                    {
                        if (Children.Any(x => x.Status == "Error")
                            || Steps.Any(x => x.Status == "Error"))
                        {
                            Status = "Error";
                        }
                    }
                }
            });
        }

        public AutoGroup Parent;

        [ObservableProperty]
        private string guid = System.Guid.NewGuid().ToString();

        [ObservableProperty]
        [JsonIgnore]
        private bool selected;

        [ObservableProperty]
        [JsonIgnore]
        private string status;

        [ObservableProperty]
        [JsonIgnore]
        private string error;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string parentGuid;

        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private ObservableCollection<AutoGroup> children = [];

        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private ObservableCollection<AutoStep> steps = [];

        [RelayCommand]
        private void AddItem()
        {
            try
            {
                var newItem = new AutoGroup
                {
                    Parent = this,
                    ParentGuid = this.Guid,
                };
                Children.Add(newItem);
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private void DeleteItem()
        {
            try
            {
                Parent.Children.Remove(this);
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }
        
        [RelayCommand]
        private async Task CopyItem()
        {
            try
            {
                await ViewModelSerializer.SaveObservableProps(this);
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }
        
        [RelayCommand]
        private async Task PasteItem()
        {
            try
            {
                await ViewModelSerializer.LoadObservableProps(this);
                this.Guid = System.Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }
        
        [RelayCommand]
        private void Run()
        {
            try
            {
                if (Selected)
                {
                    App.Bus.Publish<EnqueueTask>(new(this, async () =>
                    {
                        App.Bus.Publish<CloseAllTabs>(new());
                    }));

                    Status = "Running";
                    Steps.ForEach(x => x.Status = string.Empty);
                    foreach (var item in Steps)
                    {
                        App.Bus.Publish<EnqueueTask>(new(this, async () =>
                        {
                            await item.RunStepCommand.ExecuteAsync(null);
                        }));
                    }
                }

                Children.ForEach(x => x.Status = string.Empty);
                foreach (var child in Children)
                {
                    child.RunCommand.Execute(child);
                }
            }
            catch (Exception ex)
            {
                Status = "Error";
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private void AddStep()
        {
            try
            {
                Steps.Add(new AutoStep());
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private void RemoveStep(AutoStep step)
        {
            try
            {
                Steps.Remove(step);
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private void UpStep(AutoStep step)
        {
            try
            {
                var index = Steps.IndexOf(step);
                index--;
                if (index > -1)
                {
                    Steps.Remove(step);
                    Steps.Insert(index, step);
                }
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private void DownStep(AutoStep step)
        {
            try
            {
                var index = Steps.IndexOf(step);
                index++;
                if (index < Steps.Count)
                {
                    Steps.Remove(step);
                    Steps.Insert(index, step);
                }
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private void ExcuteStep(AutoStep step)
        {
            try
            {
                App.Bus.Publish<EnqueueTask>(new(this, async () =>
                {
                    App.Bus.Publish<CloseAllTabs>(new());
                }));

                Status = "Running";
                Steps.SkipWhile(x => x != step).ForEach(x => x.Status = string.Empty);
                foreach (var item in Steps.SkipWhile(x => x != step))
                {
                    App.Bus.Publish<EnqueueTask>(new(this, async () =>
                    {
                        await item.RunStepCommand.ExecuteAsync(null);
                    }));
                }
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        partial void OnSelectedChanged(bool value)
        {
            Children.ForEach(x => x.Selected = value);
        }
    }
}
