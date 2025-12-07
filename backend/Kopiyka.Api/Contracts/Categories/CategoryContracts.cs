namespace Kopiyka.Api.Contracts.Categories;

public record CategoryResponse(Guid Id, string Name, string Type, Guid? ParentCategoryId);

public record CreateCategoryRequest(string Name, string Type, Guid? ParentCategoryId)
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

public record UpdateCategoryRequest(string Name, string Type, Guid? ParentCategoryId)
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
