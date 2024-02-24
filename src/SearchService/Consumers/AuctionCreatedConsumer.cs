using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper mapper;

    public AuctionCreatedConsumer(IMapper mapper){
        this.mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("Consume message: " + context.Message.Id);
        var item = mapper.Map<Item>(context.Message);

        await item.SaveAsync();
    }
}