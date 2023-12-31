using RedfishViewer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedfishViewer.Services
{
    /// <summary>
    /// データベースアクセス用
    /// </summary>
    public interface IDatabaseAgent
    {
        // データベースの有無
        public bool IsCreated();

        // データベースの新規作成
        public Task<bool> CreateDatabaseAsync();

        // データベースへの保存
        public Task SaveDatabaseAsync();

        // 設定情報を取得する
        public Setting? GetSetting(int id);

        // 設定情報を追加する
        public void AddSetting(Setting item);

        // 設定情報を更新する
        public void UpdateSetting(Setting item);

        // データベースのレスポンス結果テーブルから同じホストのデータを取得する
        public IEnumerable<Result>? GetSameRootUriResults(string rootUri);

        // データベースのレスポンス結果テーブルから同一キーのデータを取得する
        public Result? GetResult(string uri);

        // データベースのレスポンス結果テーブルにデータを追加する
        public Task AddResultAsync(Result result);

        // データベースのレスポンス結果テーブルのデータを更新する
        public Task UpdateResultAsync(Result result);

        // データベースのレスポンス結果テーブルのデータを削除する
        public void RemoveSameRootUriResults(string rootUri);

        // データベースのノード情報テーブルからデータを取得する
        public IEnumerable<Node>? GetNodes();

        // データベースのノード情報テーブルにデータを追加する
        public Node? GetNode(string rootUri);

        // データベースのノード情報テーブルにデータを追加する
        public Task AddNodeAsync(Node node);

        // データベースのノード情報テーブルのデータを更新する
        public Task UpdateNodeAsync(Node node);

        // データベースのノード情報テーブルのデータを削除する
        public void RemoveNode(string rootUri);
    }
}
