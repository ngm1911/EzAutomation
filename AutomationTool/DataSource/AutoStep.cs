using AutomationTool.DataSource.Steps;
using AutomationTool.Helper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

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
        private IStep? currentStep;
        
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
                if (CurrentStep is null)
                    CurrentStep = GetBaseStep(ControlType);
                return [.. CurrentStep?.ActionType()];
            }
        }

        partial void OnControlTypeChanged(ControlTypes value)
        {
            CurrentStep = GetBaseStep(value);
        }

        IStep? GetBaseStep(ControlTypes value)
        {
            switch (value)
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
            Status = Constant.Running;
            CurrentStep ??= GetBaseStep(ControlType);
            var result = await CurrentStep?.Action();

            if (!result && !SkipError)
            {
                Status = Constant.Error;
                Error = "Step run failed";
                throw new Exception(Error);
            }

            Status = Constant.Passed;
            result = SkipError; // set it is true
            if (string.IsNullOrWhiteSpace(CachedPath))
            {
                CachedPath = Constant.GetCachedPath(CurrentStep.GetElementUI());
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
        ChangeDateTime,
        ResetDateTime,
        RestartService,
        WaitTime,
        CountItems,
        ExistedFile,
    }
}
