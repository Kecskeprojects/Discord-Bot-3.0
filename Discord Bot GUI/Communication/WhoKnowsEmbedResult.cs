using Discord;
using System;
using System.IO;

namespace Discord_Bot.Communication;
public class WhoKnowsEmbedResult : IDisposable
{
    private bool _isDisposed;

    public Embed[] Embed { get; set; }
    public bool HasImage { get; set; }
    public Stream ImageData { get; set; }
    public string ImageName { get; set; }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            ImageData.Dispose();
            _isDisposed = true;
        }
    }

    ~WhoKnowsEmbedResult()
    {
        Dispose();
    }
}
