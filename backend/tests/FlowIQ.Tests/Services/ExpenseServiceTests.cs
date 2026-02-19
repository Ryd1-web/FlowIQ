using FlowIQ.Application.DTOs.Expense;
using FlowIQ.Application.Exceptions;
using FlowIQ.Application.Services;
using FlowIQ.Domain.Entities;
using FlowIQ.Domain.Enums;
using FlowIQ.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FlowIQ.Tests.Services;

public class ExpenseServiceTests
{
    private readonly Mock<IExpenseRepository> _expenseRepo;
    private readonly Mock<IBusinessRepository> _businessRepo;
    private readonly ExpenseService _sut;

    public ExpenseServiceTests()
    {
        _expenseRepo = new Mock<IExpenseRepository>();
        _businessRepo = new Mock<IBusinessRepository>();
        _sut = new ExpenseService(_expenseRepo.Object, _businessRepo.Object);
    }

    [Fact]
    public async Task Create_ReturnsExpenseResponse_WhenBusinessExists()
    {
        var businessId = Guid.NewGuid();
        _businessRepo.Setup(r => r.ExistsAsync(businessId)).ReturnsAsync(true);
        _expenseRepo.Setup(r => r.AddAsync(It.IsAny<Expense>()))
            .ReturnsAsync((Expense e) => e);

        var request = new CreateExpenseRequest(15000m, ExpenseCategory.Transport, "Fuel", DateTime.UtcNow, null);

        var result = await _sut.CreateAsync(businessId, request);

        result.Amount.Should().Be(15000m);
        result.Category.Should().Be(ExpenseCategory.Transport);
        result.CategoryName.Should().Be("Transport");
        result.BusinessId.Should().Be(businessId);
    }

    [Fact]
    public async Task Create_ThrowsNotFoundException_WhenBusinessDoesNotExist()
    {
        var businessId = Guid.NewGuid();
        _businessRepo.Setup(r => r.ExistsAsync(businessId)).ReturnsAsync(false);

        var request = new CreateExpenseRequest(15000m, ExpenseCategory.Rent, "Office rent", DateTime.UtcNow, null);
        var act = async () => await _sut.CreateAsync(businessId, request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_ThrowsUnauthorized_WhenBusinessIdMismatch()
    {
        var expenseId = Guid.NewGuid();
        var correctBusinessId = Guid.NewGuid();
        var wrongBusinessId = Guid.NewGuid();

        _expenseRepo.Setup(r => r.GetByIdAsync(expenseId))
            .ReturnsAsync(new Expense { Id = expenseId, BusinessId = correctBusinessId, Description = "x" });

        var act = async () => await _sut.DeleteAsync(expenseId, wrongBusinessId);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task GetByCategory_ReturnsMappedResults()
    {
        var businessId = Guid.NewGuid();
        var expenses = new List<Expense>
        {
            new() { Amount = 5000, Category = ExpenseCategory.Transport, Description = "Fuel", TransactionDate = DateTime.UtcNow, BusinessId = businessId },
            new() { Amount = 3000, Category = ExpenseCategory.Transport, Description = "Bus fare", TransactionDate = DateTime.UtcNow, BusinessId = businessId }
        };

        _expenseRepo.Setup(r => r.GetByCategoryAsync(businessId, ExpenseCategory.Transport)).ReturnsAsync(expenses);

        var result = await _sut.GetByCategoryAsync(businessId, ExpenseCategory.Transport);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.CategoryName.Should().Be("Transport"));
    }
}
