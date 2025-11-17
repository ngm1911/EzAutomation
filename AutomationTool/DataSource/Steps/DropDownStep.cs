using AutomationTool.Helper;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;

namespace AutomationTool.DataSource.Steps
{
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

        public List<ActionTypes> ActionType()
        {
            return [ActionTypes.Select];
        }
    }
}
