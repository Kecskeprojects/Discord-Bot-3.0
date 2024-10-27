using System;
using System.IO;

namespace Discord_Bot.Communication;

public partial class EditPictureResult(string fileName, Stream stream) : IDisposable
{
    private bool _isDisposed;

    public string FileName { get; set; } = fileName;
    public Stream Stream { get; set; } = stream;

    public void Dispose()
    {
        if (!_isDisposed)
        {
            Stream.Dispose();
            _isDisposed = true;
        }
    }

    ~EditPictureResult()
    {
        Dispose();
    }
}
