﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Features.Teams;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DynamoLeagueBlazor.Shared.Features.Teams.TeamNameListResult;

namespace DynamoLeagueBlazor.Server.Features.Admin;

[Authorize(Policy = PolicyRequirements.Admin)]
[ApiController]
[Route(AddPlayerRouteFactory.Uri)]
public class AddPlayerController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public AddPlayerController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<int> PostAsync([FromBody] AddPlayerRequest request, CancellationToken cancellationToken)
    {
        var command = _mapper.Map<AddPlayerCommand>(request);
        return await _mediator.Send(command, cancellationToken);
    }

    [HttpGet]
    public async Task<TeamNameListResult> GetAsync(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ListQuery(), cancellationToken);
    }
}


public record AddPlayerCommand(string Name, string Position, string Headshot, int TeamId, int ContractValue) : IRequest<int> { }

public class AddPlayerHandler : IRequestHandler<AddPlayerCommand, int>
{
    private readonly ApplicationDbContext _dbContext;
    public AddPlayerHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<int> Handle(AddPlayerCommand request, CancellationToken cancellationToken)
    {
        var player = new Player(request.Name, request.Position, request.Headshot)
        {
            ContractValue = request.ContractValue,
            TeamId = request.TeamId
        };
        player.SetToUnsigned();

        _dbContext.Add(player);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return player.Id;
    }
}

public record ListQuery : IRequest<TeamNameListResult> { }
public class ListHandler : IRequestHandler<ListQuery, TeamNameListResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public ListHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<TeamNameListResult> Handle(ListQuery request, CancellationToken cancellationToken)
    {
        var teams = await _dbContext.Teams
                .ProjectTo<TeamNameItem>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

        return new TeamNameListResult
        {
            Teams = teams
        };
    }
}
public class ListMappingProfile : Profile
{
    public ListMappingProfile()
    {
        CreateMap<Team, TeamNameItem>();
    }
}
public class AddPlayerMappingProfile : Profile
{
    public AddPlayerMappingProfile()
    {
        CreateMap<AddPlayerRequest, AddPlayerCommand>();
    }
}
