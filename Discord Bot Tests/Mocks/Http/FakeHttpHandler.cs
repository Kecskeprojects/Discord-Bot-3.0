using System.Net;
using System.Net.Http;
using System.Text;

namespace Discord_Bot.Tests.Mocks.Http;

/// <summary>
/// Answers every request with the JSON the responder returns for its URL, and keeps the requested URLs, so a test can
/// check the query that was sent.
/// </summary>
public sealed class FakeHttpHandler(Func<Uri, string> respond) : HttpMessageHandler
{
    private readonly List<Uri> requests = [];

    public IReadOnlyList<Uri> Requests
    {
        get
        {
            lock (requests)
            {
                return [.. requests];
            }
        }
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Uri uri = request.RequestUri!;
        lock (requests)
        {
            requests.Add(uri);
        }

        HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent(respond(uri), Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}
