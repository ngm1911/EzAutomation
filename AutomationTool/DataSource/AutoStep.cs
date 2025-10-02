using AutomationTool.DataSource.Steps;
using AutomationTool.Helper;
using AutomationTool.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Automation;

namespace AutomationTool.DataSource
{
    public partial class AutoStep : ObservableObject
    {
        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string name;

        [ObservableProperty]
        [JsonIgnore]
        private string status;
        
        [ObservableProperty]
        [JsonIgnore]
        private string error;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ActionItemSource))]
        private ControlTypes controlType;

        [ObservableProperty]
        private ActionTypes actionType;

        [ObservableProperty]
        private string cachedPath;

        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string param0;

        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string param1;

        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string param2;

        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string param3;

        [ObservableProperty]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string param4;

        [ObservableProperty]
        private bool skipError;

        public ObservableCollection<ControlTypes> ControlItemSource { get; } = [.. Enum.GetValues(typeof(ControlTypes)).Cast<ControlTypes>()];

        public ObservableCollection<ActionTypes> ActionItemSource
        {
            get
            {
                List<ActionTypes> actionType = [];
                switch (ControlType)
                {
                    case ControlTypes.Window:
                        actionType.Add(ActionTypes.Start);
                        break;

                    case ControlTypes.DataGrid:
                        actionType.Add(ActionTypes.Select);
                        actionType.Add(ActionTypes.Open);
                        break;

                    case ControlTypes.RadioButton:
                        actionType.Add(ActionTypes.Select);
                        break;

                    case ControlTypes.CheckBox:
                        actionType.Add(ActionTypes.Check);
                        actionType.Add(ActionTypes.UnCheck);
                        break;

                    case ControlTypes.DataItem:
                        actionType.Add(ActionTypes.Select);
                        actionType.Add(ActionTypes.RightClick);
                        actionType.Add(ActionTypes.DoubleClick);
                        break;

                    case ControlTypes.ComboBox:
                        actionType.Add(ActionTypes.Select);
                        break;
                        
                    case ControlTypes.MenuItem:
                        actionType.Add(ActionTypes.Select);
                        break;

                    case ControlTypes.TabItem:
                        actionType.Add(ActionTypes.Select);
                        actionType.Add(ActionTypes.Close);
                        break;

                    case ControlTypes.SplitButton:
                        actionType.Add(ActionTypes.Click);
                        break;

                    case ControlTypes.Button:
                        actionType.Add(ActionTypes.Click);
                        actionType.Add(ActionTypes.GetText);
                        actionType.Add(ActionTypes.OpenDialog);
                        break;

                    case ControlTypes.Text:
                        actionType.Add(ActionTypes.GetText);
                        break;

                    case ControlTypes.TextBox:
                        actionType.Add(ActionTypes.SetText);
                        actionType.Add(ActionTypes.GetText);
                        break;
                        
                    case ControlTypes.Pane:
                        actionType.Add(ActionTypes.Select);
                        actionType.Add(ActionTypes.DoubleClick);
                        actionType.Add(ActionTypes.RightClick);
                        break;

                    default:
                    case ControlTypes.Unknown:
                        actionType.Add(ActionTypes.DeleteFile);
                        actionType.Add(ActionTypes.CompareFile);
                        actionType.Add(ActionTypes.CopyFile);
                        actionType.Add(ActionTypes.ShowMessageBox);
                        break;
                }

                return [.. actionType];
            }
        }

        private IStep? GetBaseStep()
        {
            switch (ControlType)
            {
                case ControlTypes.Window:
                    return new WindowStep(this);

                case ControlTypes.TextBox:
                case ControlTypes.Text:
                    return new TextBoxStep(this);

                case ControlTypes.Button:
                    return new ButtonStep(this);

                case ControlTypes.SplitButton:
                    return new SplitButtonStep(this);

                case ControlTypes.TabItem:
                    return new TabControlStep(this);
                    
                case ControlTypes.Pane:
                    return new PaneStep(this);
                    
                case ControlTypes.ComboBox:
                    return new DropDownStep(this);

                case ControlTypes.DataItem:
                    return new DataItemStep(this);

                case ControlTypes.CheckBox:
                    return new CheckBoxStep(this);

                case ControlTypes.RadioButton:
                    return new RadioButtonStep(this);

                case ControlTypes.DataGrid:
                    return new DataGridStep(this);

                case ControlTypes.MenuItem:
                    return new MenuItemStep(this);

                case ControlTypes.Unknown:
                default:
                    return new UnknownStep(this);
            }
        }

        [RelayCommand]
        private void ClearCache()
        {
            CachedPath = string.Empty;
        }

        [RelayCommand]
        private async Task<bool> RunStep()
        {
            Status = "Running";
            bool result = SkipError;
            IStep? step = GetBaseStep();
            result = await step?.Action();

            Status = "Passed";
            if (!result)
            {
                Status = "Error";
                Error = "Step run failed";
                throw new Exception(Error);
            }

            if (string.IsNullOrWhiteSpace(CachedPath))
            {
                CachedPath = Constant.GetCachedPath(step.GetElementUI());
            }
            return result;
        }   
    }

    public enum ControlTypes
    {
        Unknown,
        //AppBar,
        Button = 2,
        //Calendar,
        CheckBox = 4,
        ComboBox = 5,
        //Custom,
        DataGrid = 7,
        DataItem,
        //Document,
        TextBox = 10,
        //Group,
        //Header,
        //HeaderItem,
        //Hyperlink,
        //Image,
        //List,
        //ListItem,
        //MenuBar,
        //Menu,
        MenuItem,
        Pane,
        //ProgressBar,
        RadioButton = 23,
        //ScrollBar,
        //SemanticZoom,
        //Separator,
        //Slider,
        //Spinner,
        SplitButton = 29,
        //StatusBar,
        //Tab,
        TabItem = 32,
        //Table,
        Text,
        //Thumb,
        //TitleBar,
        //ToolBar,
        //ToolTip,
        //Tree,
        //TreeItem,
        Window = 41
    }

    public enum ActionTypes
    {
        Start,
        Click,
        DoubleClick,
        RightClick,
        SetText,
        GetText,
        CompareFile,
        DeleteFile,
        Check,
        UnCheck,
        Close,
        Open,
        Select,
        OpenDialog,
        ShowMessageBox,
        CopyFile,
    }
}
