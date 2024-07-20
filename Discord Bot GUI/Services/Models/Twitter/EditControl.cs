using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class EditControl
{
    [JsonProperty("edit_tweet_ids")]
    public List<string> EditTweetIds { get; set; }

    [JsonProperty("editable_until_msecs")]
    public string EditableUntilMsecs { get; set; }

    [JsonProperty("is_edit_eligible")]
    public bool IsEditEligible { get; set; }

    [JsonProperty("edits_remaining")]
    public string EditsRemaining { get; set; }
}
