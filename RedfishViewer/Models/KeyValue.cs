// Copyright (c) 2023- Tabito's Works
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Mvvm;

namespace RedfishViewer.Models
{
    /// <summary>
    /// キーと値(DataGrid用)
    /// </summary>
    public class KeyValue(string? key, string? value) : BindableBase
    {
        /// <summary>
        /// キー
        /// </summary>
        private string _key = key ?? string.Empty;
        public string Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }

        /// <summary>
        /// 値
        /// </summary>
        private string _value = value ?? string.Empty;
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}
