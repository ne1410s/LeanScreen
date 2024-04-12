// <copyright file="IsExternalInit.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace System.Runtime.CompilerServices;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Allows use of record types.
/// </summary>
[SuppressMessage(
    "Minor Code Smell",
    "S2094:Classes should not be empty",
    Justification = "Allow for use of record types",
    Scope = "namespace",
    Target = "~N:System.Runtime.CompilerServices")]
internal static class IsExternalInit { }