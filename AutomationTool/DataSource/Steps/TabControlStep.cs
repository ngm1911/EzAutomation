using AutomationTool.Helper;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;

namespace AutomationTool.DataSource.Steps
{
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

        public List<ActionTypes> ActionType()
        {
            return [ActionTypes.Select, ActionTypes.Close];
        }
    }
}
