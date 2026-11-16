#nullable enable

using LastFmApi.Enum;
using System.Diagnostics.CodeAnalysis;

namespace LastFmApi.Communication;

public class GenericResponseItem<T>
{
    //Set when the request succeeded: check IsSuccess first, which also tells the compiler that Response is set
    public T? Response { get; set; }
    public LastFmRequestResultEnum ResultCode { get; set; }

    [MemberNotNullWhen(true, nameof(Response))]
    public bool IsSuccess => ResultCode == LastFmRequestResultEnum.Success;

    public Exception? Exception { get; set; }
    //Null when the request was not sent because a required parameter was empty
    public LastFmRequestDetails? RequestDetails { get; set; }
    public List<LastFmRequestDetails> RequestDetailList { get; set; } = [];
    public string? Message { get; set; }
    public int? ErrorCode { get; set; }

    public void AddRequestDetailsOf<TOther>(GenericResponseItem<TOther> other)
    {
        if (other.RequestDetails != null)
        {
            RequestDetailList.Add(other.RequestDetails);
        }
    }
}
