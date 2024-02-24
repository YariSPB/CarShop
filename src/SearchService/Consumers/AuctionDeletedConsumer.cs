using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>{

    public AuctionDeletedConsumer(){

    }



    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine("Consume message: " + context.Message.Id);
       await DB.DeleteAsync<Item>(context.Message.Id);
    }
}