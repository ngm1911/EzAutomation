using AutomationTool.Helper;
using AutomationTool.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlaUI.UIA3;
using HandyControl.Tools.Extension;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
                    if (Selected)
                    {
                        if (Children.All(x => x.Status == Constant.Passed)
                            && Steps.All(x => x.Status == Constant.Passed))
                        {
                            Status = Constant.Passed;
                        }
                        else
                        {
                            if (Children.Any(x => x.Status == Constant.Error)
                                || Steps.Any(x => x.Status == Constant.Error))
                            {
                                Status = Constant.Error;
                            }
                        }
                    }

                    if (ParentGuid != System.Guid.Empty.ToString())
                    {
                        App.Bus.Publish<FinishEnqueueTask>(new(ParentGuid));
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
        private bool isSelected;
        
        [ObservableProperty]
        [JsonIgnore]
        private bool isExpanded =  true;

        [ObservableProperty]
        [JsonIgnore]
        private bool isEditing;

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

                    Status = Constant.Running;
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
                Status = Constant.Error;
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), Constant.Error));
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
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), Constant.Error));
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
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), Constant.Error));
            }
        }

        [RelayCommand]
        private async Task CheckElement(AutoStep step)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                var process1 = Process.GetProcessesByName("EZConnect").First();
                if (process1.HasExited == false)
                {
                    using (var app = Application.Attach(process1))
                    using (var automation = new UIA3Automation())
                    {
                        var p = System.Windows.Forms.Cursor.Position;
                        var element = automation.FromPoint(p);
                        if (element != null)
                        {
                            var text = Constant.GetCachedPath(element);
                            var data = text.Split("<tab>").Select(x => x.Split("<t>"));
                            if (data.Any())
                            {
                                var name = data.FirstOrDefault()[1];
                                Clipboard.SetText(name);

                                step.ControlType = ControlTypes.Unknown;
                                var controlText = data.FirstOrDefault()[2];
                                if (Enum.TryParse(controlText, out ControlTypes controlType))
                                {
                                    step.ControlType = controlType;
                                    step.ActionType = step.ActionItemSource.FirstOrDefault();
                                }
                                step.Param0 = name;
                                step.CachedPath = text;
                                App.Bus.Publish<ShowMessage>(new($"Element '{name}' cached !", "Element Info"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), Constant.Error));
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
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), Constant.Error));
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
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), Constant.Error));
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

                Status = Constant.Running;
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
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), Constant.Error));
            }
        }

        partial void OnSelectedChanged(bool value)
        {
            Children.ForEach(x => x.Selected = value);
        }

        partial void OnIsSelectedChanged(bool value)
        {
            if (value == false)
            {
                IsEditing = false;
            }
        }
    }
}
