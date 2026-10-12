namespace MovieSearchCase.WebApi.Models.Movies;

public record MovieSearchResponse
{
    public int Page { get; init; }

    public int TotalPages { get; init; }

    public int TotalResults { get; init; }

    public required IReadOnlyList<Movie> Results { get; init; }
}