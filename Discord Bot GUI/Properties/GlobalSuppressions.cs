// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

//[assembly: SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Generated Files, Code Cleanup attempts to edit files", Scope = "namespaceanddescendants", Target = "~N:Discord_Bot.Database.Models")]
[assembly: SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Decreases readability when it mosty suggests it to .ToList() LINQ method calls", Scope = "namespaceanddescendants", Target = "~N:Discord_Bot")]
[assembly: SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "Entity Framework cannot translate this type of string comparison", Scope = "namespaceanddescendants", Target = "N:Discord_Bot.Database")]
[assembly: SuppressMessage("Globalization", "CA1309:Use ordinal string comparison", Justification = "Entity Framework cannot translate the StringComparison overloads, and the code fix adds one to comparisons inside query lambdas", Scope = "namespaceanddescendants", Target = "N:Discord_Bot.Database")]
[assembly: SuppressMessage("Globalization", "CA1310:Specify StringComparison for correctness", Justification = "Entity Framework cannot translate the StringComparison overloads, and the code fix adds one to comparisons inside query lambdas", Scope = "namespaceanddescendants", Target = "N:Discord_Bot.Database")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Generated File, Code Cleanup attempts to edit file", Scope = "type", Target = "~T:Discord_Bot.Database.MainDbContext")]
[assembly: SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Double dispose cannot hurt", Scope = "namespaceanddescendants", Target = "~N:Discord_Bot")]
[assembly: SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "False positive: VoiceChannel is read again after the last Task.Delay, and the bot can be disconnected during that delay", Scope = "member", Target = "~M:Discord_Bot.Features.AudioPlayFeature.WaitAsync(Discord_Bot.Communication.ServerAudioResource)~System.Threading.Tasks.Task{System.Boolean}")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The Discord client, InteractionService and CommandService become DI singletons; BotMain disposes the client at shutdown", Scope = "member", Target = "~M:Discord_Bot.Startup.Run~System.IServiceProvider")]
