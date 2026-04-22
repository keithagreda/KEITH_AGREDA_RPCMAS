using FluentValidation;
using RPCMAS.API.Dtos;

namespace RPCMAS.API.Validators;

public class RequestDetailDtoValidator : AbstractValidator<RequestDetailDto>
{
    public RequestDetailDtoValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.ProposedNewPrice).GreaterThan(0).WithMessage("Proposed new price must be greater than zero.");
        RuleFor(x => x.EffectiveDate).NotEmpty();
        RuleFor(x => x.Remarks).MaximumLength(500);
    }
}

public class CreateRequestDtoValidator : AbstractValidator<CreateRequestDto>
{
    public CreateRequestDtoValidator()
    {
        RuleFor(x => x.DepartmentId).GreaterThan(0);
        RuleFor(x => x.ChangeType).IsInEnum();
        RuleFor(x => x.Reason).MaximumLength(1000);
        RuleFor(x => x.Details).NotEmpty().WithMessage("A request must contain at least one item.");
        RuleForEach(x => x.Details).SetValidator(new RequestDetailDtoValidator());
    }
}

public class UpdateRequestDtoValidator : AbstractValidator<UpdateRequestDto>
{
    public UpdateRequestDtoValidator()
    {
        RuleFor(x => x.ChangeType).IsInEnum();
        RuleFor(x => x.Reason).MaximumLength(1000);
        RuleFor(x => x.RowVersion).NotEmpty();
        RuleFor(x => x.Details).NotEmpty().WithMessage("A request must contain at least one item.");
        RuleForEach(x => x.Details).SetValidator(new RequestDetailDtoValidator());
    }
}

public class WorkflowActionDtoValidator : AbstractValidator<WorkflowActionDto>
{
    public WorkflowActionDtoValidator()
    {
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public class RejectActionDtoValidator : AbstractValidator<RejectActionDto>
{
    public RejectActionDtoValidator()
    {
        RuleFor(x => x.RowVersion).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
