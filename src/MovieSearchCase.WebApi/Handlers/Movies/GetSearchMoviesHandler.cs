using Microsoft.AspNetCore.Mvc;
using MovieSearchCase.Domain.Interfaces.Services;
using MovieSearchCase.Shared;
using MovieSearchCase.WebApi.Mappers;

namespace MovieSearchCase.WebApi.Handlers.Movies;

public class GetSearchMoviesHandler : IRequestHandlerAsync
{
    private readonly IMovieService _movieService;
    private readonly string _query;
    private readonly int _page;

    public GetSearchMoviesHandler(IMovieService movieService, string query, int page)
    {
        _movieService = movieService;
        _query = query;
        _page = page;
    }

    public async Task<IActionResult> HandleAsync(HttpRequest request)
    {
        var searchResult = await _movieService.SearchAsync(
            _query,
            _page,
            request.HttpContext.RequestAborted);

        return new OkObjectResult(new Models.Movies.MovieSearchResponse
        {
            Page = searchResult.Page,
            TotalPages = searchResult.TotalPages,
            TotalResults = searchResult.TotalResults,
            Results = searchResult.Results.Select(movie => movie.ToApiModel()).ToList(),
        });
    }
}