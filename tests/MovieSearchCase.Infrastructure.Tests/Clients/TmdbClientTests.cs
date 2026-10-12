using System.Net;
using System.Text;
using FluentAssertions;
using MovieSearchCase.Domain.Exceptions;
using MovieSearchCase.Infrastructure.Clients;
using Xunit;

namespace MovieSearchCase.Infrastructure.Tests.Clients;

public class TmdbClientTests
{
    [Fact]
    public async Task SearchMoviesAsyncShouldEncodeQueryAndMapPagedResponse()
    {
        HttpRequestMessage? request = null;
        var messageHandler = new StubHttpMessageHandler(message =>
        {
            request = message;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"page":2,"total_pages":4,"total_results":63,"results":[{"id":42,"title":"The Answer","overview":"A story","poster_path":"/poster.jpg","backdrop_path":null,"vote_average":9.2,"release_date":"2000-01-02"}]}""",
                    Encoding.UTF8,
                    "application/json"),
            };
        });
        using var httpClient = new HttpClient(messageHandler)
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/"),
        };
        var client = new TmdbClient(httpClient);

        var result = await client.SearchMoviesAsync("answer & life", 2, CancellationToken.None);

        request.Should().NotBeNull();
        request!.RequestUri!.AbsolutePath.Should().Be("/3/search/movie");
        request.RequestUri.Query.Should().Be("?query=answer%20%26%20life&page=2");
        result.Page.Should().Be(2);
        result.TotalPages.Should().Be(4);
        result.TotalResults.Should().Be(63);
        result.Results.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Id = 42,
            Title = "The Answer",
            Overview = "A story",
            PosterPath = "/poster.jpg",
            BackdropPath = (string?)null,
            VoteAverage = 9.2,
            ReleaseDate = new DateOnly(2000, 1, 2),
        });
    }

    [Fact]
    public async Task SearchMoviesAsyncShouldTranslateHttpFailureToUnavailableError()
    {
        var messageHandler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        using var httpClient = new HttpClient(messageHandler)
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/"),
        };
        var client = new TmdbClient(httpClient);

        var exception = await Assert.ThrowsAsync<MovieException>(() =>
            client.SearchMoviesAsync("answer", 1, CancellationToken.None));

        exception.ErrorCode.Should().Be(ErrorType.UpstreamServiceUnavailable);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(responseFactory(request));
    }
}