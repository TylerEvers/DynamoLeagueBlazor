﻿using DynamoLeagueBlazor.Shared.Features.Admin.Users;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Admin.Users;

public class ListTests : IntegrationTestBase
{
    private const string _endpoint = "api/admin/users";

    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenThereIsOneUser_ThenReturnsOneUser()
    {
        var application = CreateAdminAuthenticatedApplication();

        // Make sure the user has a team associated with it.
        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);

        var mockUser = CreateFakeUser(mockTeam.Id);
        mockUser.EmailConfirmed = true;
        await application.AddAsync(mockUser);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<UserListResult>(_endpoint);

        result.Should().NotBeNull();
        result!.Users.Should().HaveCount(1);

        var user = result!.Users.First();
        user.Id.Should().Be(mockUser.Id);
        user.Email.Should().Be(mockUser.Email);
        user.EmailConfirmed.Should().BeTrue();
        user.Team.Should().Be(mockTeam.Name);
    }
}
