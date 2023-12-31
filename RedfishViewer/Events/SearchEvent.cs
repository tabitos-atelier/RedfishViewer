using Prism.Events;
using RedfishViewer.Models;

namespace RedfishViewer.Events
{
    /// <summary>
    /// 検索イベント
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SearchEvent<T> : PubSubEvent<Search>
    {
    }
}
