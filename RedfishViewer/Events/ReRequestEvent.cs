using Prism.Events;
using RedfishViewer.Models;

namespace RedfishViewer.Events
{
    /// <summary>
    /// 再リクエストイベント
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReRequestEvent<T> : PubSubEvent<Search>
    {
    }
}
