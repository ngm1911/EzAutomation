using AutomationTool.Helper;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;

namespace AutomationTool.DataSource.Steps
{
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

        public List<ActionTypes> ActionType()
        {
            return [ActionTypes.Check,
                    ActionTypes.UnCheck];
        }
    }
}
