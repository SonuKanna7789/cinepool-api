using CinePool.API.DTOs;
using FluentValidation;

namespace CinePool.API.DTOs.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.MovieId).GreaterThan(0).WithMessage("Valid MovieId is required.");
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Review text is required.")
            .MaximumLength(2000).WithMessage("Review text must be at most 2000 characters.");
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
    }
}

public class CreateBoostRequestValidator : AbstractValidator<CreateBoostRequest>
{
    public CreateBoostRequestValidator()
    {
        RuleFor(x => x.ReviewId).GreaterThan(0).WithMessage("Valid ReviewId is required.");
        RuleFor(x => x.Comment)
            .MaximumLength(500).WithMessage("Comment must be at most 500 characters.")
            .When(x => x.Comment != null);
    }
}

public class CreatePoolRequestValidator : AbstractValidator<CreatePoolRequest>
{
    public CreatePoolRequestValidator()
    {
        RuleFor(x => x.Plan)
            .NotEmpty().WithMessage("Plan name is required.")
            .MaximumLength(200).WithMessage("Plan name must be at most 200 characters.");

        RuleFor(x => x.PricePerSlot)
            .GreaterThan(0).WithMessage("Price per slot must be greater than 0.");

        RuleFor(x => x.TotalSlots)
            .InclusiveBetween(2, 10).WithMessage("Total slots must be between 2 and 10.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country must be at most 100 characters.");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be in the future.");
    }
}

public class UpdatePreferencesRequestValidator : AbstractValidator<UpdatePreferencesRequest>
{
    public UpdatePreferencesRequestValidator()
    {
        RuleFor(x => x.FavoriteGenres)
            .NotNull().WithMessage("FavoriteGenres is required.");

        RuleFor(x => x.FavoritePlatforms)
            .NotNull().WithMessage("FavoritePlatforms is required.");

        RuleForEach(x => x.FavoriteGenres)
            .NotEmpty().MaximumLength(50);

        RuleForEach(x => x.FavoritePlatforms)
            .NotEmpty().MaximumLength(50);
    }
}
