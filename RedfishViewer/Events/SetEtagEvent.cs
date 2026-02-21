// Copyright (c) 2023- Tabito's Works
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
