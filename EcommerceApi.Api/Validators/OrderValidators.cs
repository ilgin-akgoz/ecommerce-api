using System.Data;
using EcommerceApi.Api.Dtos;
using FluentValidation;

namespace EcommerceApi.Api.Validators;

public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("A valid product must be specified.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("A valid customer must be specified.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("An order must contain at least one item.");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemDtoValidator());
    }
}