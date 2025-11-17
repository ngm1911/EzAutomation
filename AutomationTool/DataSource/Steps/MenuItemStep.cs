using AutomationTool.Helper;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.UIA3;
using System.Diagnostics;

namespace AutomationTool.DataSource.Steps
{
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

        public List<ActionTypes> ActionType()
        {
            return [ActionTypes.Select];
        }
    }
}
