﻿using DynamoLeagueBlazor.Shared.Features.Teams;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Teams;

public class ListTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();
        var client = application.CreateClient();

        var response = await client.GetAsync(TeamListRouteFactory.Uri);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOneTeam_ThenReturnsOneTeamWithPlayerCounts()
    {
        var application = CreateUserAuthenticatedApplication();
        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);

        var rosteredPlayer = CreateFakePlayer().SetToRostered(DateTime.MaxValue.Year, int.MaxValue);
        rosteredPlayer.TeamId = mockTeam.Id;
        await application.AddAsync(rosteredPlayer);

        var unrosteredPlayer = CreateFakePlayer().SetToUnrostered();
        unrosteredPlayer.TeamId = mockTeam.Id;
        await application.AddAsync(unrosteredPlayer);

        var unsignedPlayer = CreateFakePlayer().SetToUnsigned();
        unsignedPlayer.TeamId = mockTeam.Id;
        await application.AddAsync(unsignedPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<TeamListResult>(TeamListRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.Teams.Should().Contain(t => t.RosteredPlayerCount == "1"
            && t.UnsignedPlayerCount == "1"
            && t.UnrosteredPlayerCount == "1");
    }
}
