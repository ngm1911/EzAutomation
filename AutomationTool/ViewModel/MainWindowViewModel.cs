using AutomationTool.DataSource;
using AutomationTool.DataSource.Steps;
using AutomationTool.Helper;
using AutomationTool.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Windows.Forms;

namespace AutomationTool.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public string Title => $"Automation tool - File: {Path.GetFileName(currentPath)}";

        string currentPath = $"AutoTree.json";
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

        /// <summary>Path to the last automation summary XML written after a run (for Open log).</summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenLogCommand))]
        [JsonIgnore]
        private string? lastRunLogXmlPath;

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
                await ViewModelSerializer.SaveObservableProps(this, currentPath);
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }
        

        [RelayCommand]
        private async Task SaveAs()
        {
            try
            {
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Json files (*.json)|*.json|All files (*.*)|*.*",
                    FileName = "AutoTree.json"
                };
                if (dlg.ShowDialog() == true)
                {
                    currentPath = dlg.FileName;
                    OnPropertyChanged(nameof(Title));
                    await SaveCommand.ExecuteAsync(null);
                }
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
                _tokenSource = new();
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
                if (File.Exists(currentPath))
                {
                    await ViewModelSerializer.LoadObservableProps(this, currentPath);

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

        [RelayCommand(CanExecute = nameof(CanOpenLog))]
        private void OpenLog()
        {
            if (string.IsNullOrEmpty(LastRunLogXmlPath) || !File.Exists(LastRunLogXmlPath))
                return;

            var htmlPath = Path.ChangeExtension(LastRunLogXmlPath, ".html");
            var pathToOpen = File.Exists(htmlPath) ? htmlPath : LastRunLogXmlPath;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = pathToOpen,
                    UseShellExecute = true,
                });
            }
            catch
            {
                // Ignore if no default app
            }
        }

        private bool CanOpenLog() =>
            !string.IsNullOrEmpty(LastRunLogXmlPath) && File.Exists(LastRunLogXmlPath);

        [RelayCommand]
        private async Task OpenFile()
        {
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Json files (*.json)|*.json|All files (*.*)|*.*"
                };

                if (dlg.ShowDialog() == true)
                {
                    currentPath = dlg.FileName;
                    OnPropertyChanged(nameof(Title));
                    await LoadCommand.ExecuteAsync(null);
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

                {
                    Constant.VARIABLES_KEY = new Dictionary<string, string>();
                    string tempFile = Path.GetTempFileName();
                    await ViewModelSerializer.SaveObservableProps(AutoTree.FirstOrDefault(), tempFile);
                    var jRoot = JObject.Parse(await File.ReadAllTextAsync(tempFile));
                    var steps = jRoot.SelectTokens("Steps");
                    foreach(var child in steps.Children())
                    {
                        if (child.SelectToken("ActionType")?.Value<int>() == 22)
                        {
                            Constant.VARIABLES_KEY.Add(child.SelectToken("Param0")?.Value<string>(), child.SelectToken("Param1")?.Value<string>());
                        }
                    }
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }

                while (_queue.TryDequeue(out var queueItem))
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    try
                    {
                        App.Bus.Publish<BeginEnqueueTask>(new(queueItem.autoGroup));

                        if (_tokenSource.Token.IsCancellationRequested)
                        {
                            break;
                        }
                        await queueItem.task.Invoke();

                        if (_pauseEvent != null)
                            await _pauseEvent.Task;
                    }
                    catch (TaskCanceledException)
                    {
                        ProcessingQueue = false;
                        _queue.Clear();
                        break;
                    }
                    catch (Exception ex)
                    {
                        queueItem.autoGroup.Status = Constant.Error;
                        queueItem.autoGroup.Error = ex.Message;
                        //ProcessingQueue = false;
                        //_queue.Clear();

                        //App.Bus.Publish<ShowMessage>(new(ex.Message, "Error"));
                    }
                    finally
                    {
                        App.Bus.Publish<FinishEnqueueTask>(new(queueItem.autoGroup.Guid));
                    }
                }

                App.Bus.Publish<FinishEnqueueTask>(new(Guid.Empty.ToString()));

                var reportRows = EnumerateGroupTree(AutoTree)
                    .Where(g => g.Status == Constant.Passed || g.Status == Constant.Error)
                    .Select(g => new QueueRunReportEntry(
                        g.Name ?? string.Empty,
                        g.Status,
                        g.Error ?? string.Empty))
                    .ToList();
                LastRunLogXmlPath = WriteRunSummaryXmlAndOpen(reportRows);

                ProcessingQueue = false;
            }
        }

        private sealed record QueueRunReportEntry(string Title, string Status, string ErrorMessage);

        private static IEnumerable<AutoGroup> EnumerateGroupTree(IEnumerable<AutoGroup> roots)
        {
            foreach (var node in roots)
            {
                yield return node;
                foreach (var nested in EnumerateGroupTree(node.Children))
                    yield return nested;
            }
        }

        private static string WriteRunSummaryXmlAndOpen(IReadOnlyList<QueueRunReportEntry> entries)
        {
            var dir = Path.Combine(Path.GetTempPath(), "AutomationTool");
            Directory.CreateDirectory(dir);
            var baseName = $"AutomationRun_{DateTime.Now:yyyyMMdd_HHmmss}";
            var xmlPath = Path.Combine(dir, baseName + ".xml");
            var xslPath = Path.Combine(dir, baseName + ".xsl");
            var xslFileName = Path.GetFileName(xslPath);

            File.WriteAllText(xslPath, RunReportXslContent.Trim(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            var total = entries.Count;
            var success = entries.Count(e => e.Status == Constant.Passed);
            var fail = total - success;

            var cases = entries
                .Select((e, i) => new XElement(
                    "Case",
                    new XAttribute("number", i + 1),
                    new XAttribute("title", e.Title),
                    new XAttribute("status", e.Status),
                    new XAttribute("errorMessage", e.ErrorMessage ?? string.Empty)));

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XProcessingInstruction(
                    "xml-stylesheet",
                    $"type=\"text/xsl\" href=\"{xslFileName}\""),
                new XElement(
                    "Report",
                    new XAttribute("generated", DateTime.Now.ToString("o")),
                    new XElement(
                        "Total",
                        new XAttribute("run", total),
                        new XAttribute("success", success),
                        new XAttribute("fail", fail)),
                    cases));

            doc.Save(xmlPath, SaveOptions.None);

            var htmlPath = Path.Combine(dir, baseName + ".html");
            try
            {
                var xslt = new XslCompiledTransform();
                xslt.Load(xslPath);
                xslt.Transform(xmlPath, htmlPath);

                Process.Start(new ProcessStartInfo
                {
                    FileName = htmlPath,
                    UseShellExecute = true,
                });
            }
            catch
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = xmlPath,
                        UseShellExecute = true,
                    });
                }
                catch
                {
                    // Ignore if no default app
                }
            }

            return xmlPath;
        }

        private const string RunReportXslContent = """
            <?xml version="1.0" encoding="utf-8"?>
            <xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="html" indent="yes" encoding="utf-8"/>
              <xsl:template match="/Report">
                <html xmlns="http://www.w3.org/1999/xhtml">
                  <head>
                    <meta charset="utf-8"/>
                    <title>Automation run summary</title>
                    <style type="text/css">
                      body { font-family: Segoe UI, sans-serif; margin: 16px; }
                      h1 { font-size: 1.25rem; }
                      table { border-collapse: collapse; width: 100%; max-width: 1200px; }
                      th, td { border: 1px solid #ccc; padding: 6px 10px; text-align: left; vertical-align: top; }
                      th { background: #f0f0f0; }
                      tr.status-error td.status { color: #c00; font-weight: bold; }
                      tr.status-cancelled td.status { color: #a60; }
                      .summary { margin: 12px 0 20px; font-size: 1rem; }
                    </style>
                  </head>
                  <body>
                    <h1>Automation run summary</h1>
                    <p class="summary">
                      Total — run: <xsl:value-of select="Total/@run"/>,
                      success: <xsl:value-of select="Total/@success"/>,
                      fail: <xsl:value-of select="Total/@fail"/>
                    </p>
                    <table>
                      <thead>
                        <tr>
                          <th>#</th>
                          <th>Title</th>
                          <th>Status</th>
                          <th>Error message</th>
                        </tr>
                      </thead>
                      <tbody>
                        <xsl:for-each select="Case">
                          <xsl:element name="tr">
                            <xsl:attribute name="class">
                              <xsl:choose>
                                <xsl:when test="@status = 'Error'">status-error</xsl:when>
                                <xsl:when test="@status = 'Cancelled'">status-cancelled</xsl:when>
                                <xsl:otherwise></xsl:otherwise>
                              </xsl:choose>
                            </xsl:attribute>
                            <td><xsl:value-of select="@number"/></td>
                            <td><xsl:value-of select="@title"/></td>
                            <td class="status"><xsl:value-of select="@status"/></td>
                            <td><xsl:value-of select="@errorMessage"/></td>
                          </xsl:element>
                        </xsl:for-each>
                      </tbody>
                    </table>
                  </body>
                </html>
              </xsl:template>
            </xsl:stylesheet>
            """;
    }
}
