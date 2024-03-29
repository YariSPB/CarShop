using Microsoft.AspNetCore.Http.Features;
using MongoDB.Entities;

namespace SearchService;

public class AuctionsSvcHttpClient{
    private readonly HttpClient httpClient;
    private readonly IConfiguration config;

    public AuctionsSvcHttpClient(HttpClient httpClient, IConfiguration config){
        this.httpClient = httpClient;
        this.config = config;
    }

    public async Task<List<Item>> GetItemsForSearchDb(){
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        return await httpClient.GetFromJsonAsync<List<Item>>(config["AuctionServiceUrl"] 
                + "/api/auctions?date="+ lastUpdated);
    }
}