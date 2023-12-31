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
