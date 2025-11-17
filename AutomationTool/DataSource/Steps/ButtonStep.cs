using AutomationTool.Helper;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;

namespace AutomationTool.DataSource.Steps
{
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

        public List<ActionTypes> ActionType()
        {
            return [ActionTypes.Click, 
                    ActionTypes.GetText, 
                    ActionTypes.OpenDialog];
        }
    }
}
