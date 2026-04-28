using Moq;
using RPCMAS.Core.Enums;
using RPCMAS.Core.Exceptions;
using RPCMAS.Core.Interfaces;
using RPCMAS.Infrastructure.Caching;
using RPCMAS.Core.Models;

namespace RPCMAS.Tests;

[TestFixture]
public class PriceChangeRequestServiceTests
{
    private static CreateRequestInput BuildInput(int deptId, int itemId, decimal proposedPrice) => new()
    {
        DepartmentId = deptId,
        ChangeType = ChangeType.Markdown,
        Reason = "Clearance",
        Details = new()
        {
            new RequestDetailInput
            {
                ItemId = itemId,
                ProposedNewPrice = proposedPrice,
                EffectiveDate = DateTime.Today.AddDays(1),
                Remarks = "test"
            }
        }
    };

    [Test]
    public async Task Create_WithNoItems_ThrowsBusinessRule()
    {
        var (db, dept, user, _) = await TestHelpers.SeedAsync();
        var svc = TestHelpers.BuildService(db, user);
        var input = new CreateRequestInput { DepartmentId = dept.Id, ChangeType = ChangeType.Markdown, Details = new() };

        var ex = Assert.ThrowsAsync<BusinessRuleException>(() => svc.CreateAsync(input));
        Assert.That(ex!.Code, Is.EqualTo("REQUEST_NO_ITEMS"));
    }

    [Test]
    public async Task Create_WithProposedPriceZero_ThrowsBusinessRule()
    {
        var (db, dept, user, item) = await TestHelpers.SeedAsync();
        var svc = TestHelpers.BuildService(db, user);

        var ex = Assert.ThrowsAsync<BusinessRuleException>(
            () => svc.CreateAsync(BuildInput(dept.Id, item.Id, 0m)));
        Assert.That(ex!.Code, Is.EqualTo("INVALID_PRICE"));
    }

    [Test]
    public async Task Create_WithProposedEqualsCurrent_ThrowsBusinessRule()
    {
        var (db, dept, user, item) = await TestHelpers.SeedAsync(currentPrice: 100m);
        var svc = TestHelpers.BuildService(db, user);

        var ex = Assert.ThrowsAsync<BusinessRuleException>(
            () => svc.CreateAsync(BuildInput(dept.Id, item.Id, 100m)));
        Assert.That(ex!.Code, Is.EqualTo("INVALID_PRICE"));
    }

