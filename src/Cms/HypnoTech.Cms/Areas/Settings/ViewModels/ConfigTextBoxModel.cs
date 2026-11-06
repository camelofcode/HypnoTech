namespace HypnoTech.Cms.Areas.Settings.ViewModels
{
    public class ConfigTextBoxModel
    {
        private readonly string _variableName;
        private readonly string _icon;

        public ConfigTextBoxModel(string variableName, string icon = null)
        {
            _variableName = variableName;
            _icon = icon;
        }

        public string Editing => "editing_" + _variableName;
        public string Value => _variableName;
        public string Edit => "edit_" + _variableName;
        public string TextInput => _variableName;
        public string CancelEdit => "cancelEdit_" + _variableName;
        public string Save => "save_" + _variableName;
        public bool HasIcon => !string.IsNullOrEmpty(_icon);
        public string Icon => _icon;
    }
}
