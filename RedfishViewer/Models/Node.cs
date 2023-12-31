using System;
using System.ComponentModel.DataAnnotations;
using Prism.Mvvm;

namespace RedfishViewer.Models
{
    /// <summary>
    /// ノード情報(HTTPリクエスト実行後)
    /// </summary>
    public class Node : BindableBase
    {
        /// <summary>
        /// ルートURI
        /// </summary>
        private string _rootUri = string.Empty;
        [Key]
        public string RootUri
        {
            get => _rootUri;
            set => SetProperty(ref _rootUri, value);
        }

        /// <summary>
        /// ユーザ名
        /// </summary>
        private string? _username;
        public string? Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        /// <summary>
        /// パスワード
        /// </summary>
        private string? _password;
        public string? Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        /// <summary>
        /// 作成日時
        /// </summary>
        private DateTime _created;
        public DateTime Created
        {
            get => _created;
            set => SetProperty(ref _created, value);
        }

        /// <summary>
        /// 更新日付
        /// </summary>
        private DateTime _updated;
        public DateTime Updated
        {
            get => _updated;
            set => SetProperty(ref _updated, value);
        }

        /// <summary>
        /// プラグイン
        /// </summary>
        private string _plugin = "None";
        public string Plugin
        {
            get => _plugin;
            set => SetProperty(ref _plugin, value);
        }

        /// <summary>
        /// タイトル
        /// </summary>
        private string? _title;
        public string? Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// 概要
        /// </summary>
        private string? _summary;
        public string? Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
        }

        /// <summary>
        /// 備考
        /// </summary>
        private string? _note;
        public string? Note
        {
            get => _note;
            set => SetProperty(ref _note, value);
        }
    }
}
