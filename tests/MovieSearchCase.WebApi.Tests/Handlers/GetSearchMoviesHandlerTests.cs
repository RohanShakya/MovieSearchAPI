using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieSearchCase.Domain.Entities;
using MovieSearchCase.Domain.Interfaces.Services;
using MovieSearchCase.WebApi.Handlers.Movies;
using Xunit;
using ApiSearchResponse = MovieSearchCase.WebApi.Models.Movies.MovieSearchResponse;

namespace MovieSearchCase.WebApi.Tests.Handlers;

public class GetSearchMoviesHandlerTests
{
    [Fact]
    public async Task HandleShouldReturnPagedResultsMappedToApiModels()
    {
        var searchResult = new MovieSearchResult(
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
        var movieServiceMock = new Mock<IMovieService>();
        movieServiceMock
            .Setup(service => service.SearchAsync("answer", 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);
        var handler = new GetSearchMoviesHandler(movieServiceMock.Object, "answer", 2);

        var result = await handler.HandleAsync(new DefaultHttpContext().Request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiSearchResponse>().Subject;
        response.Page.Should().Be(2);
        response.TotalPages.Should().Be(4);
        response.TotalResults.Should().Be(63);
        response.Results.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Id = 42,
            Title = "The Answer",
            VoteAverage = 9.2,
        });
    }
}