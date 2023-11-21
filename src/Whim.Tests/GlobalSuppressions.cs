// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// The general justification for these suppression messages is that this project contains tests, and it's
// a bit excessive to have these particular rules for tests.
[assembly: SuppressMessage("Design", "CA1014:Mark assemblies with CLSCompliantAttribute")]
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
[assembly: SuppressMessage("Security", "CA5394:Do not use insecure randomness")]
[assembly: SuppressMessage("Style", "IDE0008:Use explicit type")]
[assembly: SuppressMessage("Style", "IDE0058:Expression value is never used")]
[assembly: SuppressMessage("Style", "IDE0028:Simplify collection initialization")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
[assembly: SuppressMessage("Performance", "CA1852:Seal internal types")]
