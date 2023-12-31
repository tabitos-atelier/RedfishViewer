using System.ComponentModel.DataAnnotations;

namespace RedfishViewer.Models
{
    /// <summary>
    /// アプリ設定(1:アプリ構成、2:キーワード一覧)
    /// </summary>
    public class Setting
    {
        [Key]
        public int Id { get; set; }         // ID
        public string? Json { get; set; }   // JSONデータ
    }
}
