using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Bot.Api.Repositories.Subjects;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Bot.Api.Services;

public class SubjectsService
{
    private readonly ISubjectRepository repository;

    public SubjectsService(ISubjectRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Subject[]> GetAll()
        => await repository.GetAll();
}