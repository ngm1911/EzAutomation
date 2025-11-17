using AutomationTool.Helper;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using System.Diagnostics;
using System.IO;

namespace AutomationTool.DataSource.Steps
{
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

        public List<ActionTypes> ActionType()
        {
            return [ActionTypes.Start];
        }
    }
}
