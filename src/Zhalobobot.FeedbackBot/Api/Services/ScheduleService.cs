using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Bot.Api.Repositories.Schedule;
using Zhalobobot.Common.Models.Schedule;

namespace Zhalobobot.Bot.Api.Services;

public class ScheduleService
{
    private readonly IScheduleRepository repository;

    public ScheduleService(IScheduleRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Common.Models.Schedule.Schedule> GetAll()
        => await repository.GetAll();

    public async Task<DayWithoutPairs[]> GetDaysWithoutPairs()
        => await repository.GetDaysWithoutPairs();
}