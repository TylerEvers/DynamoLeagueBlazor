﻿using DynamoLeagueBlazor.Shared.Features.Dashboard;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Dashboard;

public class TopTeamFinesTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(TopTeamFinesRouteFactory.Uri);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOneTeamWithAFine_ThenReturnsOneTeamWithAFine()
    {
        var application = CreateUserAuthenticatedApplication();

        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);

        var stubPlayer = CreateFakePlayer();
        stubPlayer.TeamId = mockTeam.Id;
        var mockFine = stubPlayer.AddFine(int.MaxValue, RandomString);
        mockFine.Status = true;
        await application.AddAsync(stubPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<TopTeamFinesResult>(TopTeamFinesRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.Teams.Should().HaveCount(1);

        var firstTeam = result.Teams.First();
        firstTeam.Amount.Should().Be(mockFine.Amount.ToString("C0"));
        firstTeam.Name.Should().Be(mockTeam.Name);
        firstTeam.ImageUrl.Should().Be(mockTeam.LogoUrl);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsMultipleTeamsWithApprovedFines_ThenReturnsTheHighestFineCountFirst()
    {
        var application = CreateUserAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await application.AddAsync(stubTeam);

        var mockPlayer1 = CreateFakePlayer();
        mockPlayer1.TeamId = stubTeam.Id;
        var fine = mockPlayer1.AddFine(int.MaxValue, RandomString);
        fine.Status = true;
        await application.AddAsync(mockPlayer1);

        var mockPlayer2 = CreateFakePlayer();
        mockPlayer2.TeamId = stubTeam.Id;
        var lowestFine = mockPlayer2.AddFine(int.MinValue, RandomString);
        lowestFine.Status = true;
        await application.AddAsync(mockPlayer2);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<TopTeamFinesResult>(TopTeamFinesRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.Teams.Should().BeInDescendingOrder(t => t.Amount);
    }
}
