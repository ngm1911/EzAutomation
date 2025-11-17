using AutomationTool.Helper;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;

namespace AutomationTool.DataSource.Steps
{
    public class DataGridStep(AutoStep _autoStep) : IStep
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
                case ActionTypes.Select:
                    result = Select();
                    break;

                case ActionTypes.Open:
                    result = Open();
                    break;

                case ActionTypes.CountItems:
                    result = CountItems();
                    break;
            }
            return Task.FromResult(result);
        }

        private bool Select()
        {
            try
            {
                var grid = Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Custom));
                var rows = grid?.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));
                if (string.IsNullOrWhiteSpace(_autoStep.Param0))
                {
                    if (rows.Length > 0)
                    {
                        rows.FirstOrDefault().Click();
                    }
                    return true;
                }
                foreach (var row in rows)
                {
                    var cells = row.FindAllDescendants();
                    for (int i = 0; i < cells.Length; i++)
                    {
                        if (cells[i].Name.Contains(_autoStep.Param0))
                        {
                            var value = cells[i + 1].Patterns?.Value?.PatternOrDefault?.Value;
                            if (value == _autoStep.Param1)
                            {
                                cells[i].Click();
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool Open()
        {
            try
            {
                var grid = Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Custom));
                var rows = grid?.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));
                if (string.IsNullOrWhiteSpace(_autoStep.Param0))
                {
                    if (rows.Length > 0)
                    {
                        rows.FirstOrDefault().Click();
                    }
                    return true;
                }
                foreach (var row in rows)
                {
                    var cells = row.FindAllDescendants();
                    for (int i = 0; i < cells.Length; i++)
                    {
                        if (cells[i].Name.Contains(_autoStep.Param0))
                        {
                            var value = cells[i + 1].Patterns?.Value?.PatternOrDefault?.Value;
                            if (value == _autoStep.Param1)
                            {
                                cells[i].DoubleClick();
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool CountItems()
        {
            try
            {
                var grid = Constant.GetCachedWindow()?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Custom));
                var rows = grid?.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));
                if (int.TryParse(_autoStep.Param0, out int stepInput) && stepInput == rows.Length)
                {
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
            return [ActionTypes.Select,
                    ActionTypes.Open,
                    ActionTypes.CountItems];
        }
    }
}
