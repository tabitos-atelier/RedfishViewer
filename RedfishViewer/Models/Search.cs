using System.Collections.Generic;

namespace RedfishViewer.Models
{
    /// <summary>
    /// Web＆キーワード検索
    /// </summary>
    public class Search
    {
        public string Keyword { get; set; } = string.Empty;         // キーワード(URL/検索ワード)
        public string Method { get; set; } = string.Empty;          // HTTPリクエスト:メソッド
        public string Username { get; set; } = string.Empty;        // ユーザ名
        public string Password { get; set; } = string.Empty;        // パスワード
        public List<KeyValue> Headers { get; set; } = [];           // HTTPリクエスト:ヘッダ
        public List<KeyValue> Parameters { get; set; } = [];        // HTTPリクエスト:パラメータ
        public string JsonBody { get; set; } = string.Empty;        // HTTPリクエスト:JSONボディ
        public string ParentUri { get; set; } = string.Empty;       // 自動検索時の親URI
        public bool IsAutoDive { get; set; }                        // 自動検索
    }
}
