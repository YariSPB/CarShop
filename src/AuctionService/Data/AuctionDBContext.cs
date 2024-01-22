using Microsoft.EntityFrameworkCore;
using AuctionService.Entities;

namespace AuctionService.Data;

public class AuctionDBContext : DbContext
{
    public AuctionDBContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Auction> Auctions {get;set;}
}