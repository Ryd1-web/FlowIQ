using FlowIQ.Application.DTOs.Income;
using FlowIQ.Application.Exceptions;
using FlowIQ.Application.Services;
using FlowIQ.Domain.Entities;
using FlowIQ.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FlowIQ.Tests.Services;

public class IncomeServiceTests
{
    private readonly Mock<IIncomeRepository> _incomeRepo;
    private readonly Mock<IBusinessRepository> _businessRepo;
    private readonly IncomeService _sut;

    public IncomeServiceTests()
    {
        _incomeRepo = new Mock<IIncomeRepository>();
        _businessRepo = new Mock<IBusinessRepository>();
        _sut = new IncomeService(_incomeRepo.Object, _businessRepo.Object);
    }

    [Fact]
    public async Task Create_ReturnsIncomeResponse_WhenBusinessExists()
    {
        var businessId = Guid.NewGuid();
        _businessRepo.Setup(r => r.ExistsAsync(businessId)).ReturnsAsync(true);
        _incomeRepo.Setup(r => r.AddAsync(It.IsAny<Income>()))
            .ReturnsAsync((Income i) => i);

        var request = new CreateIncomeRequest(25000m, "Shop Sales", DateTime.UtcNow, null);

        var result = await _sut.CreateAsync(businessId, request);

        result.Amount.Should().Be(25000m);
        result.Source.Should().Be("Shop Sales");
        result.BusinessId.Should().Be(businessId);
    }

    [Fact]
    public async Task Create_ThrowsNotFoundException_WhenBusinessDoesNotExist()
    {
        var businessId = Guid.NewGuid();
        _businessRepo.Setup(r => r.ExistsAsync(businessId)).ReturnsAsync(false);

        var request = new CreateIncomeRequest(25000m, "Shop Sales", DateTime.UtcNow, null);
        var act = async () => await _sut.CreateAsync(businessId, request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_ThrowsNotFoundException_WhenIncomeDoesNotExist()
    {
        var incomeId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        _incomeRepo.Setup(r => r.GetByIdAsync(incomeId)).ReturnsAsync((Income?)null);

        var request = new UpdateIncomeRequest(30000m, "Cash", DateTime.UtcNow, null);
        var act = async () => await _sut.UpdateAsync(incomeId, businessId, request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_ThrowsUnauthorized_WhenBusinessIdMismatch()
    {
        var incomeId = Guid.NewGuid();
        var correctBusinessId = Guid.NewGuid();
        var wrongBusinessId = Guid.NewGuid();

        _incomeRepo.Setup(r => r.GetByIdAsync(incomeId))
            .ReturnsAsync(new Income { Id = incomeId, BusinessId = correctBusinessId, Source = "x" });

        var request = new UpdateIncomeRequest(30000m, "Cash", DateTime.UtcNow, null);
        var act = async () => await _sut.UpdateAsync(incomeId, wrongBusinessId, request);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Delete_CallsRepository_WhenValid()
    {
        var incomeId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var income = new Income { Id = incomeId, BusinessId = businessId, Source = "x" };

        _incomeRepo.Setup(r => r.GetByIdAsync(incomeId)).ReturnsAsync(income);

        await _sut.DeleteAsync(incomeId, businessId);

        _incomeRepo.Verify(r => r.DeleteAsync(income), Times.Once);
    }

    [Fact]
    public async Task GetByDateRange_ReturnsMappedResults()
    {
        var businessId = Guid.NewGuid();
        var from = DateTime.UtcNow.Date;
        var to = from.AddDays(1);
        var incomes = new List<Income>
        {
            new() { Amount = 10000, Source = "Sales", TransactionDate = from, BusinessId = businessId },
            new() { Amount = 20000, Source = "Transfer", TransactionDate = from, BusinessId = businessId }
        };

        _incomeRepo.Setup(r => r.GetByDateRangeAsync(businessId, from, to)).ReturnsAsync(incomes);

        var result = await _sut.GetByDateRangeAsync(businessId, from, to);

        result.Should().HaveCount(2);
    }
}
