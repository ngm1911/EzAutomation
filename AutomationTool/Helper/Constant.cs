using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Security.RightsManagement;

namespace AutomationTool.Helper
{
    public static class Constant
    {
        public static string Passed = "Passed";
        public static string Error = "Error";
        public static string Running = "Running";

        public static Window? CachedMainWindow;

        public static Window? GetCachedWindow()
        {
            var process = Process.GetProcessesByName("EZConnect").FirstOrDefault(x => x.HasExited == false);
            try
            {
                if (CachedMainWindow == null || process?.MainWindowTitle != CachedMainWindow?.Name)
                {
                    using (var app = Application.Attach(process!))
                    using (var automation = new UIA3Automation())
                    {
                        CachedMainWindow = app.GetMainWindow(automation);
                    }
                }
            }
            catch
            {
                using (var app = Application.Attach(process!))
                using (var automation = new UIA3Automation())
                {
                    CachedMainWindow = app.GetMainWindow(automation);
                }
            }

            return CachedMainWindow;
        }

        public static AutomationElement? GetCachedElement(string cachedPath)
        {
            AutomationElement? element = null;
            try
            {
                if (string.IsNullOrWhiteSpace(cachedPath) == false)
                {
                    element = GetCachedWindow();
                    foreach (var item in cachedPath.Split("<tab>").Reverse())
                    {
                        try
                        {
                            AutomationElement? tempElement = null;
                            var items = item.Split("<t>");
                            if (items.All(x => string.IsNullOrWhiteSpace(x)))
                            {
                                continue;
                            }
                            var automationId = items[0];
                            var name = items[1];
                            var controlType = Enum.Parse<ControlType>(items[2]);
                            int.TryParse(items[3], out int index);
                            if (controlType == ControlType.Window)
                            {
                                var process = Process.GetProcessesByName("EZConnect").FirstOrDefault(x => x.HasExited == false);
                                using (var app = Application.Attach(process!))
                                using (var automation = new UIA3Automation())
                                {
                                    element = app.GetAllTopLevelWindows(automation).FirstOrDefault(x => x.Title == name);
                                    continue;
                                }
                            }
                            if (string.IsNullOrWhiteSpace(automationId) == false)
                            {
                                tempElement = element?.FindFirstChild(cf => cf.ByAutomationId(automationId));
                            }
                            if (tempElement == null || tempElement.ControlType != controlType || tempElement.Name != name)
                            {
                                var child = element?.FindAllChildren(cf => cf.ByName(name).And(cf.ByControlType(controlType)));
                                if (child?.Length == 0)
                                {
                                    child = element?.FindAllChildren(cf => cf.ByName(name));
                                }
                                if (child?.Length == 0 || child?.Length > 1)
                                {
                                    child = element?.FindAllChildren(cf => cf.ByControlType(controlType));
                                }
                                if (child?.Length == 0 || child?.Length > 1)
                                {
                                    child = [element.FindChildAt(index)];
                                }
                                tempElement = child?.FirstOrDefault();
                            }
                            if (tempElement == null)
                            {
                                element = null;
                                break;
                            }

                            element = tempElement;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
                element = null;
            }
            return element;
        }
        
        public static string GetCachedPath(AutomationElement? element)
        {
            string cachedPath = string.Empty;
            try
            {
                var index = element?.Parent.FindAllChildren().ToList().IndexOf(element);
                string automationId = string.Empty;
                try
                {
                    automationId = element?.AutomationId ?? string.Empty;
                }
                catch { }
                cachedPath = $"{automationId}<t>{element?.Name}<t>{element?.ControlType}<t>{index}";

                if (element?.ControlType == ControlType.Pane
                    && element.FrameworkType == FrameworkType.WinForms
                    && automationId.Contains("canvas", StringComparison.CurrentCultureIgnoreCase))
                {
                    var p = System.Windows.Forms.Cursor.Position;
                    var rect = element.BoundingRectangle;
                    var relative = new Point(p.X - rect.Left,p.Y - rect.Top);

                    cachedPath += $"<t>{relative.X},{relative.Y}";
                }

                while (element != null
                    && (element?.Parent?.ControlType == ControlType.Window && element?.Parent?.Name == "EZConnect") == false)
                {
                    element = element?.Parent;

                    index = element?.Parent?.FindAllChildren().ToList().IndexOf(element);
                    try
                    {
                        automationId = element?.AutomationId ?? string.Empty;
                    }
                    catch { }
                    cachedPath += $"<tab>{automationId}<t>{element?.Name}<t>{element?.ControlType}<t>{index}";
                }
            }
            catch { }

            return cachedPath;
        }
    }
}
