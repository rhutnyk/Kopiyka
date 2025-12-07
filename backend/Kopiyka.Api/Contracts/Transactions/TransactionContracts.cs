namespace Kopiyka.Api.Contracts.Transactions;

public record TransactionResponse(
    Guid Id,
    Guid AccountId,
    Guid? CategoryId,
    string Description,
    decimal Amount,
    string Currency,
    DateTime OccurredAt);

public record CreateTransactionRequest(
    Guid AccountId,
    Guid? CategoryId,
    string Description,
    decimal Amount,
    string Currency,
    DateTime OccurredAt)
{
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (AccountId == Guid.Empty)
        {
            errors.Add("AccountId is required.");
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            errors.Add("Description is required.");
        }

        if (Amount == 0)
        {
            errors.Add("Amount must be non-zero.");
        }

        if (string.IsNullOrWhiteSpace(Currency))
        {
            errors.Add("Currency is required.");
        }

        if (OccurredAt == default)
        {
            errors.Add("OccurredAt is required.");
        }

        return errors;
    }
}

public record UpdateTransactionRequest(
    string Description,
    decimal Amount,
    Guid? CategoryId,
    DateTime OccurredAt)
{
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Description))
        {
            errors.Add("Description is required.");
        }

        if (Amount == 0)
        {
            errors.Add("Amount must be non-zero.");
        }

        if (OccurredAt == default)
        {
            errors.Add("OccurredAt is required.");
        }

        return errors;
    }
}
