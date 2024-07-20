using LastFmApi.Enum;

namespace LastFmApi.Communication;
public class GenericResponseItem<T>
{
    public T Response { get; set; }
    public LastFmRequestResultEnum ResultCode { get; set; }
    public Exception Exception { get; set; }
    public LastFmRequestDetails RequestDetails { get; set; }
    public List<LastFmRequestDetails> RequestDetailList { get; set; } = [];
    public string Message { get; set; }
    public int? ErrorCode { get; set; }
}
