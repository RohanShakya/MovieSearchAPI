# Movie Search API

A .NET 10 Web API for discovering movies through [The Movie Database (TMDB)](https://www.themoviedb.org/). The API exposes trending movies and paginated movie search results, with its own response models and consistent problem-details errors.

## Project structure

- `MovieSearchCase.WebApi` — versioned HTTP endpoints, request handlers, API models, Swagger, and error handling.
- `MovieSearchCase.Domain` — movie and search-result types, plus service and client interfaces.
- `MovieSearchCase.Infrastructure` — TMDB HTTP client, TMDB response mapping, and movie service implementation.
- `MovieSearchCase.Shared` — shared asynchronous request-handler contract.
- `tests` — Web API handler/controller tests and Infrastructure service/client tests.

Requests flow from the controller through a handler and movie service to the TMDB client. TMDB response models are mapped into domain types, then mapped into API response models.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A TMDB API Read Access Token. Create a TMDB account and generate a token using the [TMDB getting started guide](https://developer.themoviedb.org/docs/getting-started).

## Configure and run (Windows PowerShell)

From the repository root, copy the development settings template:

```powershell
Copy-Item src/MovieSearchCase.WebApi/appsettings.Development.example.json `
  src/MovieSearchCase.WebApi/appsettings.Development.local.json
```

Open `src/MovieSearchCase.WebApi/appsettings.Development.local.json` and replace the `TmdbOptions.AccessToken` placeholder with your TMDB token. This local file is gitignored; do not commit your token.

Run the API with the configured HTTP launch profile:

```powershell
dotnet run --project src/MovieSearchCase.WebApi --launch-profile http
```

The API listens at `http://localhost:5080`. In Development, Swagger UI is available at `http://localhost:5080/swagger`.

## API endpoints

Base URL: `http://localhost:5080/api/v1`

### Get trending movies

```http
GET /movies/trending
```

Returns an array of movie objects.

### Search movies

```http
GET /movies/search?query=batman&page=1
```

`query` is required and cannot be blank. `page` is optional, defaults to `1`, and must be between `1` and `500`.

Example response:

```json
{
  "page": 1,
  "totalPages": 12,
  "totalResults": 231,
  "results": [
    {
      "id": 268,
      "title": "Batman",
      "overview": "A masked vigilante protects Gotham City.",
      "posterPath": "/poster.jpg",
      "backdropPath": "/backdrop.jpg",
      "voteAverage": 7.2,
      "releaseDate": "1989-06-21"
    }
  ]
}
```

When no movies match, search returns `200 OK` with zero totals and an empty `results` array. Movie fields use camelCase; `overview`, `posterPath`, `backdropPath`, and `releaseDate` may be `null`. Dates use `YYYY-MM-DD`.

Invalid query or page values return `400 Bad Request`. TMDB connectivity or response failures return `503 Service Unavailable`. Errors use the `application/problem+json` format.

## Run tests

From the repository root:

```powershell
dotnet test MovieSearchCase.slnx
```
