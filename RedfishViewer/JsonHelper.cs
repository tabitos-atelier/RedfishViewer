// Copyright (c) 2023-2026 Tabito's Works
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace RedfishViewer
{
    /// <summary>
    /// System.Text.Json 共通オプション
    /// </summary>
    internal static class JsonHelper
    {
        // Redfish / 設定ファイル読み書き用（ゆるめの設定）
        internal static readonly JsonSerializerOptions Options = new()
        {
            NumberHandling              = JsonNumberHandling.AllowReadingFromString,
            AllowTrailingCommas         = true,
            ReadCommentHandling         = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true,
        };

        // インデント整形用（出力のみ）
        internal static readonly JsonSerializerOptions Indented = new()
        {
            WriteIndented = true,
        };

        // JsonDocument パース用
        internal static readonly JsonDocumentOptions DocumentOptions = new()
        {
            AllowTrailingCommas = true,
            CommentHandling     = JsonCommentHandling.Skip,
        };
    }
}
