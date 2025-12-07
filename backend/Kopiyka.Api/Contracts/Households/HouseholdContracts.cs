namespace Kopiyka.Api.Contracts.Households;

public record HouseholdResponse(Guid Id, string Name, string DefaultCurrency, DateTime CreatedAt);

public record CreateHouseholdRequest(string Name, string DefaultCurrency)
{
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(DefaultCurrency))
        {
            errors.Add("Default currency is required.");
        }

        return errors;
    }
}

public record UpdateHouseholdRequest(string Name)
{
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Name is required.");
        }

        return errors;
    }
}
