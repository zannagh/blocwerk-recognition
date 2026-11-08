// Copyright (c) 2026, zannagh. All rights reserved.
// See License in the project root for license information.

namespace Blocwerk.Core.Abstractions;

/// <summary>A point in normalized image coordinates (x / width, y / height).</summary>
/// <param name="X">X as a fraction of the image width.</param>
/// <param name="Y">Y as a fraction of the image height.</param>
public readonly record struct NormalizedPoint(double X, double Y);
