// Copyright (c) 2023- Tabito's Works
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
