// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

//I know these are not great, but it would take quite a while to fix these up properly.
//There is no multithreading going on here, so it should be fine... (famous last words)

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "can't be bothered")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2223:Non-constant static fields should not be visible", Justification = "can't be bothered")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Vulnerability", "S1104:Fields should not have public accessibility", Justification = "can't be bothered")]