using Common.EntityModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Validation
{
    public class ExchangeValidator : AbstractValidator<Exchange>
    {
        private static readonly List<string> Timezones = TimeZoneInfo.GetSystemTimeZones().Select(x => x.Id).ToList();

        public ExchangeValidator()
        {
            RuleFor(req => req.Name).NotEmpty().WithMessage("Name must have a value");
            RuleFor(req => req.LongName).NotEmpty().WithMessage("LongName must have a value");
            RuleFor(req => req.Timezone).Must(x => Timezones.Contains(x)).WithMessage("Timezone does not exist");
        }
    }
}