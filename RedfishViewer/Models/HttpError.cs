using Prism.Mvvm;
using System;
using System.ComponentModel.DataAnnotations;

namespace RedfishViewer.Models
{
    /// <summary>
    /// HTTPエラー情報
    /// </summary>
    public class HttpError : BindableBase
    {
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
        /// エラーメッセージ
        /// </summary>
        private string _message = string.Empty;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        /// <summary>
        /// プロキシの有効・無効
        /// </summary>
        private bool _proxyEnabled = false;
        public bool ProxyEnabled
        {
            get => _proxyEnabled;
            set => SetProperty(ref _proxyEnabled, value);
        }

        /// <summary>
        /// HTTPリクエスト:メソッド(GET/POST/PUT/PATCH/DELETE)
        /// </summary>
        private string _method = string.Empty;
        public string Method
        {
            get => _method;
            set => SetProperty(ref _method, value);
        }

        /// <summary>
        /// HTTPリクエスト:URI
        /// </summary>
        private string _uri = string.Empty;
        public string Uri
        {
            get => _uri;
            set => SetProperty(ref _uri, value);
        }

        /// <summary>
        /// HTTPリクエスト:ヘッダー
        /// </summary>
        private string? _inHeaders;
        public string? InHeaders
        {
            get => _inHeaders;
            set => SetProperty(ref _inHeaders, value);
        }

        /// <summary>
        /// HTTPリクエスト:パラメータ
        /// </summary>
        private string? _parameters;
        public string? Parameters
        {
            get => _parameters;
            set => SetProperty(ref _parameters, value);
        }

        /// <summary>
        /// HTTPリクエスト:JSONボディ
        /// </summary>
        private string? _jsonBody;
        public string? JsonBody
        {
            get => _jsonBody;
            set => SetProperty(ref _jsonBody, value);
        }

        /// <summary>
        /// HTTPレスポンス ステータスコード
        /// </summary>
        [Required]
        [RegularExpression("[0-9]+")]
        [Range(100, 599)]
        private int _statusCode;
        public int StatusCode
        {
            get => _statusCode;
            set => SetProperty(ref _statusCode, value);
        }

        /// <summary>
        /// HTTPレスポンス:ヘッダー
        /// </summary>
        [Required]
        private string _outHeaders = string.Empty;
        public string OutHeaders
        {
            get => _outHeaders;
            set => SetProperty(ref _outHeaders, value);
        }

        /// <summary>
        /// Json 文字列かどうか
        /// </summary>
        private bool _isJsonText;
        public bool IsJsonText
        {
            get => _isJsonText;
            set => SetProperty(ref _isJsonText, value);
        }

        /// <summary>
        /// HTTPレスポンス:コンテンツ
        /// </summary>
        private string _content = string.Empty;
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }
    }
}
