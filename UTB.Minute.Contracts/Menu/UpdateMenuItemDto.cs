namespace UTB.Minute.Contracts.Menu
{
    public record UpdateMenuItemDto(
    DateOnly Date,
    int MealId,
    int AvailablePortions
);
}