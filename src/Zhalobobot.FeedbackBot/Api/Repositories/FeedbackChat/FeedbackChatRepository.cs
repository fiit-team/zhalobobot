using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Zhalobobot.Bot.Api.Repositories.Common;
using Zhalobobot.Bot.Settings;
using Zhalobobot.Common.Models.FeedbackChat;

namespace Zhalobobot.Bot.Api.Repositories.FeedbackChat;

[RequiresSecretConfiguration(typeof(BotSecrets))]
public class FeedbackChatRepository : GoogleSheetsRepositoryBase, IFeedbackChatRepository
{
    private string FeedbackChatDataRange { get; }

    public FeedbackChatRepository(IVostokHostingEnvironment environment) 
        : base(environment, environment.SecretConfigurationProvider.Get<BotSecrets>().FeedbackSpreadSheetId)
    {
        FeedbackChatDataRange = environment.SecretConfigurationProvider.Get<BotSecrets>().FeedbackChatDataRange;
    }

    public async Task<(bool ShouldBeUpdated, IEnumerable<FeedbackChatData>)> GetAll()
    {
        var values = await GetRequest(FeedbackChatDataRange).ExecuteAsync();

        var shouldUpdate = ParsingHelper.ParseBool(values.Values[0][0]);
            
        if (!shouldUpdate)
            return (false, Array.Empty<FeedbackChatData>()); //incorrect table or synchronized == false   
        
        return (true, values.Values.Skip(2).Select(r => new FeedbackChatData(
            ParsingHelper.ParseLong(r[0]),
            ParsingHelper.ParseFeedbackTypeRange(r[1], ";").ToArray(),
            ParsingHelper.ParseStringRange(r[2], ";").ToArray(),
            ParsingHelper.ParseStudyGroups(r[3]).ToArray(),
            ParsingHelper.ParseBool(r[4])
        )));
    }
}