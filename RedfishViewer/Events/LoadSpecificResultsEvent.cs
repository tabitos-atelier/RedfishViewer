using Prism.Events;

namespace RedfishViewer.Events
{
    /// <summary>
    /// 特定ホストの読み込みイベント
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LoadSpecificResultsEvent<T> : PubSubEvent<string>
    {
    }
}
