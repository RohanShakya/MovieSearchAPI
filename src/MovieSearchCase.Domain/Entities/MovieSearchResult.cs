namespace MovieSearchCase.Domain.Entities;

public sealed record MovieSearchResult(
    int Page,
    int TotalPages,
    int TotalResults,
    IReadOnlyList<Movie> Results);