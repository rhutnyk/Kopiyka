namespace Kopiyka.Api.Contracts.Accounts;

public record AccountResponse(Guid Id, string Name, string Type, string Currency, decimal Balance);

public record CreateAccountRequest(string Name, string Type, string Currency)
{
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(Type))
        {
            errors.Add("Type is required.");
        }

        if (string.IsNullOrWhiteSpace(Currency))
        {
            errors.Add("Currency is required.");
        }

        return errors;
    }
}

public record UpdateAccountRequest(string Name, string Type)
{
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(Type))
        {
            errors.Add("Type is required.");
        }

        return errors;
    }
}
