using AutomationTool.Helper;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;

namespace AutomationTool.DataSource.Steps
{
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

        public List<ActionTypes> ActionType()
        {
            return [ActionTypes.Select,
                    ActionTypes.RightClick,
                    ActionTypes.DoubleClick];
        }
    }
}
