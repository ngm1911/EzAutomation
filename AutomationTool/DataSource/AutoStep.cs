using AutomationTool.DataSource.Steps;
using AutomationTool.Helper;
using AutomationTool.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace AutomationTool.DataSource
{
    public partial class AutoStep : ObservableObject
    {
        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string name;

        [ObservableProperty]
        [JsonIgnore]
        private string status;
        
        [ObservableProperty]
        [JsonIgnore]
        private string error;

        [ObservableProperty]
        private StepType stepType;

        [ObservableProperty]
        private string cachedPath;

        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string param0;

        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string param1;

        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string param2;

        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string param3;

        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string param4;

        [ObservableProperty]
        private bool skipError;

        public ObservableCollection<StepType> StepTypes { get; } = [.. Enum.GetValues(typeof(StepType)).Cast<StepType>()];

        private IStep? GetBaseStep()
        {
            switch (StepType)
            {
                case StepType.FillTextBox:
                    return new TextBoxStep(this);

                case StepType.ClickButton:
                case StepType.OpenDialog:
                case StepType.ButtonText:
                    return new ButtonStep(this);

                case StepType.ClickSplitButton:
                    return new SplitButton(this);

                case StepType.SelectTab:
                case StepType.CloseTab:
                    return new TabControlStep(this);
                    
                case StepType.SelectDropdown:
                    return new DropDownStep(this);

                case StepType.DataItem:
                    return new DataItemStep(this);

                case StepType.CheckBox:
                    return new CheckBoxStep(this);

                default:
                    return null;
            }
        }

        [RelayCommand]
        private void ClearCache()
        {
            CachedPath = string.Empty;
        }

        [RelayCommand]
        private async void CheckElement()
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
                            if (data.All(x => x.Length == 4))
                            {
                                var name = data.FirstOrDefault()[1];
                                System.Windows.Clipboard.SetText(name);
                                CachedPath = text;
                                App.Bus.Publish<ShowMessage>(new($"Element '{name}' cached !", "Element Info"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
            }
        }

        [RelayCommand]
        private async Task<bool> RunStep()
        {
            Status = "Running";
            bool result = SkipError;
            AutomationElement? elementUI = null;
            switch (StepType)
            {
                case StepType.StartApp:
                    string path = Path.Combine(Param0, "EZConnect.exe");
                    if (Path.Exists(path))
                    {
                        var p = Process.Start(path);
                        result = p.WaitForInputIdle();
                    }
                    break;

                case StepType.FillTextBox:
                    {
                        elementUI = GetBaseStep()?.GetAutomationElement();
                        if (elementUI != null)
                        {
                            var tb = elementUI.AsTextBox();
                            if (tb.Patterns.Value.IsSupported)
                            {
                                tb.Text = Param1;
                            }
                            else
                            {
                                elementUI.FocusNative();
                                Keyboard.Type(Param1);
                            }
                            result = true;
                        }
                    }
                    break;

                case StepType.ClickButton:
                case StepType.ClickSplitButton:
                    {
                        elementUI = GetBaseStep()?.GetAutomationElement();
                        if (elementUI != null)
                        {
                            elementUI.AsButton()?.Invoke();
                            result = true;
                        }
                    }
                    break;
                
                case StepType.SelectTab:
                    {
                        elementUI = GetBaseStep()?.GetAutomationElement();
                        if (elementUI != null)
                        {
                            elementUI.AsTabItem()?.Select();
                            result = true;
                        }
                    }
                    break;
                
                case StepType.CloseTab:
                    {
                        elementUI = GetBaseStep()?.GetAutomationElement();
                        if (elementUI != null)
                        {
                            elementUI.AsTabItem()?.Select();
                            //
                            var closeBtn = elementUI.AsTabItem().FindFirstDescendant(cf => cf.ByControlType(ControlType.Button));
                            closeBtn?.Click();
                            result = true;
                        }
                    }
                    break;

                case StepType.OpenDialog:
                    {
                        elementUI = GetBaseStep()?.GetAutomationElement();
                        if (elementUI != null)
                        {
                            Task.Run(() => elementUI.Click());
                            await Task.Delay(TimeSpan.FromSeconds(2));
                            result = true;
                        }
                    }
                    break;

                case StepType.SelectDropdown:
                    {
                        elementUI = GetBaseStep()?.GetAutomationElement();
                        if (elementUI != null)
                        {
                            var items = elementUI.AsComboBox().Items;
                            items.FirstOrDefault(x => x.Name.Contains(Param1)).Click();
                            result = true;
                        }
                    }
                    break;

                case StepType.DataItem:
                    {
                        elementUI = GetBaseStep()?.GetAutomationElement();
                        if (elementUI != null)
                        {
                            elementUI.Click();
                            result = true;
                        }
                    }
                    break;
                
                case StepType.SelectGrid:
                case StepType.OpenGrid:
                    {
                        var grid = Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Custom));
                        var rows = grid?.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));
                        if (string.IsNullOrWhiteSpace(Param0))
                        {
                            if (rows.Length > 0)
                            {
                                rows.FirstOrDefault().Click();
                            }
                            result = true;
                            goto RETURN;
                        }
                        foreach (var row in rows)
                        {
                            if (result == false)
                            {
                                var cells = row.FindAllDescendants();
                                for (int i = 0; i < cells.Length; i++)
                                {
                                    if (cells[i].Name.Contains(Param0))
                                    {
                                        var value = cells[i + 1].Patterns?.Value?.PatternOrDefault?.Value;
                                        if (value == Param1)
                                        {
                                            if (StepType == StepType.SelectGrid)
                                                cells[i].Click();
                                            else
                                                cells[i].DoubleClick();
                                            result = true;
                                            goto RETURN;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;

                case StepType.DeleteFile:
                    if (File.Exists(Param0))
                    {
                        File.Delete(Param0);
                    }
                    result = true;
                    break;

                case StepType.ShowMessageBox:
                    App.Bus.Publish<ShowMessage>(new(Param0, "Run Process"));
                    result = true;
                    break;

                case StepType.ButtonText:
                    {
                        elementUI = GetBaseStep()?.GetAutomationElement();
                        if (elementUI != null)
                        {
                            result = elementUI.Name == Param1;
                        }
                    }
                    break;
                    
                case StepType.CheckBox:
                    {
                        elementUI = GetBaseStep()?.GetAutomationElement();
                        if (elementUI != null)
                        {
                            elementUI.AsCheckBox().IsChecked = Param1.Contains("true", StringComparison.CurrentCultureIgnoreCase);
                            result = true;
                        }
                    }
                    break;

                case StepType.CompareFile:
                    int retry = 5;
                    do
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3));
                        retry--;
                    }
                    while (retry > 0 && (File.Exists(Param0) == false || File.Exists(param1) == false));

                    while (DateTime.Now.Subtract(File.GetLastWriteTime(Param0)).TotalSeconds < 5
                        || DateTime.Now.Subtract(File.GetLastWriteTime(param1)).TotalSeconds < 5)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }

                    if (File.Exists(Param0) && File.Exists(param1))
                    {
                        string file1 = await File.ReadAllTextAsync(Param0);
                        string file2 = await File.ReadAllTextAsync(param1);
                        if (string.IsNullOrWhiteSpace(param2) == false)
                        {
                            foreach (var item in param2.Split(",", StringSplitOptions.RemoveEmptyEntries).Reverse())
                            {
                                var split = item.Split("-", StringSplitOptions.RemoveEmptyEntries);
                                if (int.TryParse(split.First(), out int start)
                                    && int.TryParse(split.Last(), out int end))
                                {
                                    try
                                    {
                                        file1 = file1.Remove(start, end - start);
                                        file2 = file2.Remove(start, end - start);
                                    }
                                    catch { }
                                }
                            }
                        }
                        if (file1 == file2)
                        {
                            result = true;
                        }
                    }
                    break;
            }

            RETURN:
            Status = "Passed";
            if (!result)
            {
                Status = "Error";
                Error = "Step run failed";
                throw new Exception(Error);
            }

            if (string.IsNullOrWhiteSpace(CachedPath))
            {
                CachedPath = Constant.GetCachedPath(elementUI);
            }
            return result;
        }   
    }

    public enum StepType
    {
        StartApp = 0,
        //WaitWindow = 1,
        FillTextBox = 2,
        ClickButton = 3,
        DataItem = 4,
        ButtonText = 5,
        SelectTab = 6,
        CompareFile = 7,
        DeleteFile = 8,
        SelectGrid = 9,
        OpenGrid = 10,
        SelectDropdown = 11,
        OpenDialog = 12,
        //SendKey = 13,
        CheckBox = 14,
        ClickSplitButton = 15,
        ShowMessageBox = 16,
        CloseTab = 17,
    }

    public class AutoStepParamRow
    {
        public string Name { get; set; }
        public StepType StepType { get; set; }
        public int Index { get; set; }
        public string Value { get; set; }
    }
}
