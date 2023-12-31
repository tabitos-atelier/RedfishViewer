using MaterialDesignColors;
using RedfishViewer.Models;
using RedfishViewer.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RedfishViewer.Services
{
    public interface IRedfishAdapter : IDisposable
    {
        // 表示用のディレイ値
        public const int DelayedDisplay = 73;

        // HTTPエラー
        public ObservableCollection<HttpError> HttpErrors { get; }

        // アプリケーション構成
        public Configure Configure { get; }

        // カラーマップ
        public Dictionary<string, Swatch> Swatches { get; }

        // 色一覧
        public ObservableCollection<string> Colors { get; }

        // データベース保存用の設定情報
        public Func<List<Setting>>? GetSettings { get; set; }

        // プラグイン
        public List<KeyValuePair<string, BasePlugin?>> Plugins { get; }

        // Webにリクエストを送信する
        public Task<HttpResponse> RequestAsync(Search search);

        // Webレスポンスから自動リクエスト用の @odata.id を取得する
        public ObservableCollection<string> GetOdataIds(Result result);

        // @odata.etag を取得する
        public string GetOdataEtag(string? content);
    }
}
