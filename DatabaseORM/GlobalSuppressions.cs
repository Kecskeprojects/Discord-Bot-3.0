// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// The services pass query lambdas to the repositories, and EF translates those into SQL
[assembly: SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "Entity Framework cannot translate this type of string comparison", Scope = "namespaceanddescendants", Target = "N:DatabaseORM.Repository")]
[assembly: SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "Entity Framework cannot translate this type of string comparison", Scope = "namespaceanddescendants", Target = "N:DatabaseORM.Service")]
[assembly: SuppressMessage("Globalization", "CA1309:Use ordinal string comparison", Justification = "Entity Framework cannot translate the StringComparison overloads, and the code fix adds one to comparisons inside query lambdas", Scope = "namespaceanddescendants", Target = "N:DatabaseORM.Repository")]
[assembly: SuppressMessage("Globalization", "CA1309:Use ordinal string comparison", Justification = "Entity Framework cannot translate the StringComparison overloads, and the code fix adds one to comparisons inside query lambdas", Scope = "namespaceanddescendants", Target = "N:DatabaseORM.Service")]
[assembly: SuppressMessage("Globalization", "CA1310:Specify StringComparison for correctness", Justification = "Entity Framework cannot translate the StringComparison overloads, and the code fix adds one to comparisons inside query lambdas", Scope = "namespaceanddescendants", Target = "N:DatabaseORM.Repository")]
[assembly: SuppressMessage("Globalization", "CA1310:Specify StringComparison for correctness", Justification = "Entity Framework cannot translate the StringComparison overloads, and the code fix adds one to comparisons inside query lambdas", Scope = "namespaceanddescendants", Target = "N:DatabaseORM.Service")]

[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Generated File, Code Cleanup attempts to edit file", Scope = "type", Target = "~T:DatabaseORM.Context.MainDbContext")]
