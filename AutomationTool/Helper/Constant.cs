using AutomationTool.DataSource;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using FlaUI.UIA3;
using FlaUI.UIA3.Converters;
using FlaUI.UIA3.EventHandlers;
using FlaUI.Core.EventHandlers;
using System.Diagnostics;

namespace AutomationTool.Helper
{
    public static class Constant
    {
        public static Window? CachedMainWindow;

        public static Window? GetCachedWindow()
        {
            var process = Process.GetProcessesByName("EZConnect").FirstOrDefault(x => x.HasExited == false);
            if (CachedMainWindow == null || process?.MainWindowTitle != CachedMainWindow?.Name)
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
                        AutomationElement tempElement = null;
                        var items = item.Split("<t>");
                        var automationId = items[0];
                        var name = items[1];
                        var controlType = Enum.Parse<ControlType>(items[2]);
                        var index = int.Parse(items[3]);

                        if (string.IsNullOrWhiteSpace(automationId) == false)
                        {
                            tempElement = element.FindFirstChild(cf => cf.ByAutomationId(automationId));
                        }
                        if (tempElement == null || tempElement.ControlType != controlType || tempElement.Name != name)
                        {
                            var child = element.FindAllChildren(cf => cf.ByName(name).And(cf.ByControlType(controlType)));
                            if (child.Length == 0)
                            {
                                child = element.FindAllChildren(cf => cf.ByName(name));
                            }
                            if (child.Length == 0 || child.Length > 1)
                            {
                                child = [element.FindChildAt(index)];
                            }
                            tempElement = child.FirstOrDefault();
                        }
                        if (tempElement == null)
                        {
                            element = null;
                            break;
                        }

                        element = tempElement;
                    }
                }
            }
            catch { element = null; }
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
                    automationId = element?.AutomationId;
                }
                catch { }
                cachedPath = $"{automationId}<t>{element?.Name}<t>{element?.ControlType}<t>{index}";

                while (element != null
                    && (element?.Parent?.ControlType == ControlType.Window && element?.Parent?.Name == "EZConnect") == false)
                {
                    element = element?.Parent;

                    index = element?.Parent?.FindAllChildren().ToList().IndexOf(element);
                    try
                    {
                        automationId = element?.AutomationId;
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
