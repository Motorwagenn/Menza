namespace UTB.Minute.Db.Entities
{
    public class MenuItem
    {
       
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public int PortionsAvailable { get; set; }

        public int MealId { get; set; }
        public Meal Meal { get; set; }

        public byte[] RowVersion { get; set; } = [];

    }
}
