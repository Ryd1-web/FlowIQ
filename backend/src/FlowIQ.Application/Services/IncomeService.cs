using FlowIQ.Application.DTOs.Income;
using FlowIQ.Application.Exceptions;
using FlowIQ.Application.Interfaces;
using FlowIQ.Domain.Entities;
using FlowIQ.Domain.Interfaces;

namespace FlowIQ.Application.Services;

public class IncomeService : IIncomeService
{
    private readonly IIncomeRepository _incomeRepository;
    private readonly IBusinessRepository _businessRepository;

    public IncomeService(IIncomeRepository incomeRepository, IBusinessRepository businessRepository)
    {
        _incomeRepository = incomeRepository;
        _businessRepository = businessRepository;
    }

    public async Task<IncomeResponse> CreateAsync(Guid businessId, CreateIncomeRequest request)
    {
        if (!await _businessRepository.ExistsAsync(businessId))
            throw new NotFoundException("Business", businessId);

        var income = new Income
        {
            Amount = request.Amount,
            Source = request.Source,
            TransactionDate = request.TransactionDate,
            Notes = request.Notes,
            BusinessId = businessId
        };

        await _incomeRepository.AddAsync(income);
        return MapToResponse(income);
    }

    public async Task<IncomeResponse> UpdateAsync(Guid incomeId, Guid businessId, UpdateIncomeRequest request)
    {
        var income = await _incomeRepository.GetByIdAsync(incomeId)
            ?? throw new NotFoundException("Income", incomeId);

        if (income.BusinessId != businessId)
            throw new UnauthorizedException();

        income.Amount = request.Amount;
        income.Source = request.Source;
        income.TransactionDate = request.TransactionDate;
        income.Notes = request.Notes;
        income.UpdatedAt = DateTime.UtcNow;

        await _incomeRepository.UpdateAsync(income);
        return MapToResponse(income);
    }

    public async Task DeleteAsync(Guid incomeId, Guid businessId)
    {
        var income = await _incomeRepository.GetByIdAsync(incomeId)
            ?? throw new NotFoundException("Income", incomeId);

        if (income.BusinessId != businessId)
            throw new UnauthorizedException();

        await _incomeRepository.DeleteAsync(income);
    }

    public async Task<IncomeResponse?> GetByIdAsync(Guid incomeId, Guid businessId)
    {
        var income = await _incomeRepository.GetByIdAsync(incomeId)
            ?? throw new NotFoundException("Income", incomeId);

        if (income.BusinessId != businessId)
            throw new UnauthorizedException();

        return MapToResponse(income);
    }

    public async Task<IEnumerable<IncomeResponse>> GetByDateRangeAsync(Guid businessId, DateTime from, DateTime to)
    {
        var incomes = await _incomeRepository.GetByDateRangeAsync(businessId, from, to);
        return incomes.Select(MapToResponse);
    }

    private static IncomeResponse MapToResponse(Income i)
        => new(i.Id, i.Amount, i.Source, i.TransactionDate, i.Notes, i.BusinessId, i.CreatedAt);
}
