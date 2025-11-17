using AutomationTool.Helper;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using System.Drawing;

namespace AutomationTool.DataSource.Steps
{
    public class PaneStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;
        public AutomationElement? GetAutomationElement()
        {
            elementUI = Constant.GetCachedElement(_autoStep.CachedPath);
            elementUI ??= Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Pane)
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

                case ActionTypes.RightClick:
                    result = RightClick();
                    break;

                case ActionTypes.DoubleClick:
                    result = DoubleClick();
                    break;
            }
            return Task.FromResult(result);
        }

        bool Select()
        {
            try
            {
                var elementUI = GetAutomationElement();
                Mouse.LeftClick(GetMousePoint(elementUI));
                return true;
            }
            catch
            {
                return false;
            }
        }

        bool RightClick()
        {
            try
            {
                var elementUI = GetAutomationElement();
                Mouse.RightClick(GetMousePoint(elementUI));
                return true;
            }
            catch
            {
                return false;
            }
        }

        bool DoubleClick()
        {
            try
            {
                var elementUI = GetAutomationElement();
                Mouse.DoubleClick(GetMousePoint(elementUI));
                return true;
            }
            catch
            {
                return false;
            }
        }

        Point GetMousePoint(AutomationElement element)
        {
            if (element.ControlType == ControlType.Pane
                && element.FrameworkType == FrameworkType.WinForms
                && element.AutomationId.Contains("canvas", StringComparison.CurrentCultureIgnoreCase))
            {
                var points = _autoStep.CachedPath.Split("<tab>").FirstOrDefault()?.Split("<t>").LastOrDefault()?.Split(",");
                if (points?.Length == 2)
                {
                    var rect = element.BoundingRectangle;

                    return new Point(
                        (int)(rect.Left + int.Parse(points.FirstOrDefault())),
                        (int)(rect.Top + int.Parse(points.LastOrDefault()))
                    );
                }
            }

            return Point.Empty;
        }

        public List<ActionTypes> ActionType()
        {
            return [ActionTypes.Select, ActionTypes.DoubleClick, ActionTypes.RightClick];
        }
    }
}
