using FlowIQ.Application.DTOs.Cashflow;
using FlowIQ.Application.Exceptions;
using FlowIQ.Application.Services;
using FlowIQ.Domain.Entities;
using FlowIQ.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FlowIQ.Tests.Services;

public class CashflowServiceTests
{
    private readonly Mock<IIncomeRepository> _incomeRepo;
    private readonly Mock<IExpenseRepository> _expenseRepo;
    private readonly Mock<IBusinessRepository> _businessRepo;
    private readonly CashflowService _sut;

    public CashflowServiceTests()
    {
        _incomeRepo = new Mock<IIncomeRepository>();
        _expenseRepo = new Mock<IExpenseRepository>();
        _businessRepo = new Mock<IBusinessRepository>();
        _sut = new CashflowService(_incomeRepo.Object, _expenseRepo.Object, _businessRepo.Object);
    }

    [Fact]
    public async Task CalculateCashflow_ReturnsHealthy_WhenIncomeExceedsExpense()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var from = DateTime.UtcNow.Date;
        var to = from.AddDays(1);
        var request = new CashflowRequest(businessId, from, to);

        _incomeRepo.Setup(r => r.GetTotalByDateRangeAsync(businessId, from, to)).ReturnsAsync(100000m);
        _expenseRepo.Setup(r => r.GetTotalByDateRangeAsync(businessId, from, to)).ReturnsAsync(50000m);

        // Act
        var result = await _sut.CalculateCashflowAsync(request);

        // Assert
        result.TotalIncome.Should().Be(100000m);
        result.TotalExpense.Should().Be(50000m);
        result.NetCashflow.Should().Be(50000m);
        result.Status.Should().Be(Domain.Enums.CashflowStatus.Healthy);
    }

    [Fact]
    public async Task CalculateCashflow_ReturnsCritical_WhenExpenseExceedsIncome()
    {
        var businessId = Guid.NewGuid();
        var from = DateTime.UtcNow.Date;
        var to = from.AddDays(1);
        var request = new CashflowRequest(businessId, from, to);

        _incomeRepo.Setup(r => r.GetTotalByDateRangeAsync(businessId, from, to)).ReturnsAsync(30000m);
        _expenseRepo.Setup(r => r.GetTotalByDateRangeAsync(businessId, from, to)).ReturnsAsync(50000m);

        var result = await _sut.CalculateCashflowAsync(request);

        result.NetCashflow.Should().Be(-20000m);
        result.Status.Should().Be(Domain.Enums.CashflowStatus.Critical);
    }

    [Fact]
    public async Task CalculateCashflow_ReturnsWarning_WhenExpenseIsOver80Percent()
    {
        var businessId = Guid.NewGuid();
        var from = DateTime.UtcNow.Date;
        var to = from.AddDays(1);
        var request = new CashflowRequest(businessId, from, to);

        _incomeRepo.Setup(r => r.GetTotalByDateRangeAsync(businessId, from, to)).ReturnsAsync(100000m);
        _expenseRepo.Setup(r => r.GetTotalByDateRangeAsync(businessId, from, to)).ReturnsAsync(85000m);

        var result = await _sut.CalculateCashflowAsync(request);

        result.NetCashflow.Should().Be(15000m);
        result.Status.Should().Be(Domain.Enums.CashflowStatus.Warning);
    }

    [Fact]
    public async Task GetDashboardSummary_ThrowsNotFound_WhenBusinessDoesNotExist()
    {
        var businessId = Guid.NewGuid();
        _businessRepo.Setup(r => r.GetByIdAsync(businessId)).ReturnsAsync((Business?)null);

        var act = async () => await _sut.GetDashboardSummaryAsync(businessId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetDashboardSummary_ReturnsCorrectSummary()
    {
        var businessId = Guid.NewGuid();
        _businessRepo.Setup(r => r.GetByIdAsync(businessId))
            .ReturnsAsync(new Business { Id = businessId, Name = "Test Biz", UserId = Guid.NewGuid() });

        _incomeRepo.Setup(r => r.GetTotalByDateRangeAsync(businessId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(50000m);
        _expenseRepo.Setup(r => r.GetTotalByDateRangeAsync(businessId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(30000m);

        var result = await _sut.GetDashboardSummaryAsync(businessId);

        result.BusinessName.Should().Be("Test Biz");
        result.TodayIncome.Should().Be(50000m);
        result.TodayExpense.Should().Be(30000m);
        result.TodayNetCashflow.Should().Be(20000m);
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthyMessage()
    {
        var businessId = Guid.NewGuid();
        _incomeRepo.Setup(r => r.GetTotalByDateRangeAsync(businessId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(100000m);
        _expenseRepo.Setup(r => r.GetTotalByDateRangeAsync(businessId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(40000m);

        var result = await _sut.GetHealthAsync(businessId);

        result.Status.Should().Be(Domain.Enums.CashflowStatus.Healthy);
        result.Message.Should().Contain("looking good");
    }
}
