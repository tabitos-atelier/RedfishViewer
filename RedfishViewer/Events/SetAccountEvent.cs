// Copyright (c) 2023-2026 Tabito's Works
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
