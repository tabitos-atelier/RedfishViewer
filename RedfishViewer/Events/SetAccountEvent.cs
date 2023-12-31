using Prism.Events;
using System.Collections.Generic;

namespace RedfishViewer.Events
{
    /// <summary>
    /// アカウント設定イベント
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SetAccountEvent<T> : PubSubEvent<KeyValuePair<string?, string?>>
    {
    }
}
