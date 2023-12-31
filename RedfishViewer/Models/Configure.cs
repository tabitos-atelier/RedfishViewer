namespace RedfishViewer.Models
{
    /// <summary>
    /// アプリ構成（JSONで保存するため、追加削除は自由）
    /// </summary>
    public class Configure
    {
        public bool IsWindowsMode { get; set; }                         // Windowsモード
        public bool IsDark { get; set; }                                // ライト or ダーク
        public string MahThemeColor { get; set; } = string.Empty;       // テーマ色
        public string PrimaryColor { get; set; } = "Blue";              // プライマリ色
        public string SecondaryColor { get; set; } = "Orange";          // セカンダリ色
        public bool IsColorAdjustment { get; set; }                     // 色調整
        public int MaxTimeout { get; set; } = 3600;                     // リクエストタイムアウト(秒) 1h(3600)
        public bool ProxyEnabled { get; set; }                          // プロキシ有効・無効
        public string ProxyUri { get; set; } = string.Empty;            // プロキシURI
        public string ProxyUsername { get; set; } = string.Empty;       // プロキシユーザ名
        public string ProxyPassword { get; set; } = string.Empty;       // プロキシパスワード
    }
}
