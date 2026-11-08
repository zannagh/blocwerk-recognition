// Copyright (c) 2026, zannagh. All rights reserved.
// See License in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Blocwerk.Core.Telemetry;

public class Otel
{
    public static readonly ActivitySource ActivitySource = new("Blocwerk");
    public static readonly Meter Meter = new("Blocwerk");
}