    [Test]
    public async Task Create_HappyPath_PersistsDraftAndInvalidatesCache()
    {
        var (db, dept, user, item) = await TestHelpers.SeedAsync(currentPrice: 100m);
        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var svc = TestHelpers.BuildService(db, user, cacheMock: cache);

        var created = await svc.CreateAsync(BuildInput(dept.Id, item.Id, 80m));

        Assert.That(created.Status, Is.EqualTo(RequestStatus.Draft));
        Assert.That(created.RequestNumber, Is.EqualTo("PCR-TEST-0001"));
        Assert.That(created.Details, Has.Count.EqualTo(1));
        Assert.That(created.Details.First().SnapshotCurrentPrice, Is.EqualTo(100m));
        cache.Verify(c => c.RemoveByPrefixAsync(CacheKeys.RequestListPrefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Submit_NonDraft_ThrowsBusinessRule()
    {
        var (db, dept, user, item) = await TestHelpers.SeedAsync();
        var svc = TestHelpers.BuildService(db, user);
        var created = await svc.CreateAsync(BuildInput(dept.Id, item.Id, 80m));
        await svc.SubmitAsync(created.Id, created.RowVersion);

        var ex = Assert.ThrowsAsync<BusinessRuleException>(
            () => svc.SubmitAsync(created.Id, created.RowVersion));
        Assert.That(ex!.Code, Is.EqualTo("INVALID_STATUS"));
    }

    [Test]
    public async Task Approve_NonSubmitted_ThrowsBusinessRule()
    {
        var (db, dept, user, item) = await TestHelpers.SeedAsync();
        var svc = TestHelpers.BuildService(db, user);
        var created = await svc.CreateAsync(BuildInput(dept.Id, item.Id, 80m));

        var ex = Assert.ThrowsAsync<BusinessRuleException>(
            () => svc.ApproveAsync(created.Id, created.RowVersion));
        Assert.That(ex!.Code, Is.EqualTo("INVALID_STATUS"));
    }

    [Test]
    public async Task Apply_NonApproved_ThrowsBusinessRule()
    {
        var (db, dept, user, item) = await TestHelpers.SeedAsync();
        var svc = TestHelpers.BuildService(db, user);
        var created = await svc.CreateAsync(BuildInput(dept.Id, item.Id, 80m));

        var ex = Assert.ThrowsAsync<BusinessRuleException>(
            () => svc.ApplyAsync(created.Id, created.RowVersion));
        Assert.That(ex!.Code, Is.EqualTo("INVALID_STATUS"));
    }

    [Test]
    public async Task Apply_HappyPath_UpdatesItemPriceAndInvalidatesItemCache()
    {
        var (db, dept, user, item) = await TestHelpers.SeedAsync(currentPrice: 100m);
        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var svc = TestHelpers.BuildService(db, user, cacheMock: cache);

        var created = await svc.CreateAsync(BuildInput(dept.Id, item.Id, 80m));
        await svc.SubmitAsync(created.Id, created.RowVersion);
        var submitted = await svc.GetByIdAsync(created.Id);
        await svc.ApproveAsync(submitted.Id, submitted.RowVersion);
        var approved = await svc.GetByIdAsync(submitted.Id);
        await svc.ApplyAsync(approved.Id, approved.RowVersion);

        var applied = await svc.GetByIdAsync(approved.Id);
        Assert.That(applied.Status, Is.EqualTo(RequestStatus.Applied));
        var refreshed = await db.Items.FindAsync(item.Id);
        Assert.That(refreshed!.CurrentPrice, Is.EqualTo(80m));
        cache.Verify(c => c.RemoveByPrefixAsync(CacheKeys.ItemListPrefix, It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(c => c.RemoveByPrefixAsync(CacheKeys.ItemByIdPrefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Reject_OnSubmitted_TransitionsAndStoresReason()
    {
        var (db, dept, user, item) = await TestHelpers.SeedAsync();
        var svc = TestHelpers.BuildService(db, user);
        var created = await svc.CreateAsync(BuildInput(dept.Id, item.Id, 80m));
        await svc.SubmitAsync(created.Id, created.RowVersion);
        var submitted = await svc.GetByIdAsync(created.Id);

        await svc.RejectAsync(submitted.Id, submitted.RowVersion, "Too aggressive");

        var rejected = await svc.GetByIdAsync(submitted.Id);
        Assert.That(rejected.Status, Is.EqualTo(RequestStatus.Rejected));
        Assert.That(rejected.RejectionReason, Is.EqualTo("Too aggressive"));
    }

    [Test]
    public async Task Cancel_AppliedRequest_ThrowsBusinessRule()
    {
        var (db, dept, user, item) = await TestHelpers.SeedAsync();
        var svc = TestHelpers.BuildService(db, user);
        var created = await svc.CreateAsync(BuildInput(dept.Id, item.Id, 80m));
        await svc.SubmitAsync(created.Id, created.RowVersion);
        var s = await svc.GetByIdAsync(created.Id);
        await svc.ApproveAsync(s.Id, s.RowVersion);
        var a = await svc.GetByIdAsync(s.Id);
        await svc.ApplyAsync(a.Id, a.RowVersion);
        var applied = await svc.GetByIdAsync(a.Id);

        var ex = Assert.ThrowsAsync<BusinessRuleException>(
            () => svc.CancelAsync(applied.Id, applied.RowVersion));
        Assert.That(ex!.Code, Is.EqualTo("INVALID_STATUS"));
    }
}
