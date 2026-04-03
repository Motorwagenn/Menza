using System;
using System.Collections.Generic;
using System.Text;
using UTB.Minute.Contracts.Enums;

namespace UTB.Minute.Db.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public int MenuItemId { get; set; }

        public MenuItem MenuItem { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
