namespace Discord_Bot.Communication
{
    public class InstagramMessageResult
    {
        public InstagramMessageResult()
        {
            ShouldMessageBeSuppressed = false;
            HasFileDownloadHappened = false;
        }

        public bool ShouldMessageBeSuppressed { get; set; }
        public bool HasFileDownloadHappened { get; set; }
    }
}
