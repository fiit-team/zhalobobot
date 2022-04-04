using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Models.FeedbackChat;

namespace Zhalobobot.Api.Server.Repositories.FeedbackChat;

public class FeedbackChatRepository : GoogleSheetsRepositoryBase, IFeedbackChatRepository
{
    private string FeedbackChatDataRange { get; }

    public FeedbackChatRepository(IConfiguration configuration) 
        : base(configuration, configuration["FeedbackSpreadSheetId"])
    {
        FeedbackChatDataRange = configuration["FeedbackChatDataRange"];
    }

    public async Task<(bool ShouldBeUpdated, IEnumerable<FeedbackChatData>)> GetAll()
    {
        var values = await GetRequest(FeedbackChatDataRange).ExecuteAsync();

        var shouldUpdate = ParsingHelper.ParseBool(values.Values[0][0]);
            
        if (!shouldUpdate)
            return (false, Array.Empty<FeedbackChatData>()); //incorrect table or synchronized == false   
        
        return (true, values.Values.Select(r => new FeedbackChatData(
            ParsingHelper.ParseLong(r[0]),
            ParsingHelper.ParseFeedbackTypeRange(r[1], ";").ToArray(),
            ParsingHelper.ParseStringRange(r[2], ";").ToArray(),
            ParsingHelper.ParseStudyGroups(r[3]).ToArray(),
            ParsingHelper.ParseBool(r[4])
        )));
    }
}