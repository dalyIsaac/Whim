// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0047:Remove unnecessary parentheses", Justification = "For clarity", Scope = "member", Target = "~M:Whim.TreeLayout.WindowNode.Equals(System.Object)~System.Boolean")]
[assembly: SuppressMessage("Style", "IDE0047:Remove unnecessary parentheses", Justification = "For clarity", Scope = "member", Target = "~M:Whim.TreeLayout.SplitNode.Equals(System.Object)~System.Boolean")]
[assembly: SuppressMessage("Style", "IDE0072:Add missing cases", Justification = "Fallthrough should make this check redundant", Scope = "member", Target = "~M:Whim.TreeLayout.TreeLayoutEngine.GetAdjacentNode(Whim.TreeLayout.LeafNode,Whim.Direction)~Whim.TreeLayout.LeafNode")]
[assembly: SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "For clarity", Scope = "member", Target = "~M:Whim.TreeLayout.TreeLayoutEngine.AddWindowAtPoint(Whim.IWindow,Whim.IPoint{System.Double},System.Boolean)")]
[assembly: SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "They are exposed for custom subclasses to use in tests", Scope = "module")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "They are exposed for custom subclasses to use in tests", Scope = "module")]
