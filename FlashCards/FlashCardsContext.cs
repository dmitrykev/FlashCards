using Microsoft.EntityFrameworkCore;

namespace FlashCards
{
    internal class FlashCardsContext : DbContext
    {
        public DbSet<Card> Cards { get; set; } = null!;

        public FlashCardsContext(DbContextOptions<FlashCardsContext> options) : base(options)
        {
            //Database.EnsureCreated();
        }
    }
}
