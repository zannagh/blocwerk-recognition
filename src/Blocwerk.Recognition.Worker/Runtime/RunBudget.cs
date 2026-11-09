// Copyright (c) 2026, zannagh. All rights reserved.
// See License in the project root for license information.

using Blocwerk.Recognition.Worker.Envelope;
using Blocwerk.Recognition.Worker.Protocol;

namespace Blocwerk.Recognition.Worker.Runtime;

/// <summary>
/// How long the server lets a claim run. The claim carries it as <c>runSeconds</c>; for older servers that do not, the app's
/// former defaults are rebuilt from the lease: 30 minutes in the background lane (120 s lease), else 90 s per photo for
/// <c>detect-holds@2</c> and 60 s for the other kinds. Past it the server has ended the job already.
/// </summary>
public static class RunBudget
{
    /// <summary>How long after the run deadline a cancelled job's work may still take to end before the process exits.</summary>
    public static readonly TimeSpan SettleMargin = TimeSpan.FromSeconds(30);

    private const int BackgroundLeaseSeconds = 120;

    public static TimeSpan Of(RunnerClaim claim, RecognitionEnvelope envelope) =>
        claim.RunSeconds is > 0 ? TimeSpan.FromSeconds(claim.RunSeconds.Value)
        : claim.LeaseSeconds >= BackgroundLeaseSeconds ? TimeSpan.FromMinutes(30)
        : claim.Kind == "detect-holds@2" ? TimeSpan.FromSeconds(90) * Math.Max(1, envelope.Images("photos").Count)
        : TimeSpan.FromSeconds(60);
}
