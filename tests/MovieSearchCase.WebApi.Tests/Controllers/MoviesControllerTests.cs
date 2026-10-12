using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieSearchCase.Domain.Exceptions;
using MovieSearchCase.Shared;
using MovieSearchCase.WebApi.Controllers.V1;
using MovieSearchCase.WebApi.Handlers.Movies;
using Xunit;

namespace MovieSearchCase.WebApi.Tests.Controllers;

public class MoviesControllerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchShouldRejectMissingOrBlankQuery(string? query)
    {
        var factoryMock = new Mock<IMoviesRequestHandlerFactory>();
        var controller = new MoviesController(factoryMock.Object);

        var exception = await Assert.ThrowsAsync<MovieException>(() => controller.Search(query));

        exception.ErrorCode.Should().Be(ErrorType.InvalidRequest);
        exception.Message.Should().Be("The 'query' parameter is required.");
        factoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(501)]
    public async Task SearchShouldRejectPageOutsideSupportedRange(int page)
    {
        var factoryMock = new Mock<IMoviesRequestHandlerFactory>();
        var controller = new MoviesController(factoryMock.Object);

        var exception = await Assert.ThrowsAsync<MovieException>(() => controller.Search("answer", page));

        exception.ErrorCode.Should().Be(ErrorType.InvalidRequest);
        exception.Message.Should().Be("The 'page' parameter must be between 1 and 500.");
        factoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SearchShouldDefaultPageToOne()
    {
        var requestHandlerMock = new Mock<IRequestHandlerAsync>();
        requestHandlerMock
            .Setup(handler => handler.HandleAsync(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new OkObjectResult(new object()));
        var factoryMock = new Mock<IMoviesRequestHandlerFactory>();
        factoryMock
            .Setup(factory => factory.SearchMovies("answer", 1))
            .Returns(requestHandlerMock.Object);
        var controller = new MoviesController(factoryMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };

        await controller.Search("answer");

        factoryMock.Verify(factory => factory.SearchMovies("answer", 1), Times.Once);
    }

    [Fact]
    public async Task SearchShouldTrimQueryBeforeCallingHandler()
    {
        var requestHandlerMock = new Mock<IRequestHandlerAsync>();
        requestHandlerMock
            .Setup(handler => handler.HandleAsync(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new OkObjectResult(new object()));
        var factoryMock = new Mock<IMoviesRequestHandlerFactory>();
        factoryMock
            .Setup(factory => factory.SearchMovies("answer", 2))
            .Returns(requestHandlerMock.Object);
        var controller = new MoviesController(factoryMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };

        await controller.Search("  answer  ", 2);

        factoryMock.Verify(factory => factory.SearchMovies("answer", 2), Times.Once);
    }
}