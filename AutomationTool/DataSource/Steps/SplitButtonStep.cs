using AutomationTool.Helper;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;

namespace AutomationTool.DataSource.Steps
{
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


        public List<ActionTypes> ActionType()
        {
            return [ActionTypes.Click];
        }
    }
}
