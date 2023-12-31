using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using NLog;
using RedfishViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedfishViewer.Services
{
    /// <summary>
    /// データベースアクセス(EntiryFrameworkCore版)
    /// </summary>
    public class DatabaseContext : DbContext, IDatabaseAgent
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly string _path;

        public string DataSource { get; }                           // SQLiteデータベースのパス

        protected DbSet<Setting>? Settings { get; private set; }    // 設定情報
        protected DbSet<Node>? Nodes { get; private set; }          // ノード情報
        protected DbSet<Result>? Results { get; private set; }      // レスポンス情報(リクエストの結果)

        /// <summary>
        /// DatabaseContext
        /// </summary>
        public DatabaseContext()
        {
            _logger.Trace($"{this.GetType().Name}.");
            var assembly = Assembly.GetExecutingAssembly().GetName();
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? @".";
            _path = Path.Combine(basePath, "data");
            DataSource = Path.Combine(_path, $"{assembly.Name}.db");
        }

        /// <summary>
        /// SQLite の組込み
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _logger.Trace("OnConfiguring.");
            optionsBuilder.UseSqlite($"Data Source={DataSource}; Cache = Shared");
        }

        /// <summary>
        /// データベースが存在するのか？
        /// </summary>
        /// <returns></returns>
        public bool IsCreated()
            => Database.GetService<IRelationalDatabaseCreator>().Exists();

        /// <summary>
        /// データベース新規作成
        /// </summary>
        /// <returns>true:新規, false:既にある</returns>
        public async Task<bool> CreateDatabaseAsync()
        {
            _logger.Debug($"CreateDatabase: {_path}");
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            return !Database.GetService<IRelationalDatabaseCreator>().Exists() && await Database.EnsureCreatedAsync();
        }

        /// <summary>
        /// データベースの保存
        /// </summary>
        /// <returns></returns>
        public async Task SaveDatabaseAsync()
            => await SaveChangesAsync();

        /// <summary>
        /// 設定情報を取得する
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Setting? GetSetting(int id)
            => Settings?.FirstOrDefault(x => x.Id == id);

        /// <summary>
        /// 設定情報を追加する
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void AddSetting(Setting item)
            => Settings?.Add(item);

        /// <summary>
        /// 設定情報を更新する
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void UpdateSetting(Setting item)
            => Settings?.Update(item);

        /// <summary>
        /// データベースのレスポンス結果テーブルから同じホストのデータを取得する
        /// </summary>
        /// <param name="rootUri"></param>
        /// <returns></returns>
        public IEnumerable<Result>? GetSameRootUriResults(string rootUri)
            => Results?.Where(x => Regex.IsMatch(x.Uri, $"^{rootUri}"));

        /// <summary>
        /// データベースのレスポンス結果テーブルから同一キーのデータを取得する
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public Result? GetResult(string uri)
            => Results?.FirstOrDefault(x => x.Uri == uri);

        /// <summary>
        /// データベースのレスポンス結果テーブルにデータを追加する
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public async Task AddResultAsync(Result result)
            => await Task.Run(() => Results?.Add(result));

        /// <summary>
        /// データベースのレスポンス結果テーブルのデータを更新する
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public async Task UpdateResultAsync(Result result)
            => await Task.Run(() => Results?.Update(result));

        /// <summary>
        /// データベースのレスポンス結果テーブルのデータを削除する
        /// </summary>
        /// <param name="rootUri"></param>
        /// <returns></returns>
        public void RemoveSameRootUriResults(string rootUri)
            => Results?.RemoveRange(Results.Where(x => Regex.IsMatch(x.Uri, $"^{rootUri}")));

        /// <summary>
        /// データベースのノード情報テーブルからデータを取得する
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Node>? GetNodes()
            => Nodes;

        /// <summary>
        /// データベースのノード情報テーブルにデータを追加する
        /// </summary>
        /// <param name="rootUri"></param>
        /// <returns></returns>
        public Node? GetNode(string rootUri)
            => Nodes?.FirstOrDefault(x => x.RootUri == rootUri);

        /// <summary>
        /// データベースのノード情報テーブルにデータを追加する
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public async Task AddNodeAsync(Node node)
            => await Task.Run(() => Nodes?.Add(node));

        /// <summary>
        /// データベースのノード情報テーブルにデータを更新する
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public async Task UpdateNodeAsync(Node node)
            => await Task.Run(() => Nodes?.Update(node));

        /// <summary>
        /// データベースのノード情報テーブルのデータを削除する
        /// </summary>
        /// <param name="rootUri"></param>
        /// <returns></returns>
        public void RemoveNode(string rootUri)
            => Nodes?.Remove(Nodes.First(x => x.RootUri == rootUri));
    }
}
