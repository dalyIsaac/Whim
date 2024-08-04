// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
	"Design",
	"CA1002:Do not expose generic lists",
	Justification = "They are used by subclasses",
	Scope = "module"
)]
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
[assembly: SuppressMessage(
	"Performance",
	"CA1813:Avoid unsealed attributes",
	Justification = "It's fine for tests",
	Scope = "namespaceanddescendants",
	Target = "~N:Whim.TestUtils"
)]
[assembly: SuppressMessage(
	"Design",
	"CA1019:Define accessors for attribute arguments",
	Justification = "It's fine for InlineData"
)]
[assembly: SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
