﻿using DynamoLeagueBlazor.Shared.Features.Teams;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Teams;

public sealed partial class SignPlayer : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    [CascadingParameter] public MudDialogInstance MudDialogInstance { get; set; } = null!;
    [Parameter, EditorRequired] public int PlayerId { get; set; }
    private SignPlayerRequest _form = null!;
    private SignPlayerDetailResult? _signPlayerDetailResult;
    private bool _processingForm;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _form = new SignPlayerRequest { PlayerId = PlayerId };

        await SignPlayerDetailsAsync();
    }

    private async Task SignPlayerDetailsAsync()
    {
        try
        {
            _signPlayerDetailResult = await HttpClient.GetFromJsonAsync<SignPlayerDetailResult>(SignPlayerRouteFactory.Create(PlayerId), _cts.Token);
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        try
        {
            var response = await HttpClient.PostAsJsonAsync(SignPlayerRouteFactory.Uri, _form);

            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("Successfully signed player.", Severity.Success);
            }
            else
            {
                SnackBar.Add("Something went wrong...", Severity.Error);
            }
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        _processingForm = false;
        MudDialogInstance.Close();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
