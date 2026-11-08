// Copyright (c) 2026, zannagh. All rights reserved.
// See License in the project root for license information.

using Blocwerk.Core.Abstractions;
using Blocwerk.Core.Detection.Outlines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blocwerk.HoldDetection.Outlines;

/// <summary>DI registration for hold outlining and relocation matching.</summary>
public static class HoldOutlineServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IHoldOutlineService"/> (stateless OpenCV, singleton) and the position-free
    /// <see cref="HoldRelocationMatcher"/> with its default thresholds.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection AddHoldOutlines(this IServiceCollection services)
    {
        services.TryAddSingleton<IHoldOutlineService, OpenCvHoldOutlineService>();
        services.TryAddSingleton(_ => new HoldRelocationMatcher());
        return services;
    }
}
