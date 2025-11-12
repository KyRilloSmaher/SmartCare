using FluentValidation;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Products
{
    public class FilterProductsDtoValidation : AbstractValidator<FilterProductsDTo>
    {
        public FilterProductsDtoValidation()
        {
            
            When(filter => filter.FromRate.HasValue, () =>
            {
                RuleFor(filter => filter.FromRate)
                         .GreaterThanOrEqualTo(0.0f)
                         .WithMessage("The value must be greater than or equal to 0");
            });

            When(filter => filter.FromRate.HasValue && filter.ToRate.HasValue, () =>
            {
                RuleFor(filter => filter.ToRate)
                   .GreaterThan(filter => filter.FromRate)
                   .WithMessage("'ToRate' must be greater than 'FromRate'");
            });

            When(filter => filter.FromPrice.HasValue, () =>
            {
                RuleFor(filter => filter.FromPrice)
                         .GreaterThanOrEqualTo(0.0m)
                         .WithMessage("The value must be greater than or equal to 0");
            });

            When(filter => filter.FromPrice.HasValue && filter.ToPrice.HasValue, () =>
            {
                RuleFor(filter => filter.ToPrice)
                   .GreaterThan(filter => filter.FromPrice)
                   .WithMessage("'ToPrice' must be greater than 'FromPrice'");
            });

            RuleFor(filter => filter)
                   .Custom((filter, context) =>
                    {
                       var orderByCount = new[] {
                       filter.OrderByName,
                       filter.OrderByPrice,
                       filter.OrderByRate
                         }.Count(x => x.HasValue);

                       if (orderByCount > 1)
                       {
                          context.AddFailure("Only one OrderBy option can be selected at a time.");
                        }
                  });

        }
    }
}
