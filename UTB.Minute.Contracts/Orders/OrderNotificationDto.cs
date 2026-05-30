using System;
using System.Collections.Generic;
using System.Text;
using UTB.Minute.Contracts.Enums;

namespace UTB.Minute.Contracts.Orders
{
    public record OrderNotificationDto(int OrderId, string MealName , OrderStatus Status);
}
