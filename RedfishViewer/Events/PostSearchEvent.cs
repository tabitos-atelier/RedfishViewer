using Prism.Events;

namespace RedfishViewer.Events
{
    /// <summary>
    /// 検索後処理イベント
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PostSearchEvent<T> : PubSubEvent<string?>
    {
    }
}
