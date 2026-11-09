// Copyright (c) 2026, zannagh. All rights reserved.
// See License in the project root for license information.

using Blocwerk.Recognition.Worker.Envelope;
using Blocwerk.Recognition.Worker.Runtime;

namespace Blocwerk.Recognition.Worker.Tests;

/// <summary>The run budget: the claim's <c>runSeconds</c> wins; without it the worker rebuilds the server's old defaults.</summary>
public class RunBudgetTests
{
    [Fact]
    public void TheClaimsRunSecondsWins()
    {
        var claim = FakeRunnerServer.ClaimFor([], "outline-holds") with { RunSeconds = 540 };

        Assert.Equal(TimeSpan.FromSeconds(540), RunBudget.Of(claim, EnvelopeOf("outline-holds")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void WithoutRunSecondsTheOldValuesStay(int? runSeconds)
    {
        var outline = FakeRunnerServer.ClaimFor([], "outline-holds") with { RunSeconds = runSeconds };
        var background = FakeRunnerServer.ClaimFor([], "outline-holds", leaseSeconds: 120) with { RunSeconds = runSeconds };

        Assert.Equal(TimeSpan.FromSeconds(60), RunBudget.Of(outline, EnvelopeOf("outline-holds")));
        Assert.Equal(TimeSpan.FromMinutes(30), RunBudget.Of(background, EnvelopeOf("outline-holds")));
    }

    private static RecognitionEnvelope EnvelopeOf(string kind) =>
        RecognitionEnvelope.Read(TestBundles.Envelope(kind, new { }, ("photo", "photo.png", TestBundles.Png())), kind);
}
