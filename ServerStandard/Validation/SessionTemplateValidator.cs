using Common.EntityModels;
using FluentValidation;

namespace Server.Validation
{
    public class SessionTemplateValidator : AbstractValidator<SessionTemplate>
    {
        public SessionTemplateValidator()
        {
            RuleFor(req => req.Name).NotEmpty().WithMessage("Must have a name");
        }
    }
}