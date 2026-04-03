namespace UTB.Minute.Contracts.Menu
{
    public record MenuItemDto(
    int Id,
    DateOnly Date,
    int MealId,
    string MealName,
    int AvailablePortions
);
}