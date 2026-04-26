// Copyright (c) 2023-2026 Tabito's Works
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
