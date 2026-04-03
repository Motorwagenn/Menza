using UTB.Minute.Contracts.Enums;

namespace UTB.Minute.Contracts.Orders
{
    public record OrderDto(
    int Id,
    int MenuItemId,
    string MealName,
    OrderStatus Status,
    DateTime CreatedAt
);
}