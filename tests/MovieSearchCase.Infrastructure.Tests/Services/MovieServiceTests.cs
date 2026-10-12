using FluentAssertions;
using Moq;
using MovieSearchCase.Domain.Entities;
using MovieSearchCase.Domain.Interfaces.Clients;
using MovieSearchCase.Infrastructure.Services;
using Xunit;

namespace MovieSearchCase.Infrastructure.Tests.Services;

public class MovieServiceTests
{
    [Fact]
    public async Task GetTrendingAsyncShouldReturnMoviesFromTmdbClient()
    {
        var expectedMovies = new List<Movie>
        {
            new()
            {
                Id = 42,
                Title = "The Answer",
                VoteAverage = 9.2,
            },
        };

        var tmdbClientMock = new Mock<ITmdbClient>();
        tmdbClientMock
            .Setup(client => client.GetTrendingMoviesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMovies);

        var movieService = new MovieService(tmdbClientMock.Object);

        var result = await movieService.GetTrendingAsync(CancellationToken.None);

        result.Should().BeEquivalentTo(expectedMovies);
    }

    [Fact]
    public async Task SearchAsyncShouldReturnPagedResultFromTmdbClient()
    {
        var expectedResult = new MovieSearchResult(
            2,
            4,
            63,
            new List<Movie>
            {
                new()
                {
                    Id = 42,
                    Title = "The Answer",
                    VoteAverage = 9.2,
                },
            });

        var tmdbClientMock = new Mock<ITmdbClient>();
        tmdbClientMock
            .Setup(client => client.SearchMoviesAsync("answer", 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var movieService = new MovieService(tmdbClientMock.Object);

        var result = await movieService.SearchAsync("answer", 2, CancellationToken.None);

        result.Should().BeEquivalentTo(expectedResult);
    }
}
