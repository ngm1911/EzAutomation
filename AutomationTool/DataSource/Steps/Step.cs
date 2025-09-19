using AutomationTool.Helper;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
namespace AutomationTool.DataSource.Steps
{
    internal interface IStep
    {
        AutomationElement? GetAutomationElement();
    }

    public class TextBoxStep(AutoStep _autoStep) : IStep
    {
        public AutomationElement? GetAutomationElement()
        {
            AutomationElement? elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)
                                                                      .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.IgnoreCase)
                                                                             .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.IgnoreCase))
                                                                             .Or(cf.ByAutomationId(_autoStep.Param0, PropertyConditionFlags.IgnoreCase))
                                                             ));
            
            return elementUI;
        }
    }

    public class ButtonStep(AutoStep _autoStep) : IStep
    {
        public AutomationElement? GetAutomationElement()
        {
            AutomationElement? elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
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
    }


    public class SplitButton(AutoStep _autoStep) : IStep
    {
        public AutomationElement? GetAutomationElement()
        {
            AutomationElement? elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
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
    }

    public class TabControlStep(AutoStep _autoStep) : IStep
    {
        public AutomationElement? GetAutomationElement()
        {
            AutomationElement? elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.TabItem)
                                                                      .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                             .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            
            return elementUI;
        }
    }

    public class DropDownStep(AutoStep _autoStep) : IStep
    {
        public AutomationElement? GetAutomationElement()
        {
            AutomationElement? elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.ComboBox)
                                                                        .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                               .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            
            return elementUI;
        }
    }

    public class DataItemStep(AutoStep _autoStep) : IStep
    {
        public AutomationElement? GetAutomationElement()
        {
            AutomationElement? elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.DataItem).And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.IgnoreCase)));

            
            return elementUI;
        }
    }

    public class CheckBoxStep(AutoStep _autoStep) : IStep
    {
        public AutomationElement? GetAutomationElement()
        {
            AutomationElement? elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.CheckBox)
                                                                       .And(cf.ByText(_autoStep.Param0, PropertyConditionFlags.MatchSubstring)
                                                                              .Or(cf.ByName(_autoStep.Param0, PropertyConditionFlags.MatchSubstring))));

            
            return elementUI;
        }
    }
}
