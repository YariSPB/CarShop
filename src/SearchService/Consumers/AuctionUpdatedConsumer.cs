using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>{
    private readonly IMapper mapper;

    public AuctionUpdatedConsumer(IMapper mapper){
        this.mapper = mapper;
    }



    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine("Consume message: " + context.Message.Id);
       await DB.Update<Item>()
            .MatchID(context.Message.Id)
            .Modify(a => a.Make, context.Message.Make)
            .Modify(a => a.Model, context.Message.Model)
            .Modify(a => a.Year, context.Message.Year)
            .Modify(a => a.Color, context.Message.Color)
            .Modify(a => a.Mileage, context.Message.Mileage)
            .Modify(a => a.ImageUrl, context.Message.ImageUrl)
            .ExecuteAsync();
    }
}