#nullable enable

using Discord;
using System;
using System.IO;

namespace Discord_Bot.Communication;

//No finalizer: it holds only a managed stream, which cleans up after itself
public sealed partial class WhoKnowsEmbedResult : IDisposable
{
    private bool isDisposed;

    public Embed[] Embed { get; set; } = [];
    public bool HasImage { get; set; }
    public Stream? ImageData { get; set; }
    public string? ImageName { get; set; }

    public void Dispose()
    {
        if (!isDisposed)
        {
            //Null when the embed has no image
            ImageData?.Dispose();
            isDisposed = true;
        }
    }
}
