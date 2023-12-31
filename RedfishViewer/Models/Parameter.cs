using Prism.Mvvm;

namespace RedfishViewer.Models
{
    /// <summary>
    /// HTTPリクエスト:ヘッダ＆パラメータ用(DataGrid)
    /// </summary>
    public class Parameter : BindableBase
    {
        // 有効
        private bool _enabled;
        public bool Enabled 
        {
            get => _enabled;
            set => SetProperty(ref _enabled, value);
        }

        // キー名
        private string _name = string.Empty;
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        // 値
        private string _value = string.Empty;
        public string Value {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}
