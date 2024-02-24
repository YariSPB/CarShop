

using AuctionService.Data;
using AuctionService.Dto;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controller;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase {
    private readonly AuctionDBContext _context;
    private readonly IMapper _mapper;

    public IPublishEndpoint PublishEndpoint { get; }

    public AuctionsController(AuctionDBContext context, IMapper mapper, IPublishEndpoint publishEndpoint){
        _context = context;
        _mapper = mapper;
        PublishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date){
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if(!string.IsNullOrEmpty(date)){
            query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id){
        var auction = await _context.Auctions
            .Include(x =>x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if(auction == null) {
            return NotFound();
        }

        return _mapper.Map<AuctionDto>(auction);
        
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>>  CreateAuction(CreateAuctionDto auctionDto){
        var auction = _mapper.Map<Auction>(auctionDto);
        auction.Seller = "test";
        _context.Auctions.Add(auction);

        var newAuction = _mapper.Map<AuctionDto>(auction);
        await PublishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var result = await _context.SaveChangesAsync() > 0;

        if(!result){
            return BadRequest("Couldnt save changes");
        }

        return CreatedAtAction(nameof(GetAuctionById), new {auction.Id},newAuction);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAction(Guid id, UpdateAuctionDto updateAuctionDto){
        var auction = await _context.Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if(auction == null) return NotFound();

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var newUpdated = _mapper.Map<AuctionUpdated>(auction.Item);
        newUpdated.Id = id.ToString();
        await PublishEndpoint.Publish(newUpdated);

        var result = await _context.SaveChangesAsync() > 0;

        if(result) return Ok();

        return BadRequest("Problem saving changes");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAction(Guid id){
        var auction = await _context.Auctions.FindAsync(id);
        if(auction == null) return NotFound();

        await PublishEndpoint.Publish( new AuctionDeleted{Id = id.ToString()});

        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if(!result) return BadRequest("Could not update DB");
        
        return Ok();

    }
}