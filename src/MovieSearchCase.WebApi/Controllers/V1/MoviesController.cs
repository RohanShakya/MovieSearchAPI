using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MovieSearchCase.Domain.Exceptions;
using MovieSearchCase.WebApi.Application.Versioning;
using MovieSearchCase.WebApi.Handlers.Movies;
using Swashbuckle.AspNetCore.Annotations;

namespace MovieSearchCase.WebApi.Controllers.V1;

[ApiVersion("1.0")]
[VersionedRoute]
public class MoviesController : ApiControllerBase
{
    private readonly IMoviesRequestHandlerFactory _requestHandlerFactory;

    public MoviesController(IMoviesRequestHandlerFactory requestHandlerFactory)
    {
        _requestHandlerFactory = requestHandlerFactory;
    }

    /// <summary>
    /// Returns this week's TMDB-backed trending movies.
    /// </summary>
    [HttpGet("trending")]
    [SwaggerOperation(Summary = "Get trending movies", OperationId = "GetTrendingMovies")]
    [SwaggerResponse(StatusCodes.Status200OK, "This week's trending movies", typeof(IEnumerable<Models.Movies.Movie>))]
    public async Task<IActionResult> GetTrending() =>
        await _requestHandlerFactory.GetTrendingMovies().HandleAsync(Request);

    /// <summary>
    /// Searches TMDB movies and returns one page of results.
    /// </summary>
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Search movies", OperationId = "SearchMovies")]
    [SwaggerResponse(StatusCodes.Status200OK, "Paginated movie search results", typeof(Models.Movies.MovieSearchResponse))]
    public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] int page = 1)
    {
        var normalizedQuery = query?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            throw new MovieException(
                MovieException.ExceptionTitle,
                ErrorType.InvalidRequest,
                "The 'query' parameter is required.");
        }

        if (page is < 1 or > 500)
        {
            throw new MovieException(
                MovieException.ExceptionTitle,
                ErrorType.InvalidRequest,
                "The 'page' parameter must be between 1 and 500.");
        }

        return await _requestHandlerFactory.SearchMovies(normalizedQuery, page).HandleAsync(Request);
    }
}
