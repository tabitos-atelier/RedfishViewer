using Prism.Mvvm;
using System;
using System.ComponentModel.DataAnnotations;

namespace RedfishViewer.Models
{
    /// <summary>
    /// HTTPレスポンス結果(Webリクエスト結果)
    /// </summary>
    public class Result : BindableBase
    {
        // ********** HTTPリクエスト情報 **********

        /// <summary>
        /// HTTPリクエスト:URI(KEY)
        /// </summary>
        private string _uri = string.Empty;
        [Key]
        public string Uri
        {
            get => _uri;
            set => SetProperty(ref _uri, value);
        }

        /// <summary>
        /// HTTPリクエスト:メソッド(GET/POST/PUT/PATCH/DELETE)
        /// </summary>
        private string _method = string.Empty;
        [Required]
        public string Method
        {
            get => _method;
            set => SetProperty(ref _method, value);
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

        // ********** HTTPレスポンス情報 **********

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
        /// 更新日付
        /// </summary>
        private DateTime _updated;
        public DateTime Updated
        {
            get => _updated;
            set => SetProperty(ref _updated, value);
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

        /// <summary>
        /// 前回更新日付
        /// </summary>
        private DateTime? _lastUpdated;
        public DateTime? LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }

        /// <summary>
        /// HTTPレスポンス:前回コンテンツ
        /// </summary>
        private string? _lastContent;
        public string? LastContent
        {
            get => _lastContent;
            set => SetProperty(ref _lastContent, value);
        }
    }
}
