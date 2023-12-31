using Prism.Events;
using RedfishViewer.Models;

namespace RedfishViewer.Events
{
    /// <summary>
    /// HTTPエラーイベント
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpErrorEvent<T> : PubSubEvent<HttpError>
    {
    }
}
