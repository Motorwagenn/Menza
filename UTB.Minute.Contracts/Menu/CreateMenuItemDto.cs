namespace UTB.Minute.Contracts.Menu
{
    public record CreateMenuItemDto(
    DateOnly Date,
    int MealId,
    int AvailablePortions
);
}