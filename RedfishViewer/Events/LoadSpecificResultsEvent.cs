// Copyright (c) 2023-2026 Tabito's Works
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
