// Copyright (c) 2026, zannagh. All rights reserved.
// See License in the project root for license information.

using Blocwerk.Core.Abstractions;

namespace Blocwerk.Core.Detection.Outlines;

/// <summary>A hold taking part in relocation matching: its id and its fingerprint (position is ignored).</summary>
/// <param name="HoldId">The hold's id.</param>
/// <param name="Fingerprint">The hold's appearance fingerprint.</param>
public sealed record RelocationCandidate(Guid HoldId, HoldFingerprint Fingerprint);
