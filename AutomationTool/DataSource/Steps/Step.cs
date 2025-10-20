using AutomationTool.Helper;
using AutomationTool.Model;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.UIA3;
using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.ServiceProcess;
namespace AutomationTool.DataSource.Steps
{
    internal interface IStep
    {
        Task<bool> Action();

        AutomationElement? GetElementUI();
    }

    public class TextBoxStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;

        public AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)
                                                                      .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.IgnoreCase)
                                                                             .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.IgnoreCase))
                                                                             .Or(cf.ByAutomationId(_autoStep.Param0, PropertyConditionFlags.IgnoreCase))
                                                             ));

            return elementUI;
        }

        public Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.GetText:
                    result = GetText(_autoStep.Param1);
                    break;

                case ActionTypes.SetText:
                    result = SetText(_autoStep.Param1);
                    break;
            }
            return Task.FromResult(result);
        }

        private bool GetText(string text)
        {
            try
            {
                var elementUI = GetAutomationElement();
                if (elementUI?.ControlType == ControlType.Text)
                {
                    return elementUI.AsTextBox().Name == text;
                }
                return elementUI?.AsTextBox().Text == text;
            }
            catch
            {
                return false;
            }
        }

        private bool SetText(string text)
        {
            try
            {
                var elementUI = GetAutomationElement();
                if (elementUI != null)
                {
                    var tb = elementUI.AsTextBox();
                    if (tb.Patterns.Value.IsSupported)
                    {
                        tb.Text = text;
                    }
                    else
                    {
                        elementUI?.FocusNative();
                        Keyboard.Type(text);
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    public class ButtonStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;
        private AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button)
                                                                             .And(cf.ByFrameworkType(FrameworkType.Win32))
                                                                             .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                                    .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button)
                                                                        .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                               .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            if (elementUI == null)
            {
                var elementUIs = Constant.GetCachedWindow()?.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));
                elementUI = elementUIs?.FirstOrDefault(x => x.ToString().Contains($"Tool : {_autoStep.Param0}"));
            }

            
            return elementUI;
        }

        public async Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.Click:
                    result = Click();
                    break;

                case ActionTypes.GetText:
                    result = GetText();
                    break;

                case ActionTypes.OpenDialog:
                    result = await OpenDialog();
                    break;
            }
            return result;
        }

        private bool GetText()
        {
            try
            {
                var elementUI = GetAutomationElement();
                return elementUI?.Name == _autoStep.Param1;
            }
            catch
            {
                return false;
            }
        }

        private bool Click()
        {
            try
            {
                var elementUI = GetAutomationElement();
                elementUI?.AsButton()?.Invoke();
                return elementUI != null;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> OpenDialog()
        {
            try
            {
                var elementUI = GetAutomationElement();
                Task.Run(() => elementUI.Click());
                await Task.Delay(TimeSpan.FromSeconds(2));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }


    public class SplitButtonStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;
        public AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.SplitButton)
                                                                              .And(cf.ByFrameworkType(FrameworkType.Win32))
                                                                              .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                                     .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.SplitButton)
                                                                        .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                               .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            if (elementUI == null)
            {
                var elementUIs = Constant.GetCachedWindow()?.FindAllDescendants(cf => cf.ByControlType(ControlType.SplitButton));
                elementUI = elementUIs?.FirstOrDefault(x => x.ToString().Contains($"Tool : {_autoStep.Param0}"));
            }

            
            return elementUI;
        }

        public Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.Click:
                    result = Click();
                    break;
            }
            return Task.FromResult(result);
        }

        bool Click()
        {
            try
            {
                var elementUI = GetAutomationElement();
                elementUI?.AsButton()?.Invoke();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class TabControlStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;
        public AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.TabItem)
                                                                      .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                             .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            
            return elementUI;
        }

        public Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.Select:
                    result = Select();
                    break;

                case ActionTypes.Close:
                    result = Close();
                    break;
            }
            return Task.FromResult(result);
        }

        bool Select()
        {
            try
            {
                var elementUI = GetAutomationElement();
                elementUI?.AsTabItem()?.Select();
                return true;
            }
            catch 
            { 
                return false;
            }
        }

        bool Close()
        {
            try
            {
                var elementUI = GetAutomationElement();
                elementUI?.AsTabItem()?.Select();
                //
                var closeBtn = elementUI?.AsTabItem().FindFirstDescendant(cf => cf.ByControlType(ControlType.Button));
                closeBtn?.Click();
                //
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class PaneStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;
        public AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Pane)
                                                                      .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                             .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));
            return elementUI;
        }

        public Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.Select:
                    result = Select();
                    break;

                case ActionTypes.RightClick:
                    result = RightClick();
                    break;

                case ActionTypes.DoubleClick:
                    result = DoubleClick();
                    break;
            }
            return Task.FromResult(result);
        }

        bool Select()
        {
            try
            {
                var elementUI = GetAutomationElement();
                Mouse.LeftClick(GetMousePoint(elementUI));
                return true;
            }
            catch
            {
                return false;
            }
        }

        bool RightClick()
        {
            try
            {
                var elementUI = GetAutomationElement();
                Mouse.RightClick(GetMousePoint(elementUI));
                return true;
            }
            catch
            {
                return false;
            }
        }

        bool DoubleClick()
        {
            try
            {
                var elementUI = GetAutomationElement();
                Mouse.DoubleClick(GetMousePoint(elementUI));
                return true;
            }
            catch
            {
                return false;
            }
        }

        Point GetMousePoint(AutomationElement element)
        {
            if (element.ControlType == ControlType.Pane
                && element.FrameworkType == FrameworkType.WinForms
                && element.AutomationId.Contains("canvas", StringComparison.CurrentCultureIgnoreCase))
            {
                var points = _autoStep.CachedPath.Split("<tab>").FirstOrDefault()?.Split("<t>").LastOrDefault()?.Split(",");
                if (points?.Length == 2)
                {
                    var rect = element.BoundingRectangle;

                    return new Point(
                        (int)(rect.Left + int.Parse(points.FirstOrDefault())),
                        (int)(rect.Top + int.Parse(points.LastOrDefault()))
                    );
                }
            }

            return Point.Empty;
        }
    }

    public class DropDownStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;
        public AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.ComboBox)
                                                                        .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                               .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            
            return elementUI;
        }

        public Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.Select:
                    result = Select();
                    break;
            }
            return Task.FromResult(result);
        }

        private bool Select()
        {
            try
            {
                var elementUI = GetAutomationElement();
                elementUI?.AsComboBox().Items.FirstOrDefault(x => x.Name.Contains(_autoStep.Param1))?.Click();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class DataItemStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;
        public AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.DataItem).And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.IgnoreCase)));

            
            return elementUI;
        }

        public Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.Select:
                    result = Select();
                    break;
                case ActionTypes.RightClick:
                    result = RightClick();
                    break;
                case ActionTypes.DoubleClick:
                    result = DoubleClick();
                    break;
            }
            return Task.FromResult(result);
        }

        private bool Select()
        {
            try
            {
                var elementUI = GetAutomationElement();
                elementUI?.Click();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private bool DoubleClick()
        {
            try
            {
                var elementUI = GetAutomationElement();
                elementUI?.DoubleClick();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool RightClick()
        {
            try
            {
                var elementUI = GetAutomationElement();
                elementUI?.RightClick();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class CheckBoxStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;
        public AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.CheckBox)
                                                                       .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                              .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            
            return elementUI;
        }

        public Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.Check:
                    result = Check(true);
                    break;
                case ActionTypes.UnCheck:
                    result = Check(false);
                    break;
            }
            return Task.FromResult(result);
        }

        private bool Check(bool isChecked)
        {
            try
            {
                var elementUI = GetAutomationElement();
                elementUI.AsCheckBox().IsChecked = isChecked;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    
    public class RadioButtonStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;
        public AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.RadioButton)
                                                                       .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                              .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            
            return elementUI;
        }

        public Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.Select:
                    result = Select();
                    break;
            }
            return Task.FromResult(result);
        }

        private bool Select()
        {
            try
            {
                var elementUI = GetAutomationElement();
                elementUI.AsRadioButton().IsChecked = true;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class DataGridStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;
        public AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.RadioButton)
                                                                       .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                              .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            
            return elementUI;
        }

        public Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.Select:
                    result = Select();
                    break;

                case ActionTypes.Open:
                    result = Open();
                    break;
            }
            return Task.FromResult(result);
        }

        private bool Select()
        {
            try
            {
                var grid = Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Custom));
                var rows = grid?.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));
                if (string.IsNullOrWhiteSpace(_autoStep.Param0))
                {
                    if (rows.Length > 0)
                    {
                        rows.FirstOrDefault().Click();
                    }
                    return true;
                }
                foreach (var row in rows)
                {
                    var cells = row.FindAllDescendants();
                    for (int i = 0; i < cells.Length; i++)
                    {
                        if (cells[i].Name.Contains(_autoStep.Param0))
                        {
                            var value = cells[i + 1].Patterns?.Value?.PatternOrDefault?.Value;
                            if (value == _autoStep.Param1)
                            {
                                cells[i].Click();
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        private bool Open()
        {
            try
            {
                var grid = Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Custom));
                var rows = grid?.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));
                if (string.IsNullOrWhiteSpace(_autoStep.Param0))
                {
                    if (rows.Length > 0)
                    {
                        rows.FirstOrDefault().Click();
                    }
                    return true;
                }
                foreach (var row in rows)
                {
                    var cells = row.FindAllDescendants();
                    for (int i = 0; i < cells.Length; i++)
                    {
                        if (cells[i].Name.Contains(_autoStep.Param0))
                        {
                            var value = cells[i + 1].Patterns?.Value?.PatternOrDefault?.Value;
                            if (value == _autoStep.Param1)
                            {
                                cells[i].DoubleClick();
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
    
    public class MenuItemStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;
        public AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.MenuItem)
                                                                       .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                              .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            var process = Process.GetProcessesByName("EZConnect").FirstOrDefault(x => x.HasExited == false);
            using (var app = Application.Attach(process!))
            using (var automation = new UIA3Automation())
            {
                List<AutomationElement> list = new List<AutomationElement>();
                var windows = app.GetAllTopLevelWindows(automation);
                foreach (var window in windows)
                {
                    elementUI = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.MenuItem)
                                                                       .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                              .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));
                    if (elementUI != null)
                        break;
                }
            }

            return elementUI;
        }

        public async Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.Select:
                    result = await Select();
                    break;
            }
            return result;
        }

        private async Task<bool> Select()
        {

            try
            {
                var elementUI = GetAutomationElement();
                Task.Run(() => elementUI?.AsMenuItem().Click());
                await Task.Delay(TimeSpan.FromSeconds(2));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    
    public class WindowStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;

        public AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.RadioButton)
                                                                       .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                              .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            
            return elementUI;
        }

        public Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.Start:
                    result = StartWindow(_autoStep.Param0);
                    break;
            }
            return Task.FromResult(result);
        }

        private bool StartWindow(string applicationPath)
        {
            try
            {
                string path = Path.Combine(applicationPath, "EZConnect.exe");
                if (Path.Exists(path))
                {
                    var psi = new ProcessStartInfo(path)
                    {
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    var p = Process.Start(psi);
                    return p.WaitForInputIdle();
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    public class UnknownStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;

        public async Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.ShowMessageBox:
                    App.Bus.Publish<ShowMessage>(new(_autoStep.Param0, "Run Process"));
                    result = true;
                    break;

                case ActionTypes.DeleteFile:
                    result = DeleteFile(_autoStep.Param0);
                    break;

                case ActionTypes.CopyFile:
                    result = CopyFile(_autoStep.Param0, _autoStep.Param1);
                    break;

                case ActionTypes.CompareFile:
                    result = await CompareFile(_autoStep.Param0, _autoStep.Param1, _autoStep.Param2);
                    break;

                case ActionTypes.ChangeDateTime:
                    result = ChangeDateTime(_autoStep.Param0, _autoStep.Param1);
                    break;

                case ActionTypes.ResetDateTime:
                    result = ChangeDateTime(string.Empty, string.Empty);
                    break;

                case ActionTypes.RestartService:
                    result = RestartService(_autoStep.Param0);
                    break;

                case ActionTypes.WaitTime:
                    result = await WaitTime(_autoStep.Param0);
                    break;
            }
            return result;
        }

        private bool DeleteFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool CopyFile(string source, string target)
        {
            try
            {
                if (File.Exists(source))
                {
                    File.Copy(source, target, true);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> CompareFile(string file1, string file2, string ignores)
        {
            try
            {
                int retry = 5;
                do
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    retry--;
                }
                while (retry > 0 && (File.Exists(file1) == false || File.Exists(file2) == false));

                while (DateTime.Now.Subtract(File.GetLastWriteTime(file1)).TotalSeconds < 5
                    || DateTime.Now.Subtract(File.GetLastWriteTime(file2)).TotalSeconds < 5)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }

                if (File.Exists(file1) && File.Exists(file2))
                {
                    string textFile1 = await File.ReadAllTextAsync(file1);
                    string textFile2 = await File.ReadAllTextAsync(file2);
                    if (string.IsNullOrWhiteSpace(ignores) == false)
                    {
                        foreach (var item in ignores.Split(",", StringSplitOptions.RemoveEmptyEntries).Reverse())
                        {
                            var split = item.Split("-", StringSplitOptions.RemoveEmptyEntries);
                            if (int.TryParse(split.First(), out int start)
                                && int.TryParse(split.Last(), out int end))
                            {
                                try
                                {
                                    textFile1 = textFile1.Remove(start, end - start);
                                    textFile2 = textFile2.Remove(start, end - start);
                                }
                                catch { }
                            }
                        }
                    }
                    if (textFile1 == textFile2)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool ChangeDateTime(string date, string time)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(date) && string.IsNullOrWhiteSpace(time))
                {
                    EnableAutoTime();
                }
                else if (int.TryParse(date, out int intDate))
                {
                    DisableAutoTime();
                    RunCmd($"date {DateTime.Now.AddDays(intDate).ToShortDateString()}");
                    RunCmd($"time {time}");
                }
                return true;
            }
            catch
            {
                return false;
            }

            void RunCmd(string args)
            {
                var p = new ProcessStartInfo("cmd.exe", "/C " + args) { CreateNoWindow = true, UseShellExecute = false };
                Process.Start(p)?.WaitForExit();
            }

            void DisableAutoTime()
            {
                RunCmd("net stop w32time");
                RunCmd("sc config w32time start= disabled");
                using (var key = Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Services\W32Time\Parameters", writable: true))
                {
                    if (key != null) key.SetValue("Type", "NoSync", RegistryValueKind.String);
                }
            }

            void EnableAutoTime()
            {
                RunCmd("sc config w32time start= auto");
                RunCmd("net start w32time");
                RunCmd("w32tm /config /update");
                RunCmd("w32tm /resync /force");
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\W32Time\Parameters", writable: true))
                {
                    if (key != null) key.SetValue("Type", "NTP", RegistryValueKind.String);
                }
            }
        }

        private bool RestartService(string serviceName)
        {
            try
            {
                var service = new ServiceController(serviceName);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private async Task<bool> WaitTime(string minutes)
        {
            try
            {
                if (int.TryParse(minutes, out int intMinutes))
                {
                    await Task.Delay(TimeSpan.FromSeconds(intMinutes));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
