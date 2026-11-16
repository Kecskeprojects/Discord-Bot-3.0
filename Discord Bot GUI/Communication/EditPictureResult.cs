#nullable enable

using System;
using System.IO;

namespace Discord_Bot.Communication;

//No finalizer: it holds only a managed stream, and callers may hand that stream on to another object
public sealed partial class EditPictureResult(string fileName, Stream stream) : IDisposable
{
    private bool isDisposed;

    public string FileName { get; set; } = fileName;
    public Stream Stream { get; set; } = stream;

    public void Dispose()
    {
        if (!isDisposed)
        {
            Stream?.Dispose();
            isDisposed = true;
        }
    }
}
