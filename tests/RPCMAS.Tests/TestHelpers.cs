using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using RPCMAS.Core.Entities;
using RPCMAS.Core.Enums;
using RPCMAS.Core.Interfaces;
using RPCMAS.Infrastructure.Data;
using RPCMAS.Infrastructure.Persistence;
using RPCMAS.Infrastructure.Services;

namespace RPCMAS.Tests;

internal static class TestHelpers
{
    public static AppDbContext NewDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(opts);
    }

    public static async Task<(AppDbContext db, Department dept, User user, Item item)> SeedAsync(decimal currentPrice = 100m)
    {
        var db = NewDb();
        var dept = new Department { Name = "Test Dept" };
        var user = new User { Name = "Tester", Role = UserRole.MerchandisingManager };
        db.Departments.Add(dept);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var item = new Item
        {
            Sku = "SKU-001", Name = "Test Item",
            DepartmentId = dept.Id, Category = "Cat", Brand = "BrandX",
            Color = "Red", Size = "M", CurrentPrice = currentPrice, Cost = 50m,
            Status = ItemStatus.Active
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return (db, dept, user, item);
    }

    public static PriceChangeRequestService BuildService(
        AppDbContext db,
        User currentUser,
        Mock<ICacheService>? cacheMock = null,
        Mock<IRequestNumberGenerator>? numberMock = null)
    {
        var cache = cacheMock ?? new Mock<ICacheService>();
        cache.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var numberGen = numberMock ?? new Mock<IRequestNumberGenerator>();
        numberGen.Setup(n => n.NextAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("PCR-TEST-0001");

        var currentUserSvc = new Mock<ICurrentUserService>();
        currentUserSvc.SetupGet(c => c.UserId).Returns(currentUser.Id);
        currentUserSvc.SetupGet(c => c.UserName).Returns(currentUser.Name);
        currentUserSvc.SetupGet(c => c.Role).Returns(currentUser.Role);

        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(ct => db.SaveChangesAsync(ct));
        uow.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((action, ct) => action(ct));

        var itemRepo = new ItemRepository(db);
        var requestRepo = new PriceChangeRequestRepository(db);

        return new PriceChangeRequestService(
            db, requestRepo, itemRepo, uow.Object, cache.Object, currentUserSvc.Object, numberGen.Object);
    }
}
