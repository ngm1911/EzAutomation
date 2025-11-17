using AutomationTool.Helper;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;

namespace AutomationTool.DataSource.Steps
{
    public class TextBoxStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;

        public AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)
                                                                      .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.IgnoreCase)
                                                                             .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.IgnoreCase))
                                                                             .Or(cf.ByAutomationId(_autoStep.Param0, PropertyConditionFlags.IgnoreCase))
                                                             ));

            return elementUI;
        }

        public Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.GetText:
                    result = GetText(_autoStep.Param1);
                    break;

                case ActionTypes.SetText:
                    result = SetText(_autoStep.Param1);
                    break;
            }
            return Task.FromResult(result);
        }

        private bool GetText(string text)
        {
            try
            {
                var elementUI = GetAutomationElement();
                if (elementUI?.ControlType == ControlType.Text)
                {
                    return elementUI.AsTextBox().Name == text;
                }
                return elementUI?.AsTextBox().Text == text;
            }
            catch
            {
                return false;
            }
        }

        private bool SetText(string text)
        {
            try
            {
                var elementUI = GetAutomationElement();
                if (elementUI != null)
                {
                    var tb = elementUI.AsTextBox();
                    if (tb.Patterns.Value.IsSupported)
                    {
                        tb.Text = text;
                    }
                    else
                    {
                        elementUI?.FocusNative();
                        Keyboard.Type(text);
                    }
                    return true;
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
            return [ActionTypes.GetText, ActionTypes.SetText];
        }
    }
}
