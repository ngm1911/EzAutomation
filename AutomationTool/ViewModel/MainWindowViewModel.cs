using AutomationTool.DataSource;
using AutomationTool.Helper;
using AutomationTool.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools.Extension;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;

namespace AutomationTool.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        CancellationTokenSource _tokenSource = new();
        TaskCompletionSource _pauseEvent;
        Queue<(AutoGroup autoGroup, Func<Task> task)> _queue = new();

        [ObservableProperty]
        private ObservableCollection<AutoGroup> autoTree = [];

        [ObservableProperty]
        [JsonIgnore]
        private bool processingQueue;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableFeature))]
        [JsonIgnore]
        private AutoGroup selectedGroup;

        public bool EnableFeature => SelectedGroup != null;

        public MainWindowViewModel()
        {
            Load();

            App.Bus.Subscribe<EnqueueTask>(m =>
            {
                Enqueue(m.autoGroup, m.task);
            });
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                await ViewModelSerializer.SaveObservableProps(this, $"AutoTree.json");
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private void Rename(AutoGroup step)
        {
            try
            {
                step.IsEditing = true;
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
                AutoTree.ForEach(x => x.RunCommand.Execute(null));
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private void Pause()
        {
            try
            {
                if (ProcessingQueue && _pauseEvent == null)
                {
                    _pauseEvent = new TaskCompletionSource();
                }
                else
                {
                    if (_pauseEvent != null)
                    {
                        _pauseEvent?.SetResult();
                        _pauseEvent = null;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private async Task Stop()
        {
            try
            {
                await _tokenSource.CancelAsync();
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private void UpStep(AutoGroup step)
        {
            try
            {
                var index = step.Parent.Children.IndexOf(step);
                index--;
                if (index > -1)
                {
                    step.Parent.Children.Remove(step);
                    step.Parent.Children.Insert(index, step);

                    SelectedGroup = step;
                    SelectedGroup.IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private void DownStep(AutoGroup step)
        {
            try
            {
                var index = step.Parent.Children.IndexOf(step);
                index++;
                if (index < step.Parent.Children.Count)
                {
                    step.Parent.Children.Remove(step);
                    step.Parent.Children.Insert(index, step);


                    SelectedGroup = step;
                    SelectedGroup.IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private void DeleteItem(AutoGroup step)
        {
            try
            {
                step.Parent.Children.Remove(step);
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private void AddItem(AutoGroup step)
        {
            try
            {
                var newItem = new AutoGroup
                {
                    Parent = step,
                    ParentGuid = step.Guid,
                };
                step.Children.Add(newItem);
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private async Task CopyItem(AutoGroup step)
        {
            try
            {
                await ViewModelSerializer.SaveObservableProps(step);
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private async Task PasteItem(AutoGroup step)
        {
            try
            {
                await ViewModelSerializer.LoadObservableProps(step);
                step.Guid = System.Guid.NewGuid().ToString();

                foreach (var item in AutoTree)
                {
                    UpdateParent(item);
                }
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        void UpdateParent(AutoGroup parent)
        {
            foreach (var child in parent.Children)
            {
                child.Parent = parent;
                UpdateParent(child);
            }
        }

        [RelayCommand]
        private async Task Load()
        {
            try
            {
                if (File.Exists("AutoTree.json"))
                {
                    await ViewModelSerializer.LoadObservableProps(this, $"AutoTree.json");

                    foreach (var item in AutoTree)
                    {
                        UpdateParent(item);
                    }
                }
                else
                {
                    AutoTree.Add(new AutoGroup() { Name = "Root" });
                }
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        private void Enqueue(AutoGroup autoGroup, Func<Task> action)
        {
            _queue.Enqueue((autoGroup, action));
            ProcessQueue();
        }

        private async Task ProcessQueue()
        {
            if (!ProcessingQueue)
            {
                ProcessingQueue = true;
                //await SaveCommand.ExecuteAsync(null);
                Constant.CachedMainWindow = null;

                while (_queue.TryDequeue(out var queueItem))
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    try
                    {
                        App.Bus.Publish<BeginEnqueueTask>(new(queueItem.autoGroup));

                        _tokenSource.Token.ThrowIfCancellationRequested();
                        await queueItem.task.Invoke();

                        if (_pauseEvent != null)
                            await _pauseEvent.Task;
                    }
                    catch (TaskCanceledException)
                    {
                        ProcessingQueue = false;
                        _queue.Clear();
                    }
                    catch (Exception ex)
                    {
                        queueItem.autoGroup.Status = "Error";
                        queueItem.autoGroup.Error = ex.Message;
                        ProcessingQueue = false;
                        _queue.Clear();

                        App.Bus.Publish<ShowMessage>(new(ex.Message, "Error"));
                    }
                    finally
                    {
                        App.Bus.Publish<FinishEnqueueTask>(new(queueItem.autoGroup.Guid));
                    }
                }

                App.Bus.Publish<FinishEnqueueTask>(new(Guid.Empty.ToString()));

                ProcessingQueue = false;
            }
        }
    }
}
