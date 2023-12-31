using Prism.Events;

namespace RedfishViewer.Events
{
    /// <summary>
    /// 最新の @odata.etag 設定イベント
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SetEtagEvent<T> : PubSubEvent<string>
    {
    }
}
