// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Decreases readability when it mosty suggests it to .ToList() LINQ method calls", Scope = "namespaceanddescendants", Target = "~N:Discord_Bot")]
[assembly: SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Double dispose cannot hurt", Scope = "namespaceanddescendants", Target = "~N:Discord_Bot")]
[assembly: SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "False positive: VoiceChannel is read again after the last Task.Delay, and the bot can be disconnected during that delay", Scope = "member", Target = "~M:Discord_Bot.Features.AudioPlayFeature.WaitAsync(Discord_Bot.Communication.ServerAudioResource)~System.Threading.Tasks.Task{System.Boolean}")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The Discord client, InteractionService, CommandService and the Last.fm RestClient become DI singletons that live as long as the process; BotMain disposes the Discord client at shutdown", Scope = "member", Target = "~M:Discord_Bot.Startup.ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)")]
