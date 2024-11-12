using System.ComponentModel.DataAnnotations;

public class ValidateOnCreateAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var property = validationContext.ObjectType.GetProperty("Id");
        if (property != null)
        {
            var idValue = property.GetValue(instance);
            if (idValue != null && idValue is int id && id > 0)
            {
                // Skip validation if the Id is greater than 0 (indicating update)
                return ValidationResult.Success;
            }
        }

        // Apply validation rules for create
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult(ErrorMessage ?? "The field is required.");
        }

        return ValidationResult.Success;
    }
}
