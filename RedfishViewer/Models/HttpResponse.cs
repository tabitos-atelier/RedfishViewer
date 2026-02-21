// Copyright (c) 2023- Tabito's Works
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RestSharp;
using System;

namespace RedfishViewer.Models
{
    /// <summary>
    /// HTTPレスポンス情報
    /// </summary>
    public class HttpResponse
    {
        public Uri? Uri { get; set; }                           // HTTPリクエストURI
        public RestResponse? RestResponse { get; set; }         // レスポンス(成功時)
        public bool IsJsonText { get; set; } = false;           // コンテンツは JSON 形式
        public Result? Result { get; set; }                     // 表示用の形式
        public HttpError? HttpError {  get; set; }              // エラー発生時
    }
}
