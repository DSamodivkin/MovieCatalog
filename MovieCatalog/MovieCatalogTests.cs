using MovieCatalog.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;


namespace MovieCatalogTests
{
    [TestFixture]
    public class IdeaCenterTests
    {
        private RestClient _client;

        private static string movieId;

        private const string baseUrl = "http://144.91.123.158:5000";
        private const string staticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiIzODI2OGVlMC01NzZlLTQ2NWMtOTJiYy0yZWI5YjkwNTUwZTMiLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjA2OjE3IiwiVXNlcklkIjoiNzhjZGQ0Y2YtMjRkZC00MGFkLTYyMWItMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJ0ZXN0VXNlckBleGFtcGxlLmNvbSIsIlVzZXJOYW1lIjoiVGVzdFVzZXI5OTkiLCJleHAiOjE3NzY1MTM5NzcsImlzcyI6Ik1vdmllQ2F0YWxvZ19BcHBfU29mdFVuaSIsImF1ZCI6Ik1vdmllQ2F0YWxvZ19XZWJBUElfU29mdFVuaSJ9.e4YG5Z6wkwQiqqjW-I_Xtz0gs5oHm7O_vGVr7N41fHo";
        private const string loginEmail = "testUser@example.com";
        private const string loginPassword = "123456";

        [OneTimeSetUp]

        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(staticToken))
            {
                jwtToken = staticToken;
            }
            else
            {
                jwtToken = GetJwtToken(loginEmail, loginPassword);
            }

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this._client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not Found in th response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response:{response.Content}");
            }

        }

        [Order(1)]
        [Test]

        public void CreateMovie_withReuiredFields_ReturnsSuccess()
        {
            var movieData = new 
            {
                title = "Test Movie",
                description = "This is a test movie.",
                posterUrl = "",
                trailerLink = "",
                isWatched = true
            };
            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);

            var response = this._client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(readyResponse.Movie, Is.Not.Null);
            Assert.That(readyResponse.Movie.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie created successfully!"));

            movieId = readyResponse.Movie.Id;
        }
       
        [Order(2)]
        [Test]

        public void EditMovie_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Movie/Edit/", Method.Put);
            request.AddJsonBody(new
            {
                title = "Edited Test Movie",
                description = "This is an edited test movie.",
                posterUrl = "",
                trailerLink = "",
                isWatched = true
            });
            request.AddQueryParameter("movieid", movieId);

            var response = this._client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]

        public void GetAllMovies_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);

            var response = this._client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<List<MovieDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(readyResponse, Is.Not.Null);
            Assert.That(readyResponse, Is.Not.Empty);
            Assert.That(readyResponse.Count, Is.GreaterThanOrEqualTo(1));
        }

        [Order(4)]
        [Test]

        public void DeleteMovie_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Movie/Delete/", Method.Delete);
            request.AddQueryParameter("movieid", movieId);

            var response = this._client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]

        public void CreateMovie_WithoutRequiredFields_ReturnsBadRequest()
        {
            var movieData = new
            {
                title = "",
                description = "",
                posterUrl = "",
                trailerLink = "",
                isWatched = true
            };
            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);

            var response = this._client.Execute(request);
           
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]

        public void EditNonExistingMovie_ShouldReturnBadRequest()
        {
            string nonExistingMovieId = "9999999999";
            var request = new RestRequest($"/api/Movie/Edit/", Method.Put);
            request.AddJsonBody(new
            {
                title = "Non existing Movie",
                description = "Non existing Movie",
                posterUrl = "",
                trailerLink = "",
                isWatched = true
            });
            request.AddQueryParameter("movieid", nonExistingMovieId);

            var response = this._client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]

        public void DeleteNonExistingMovie_ShouldReturnBadRequest()
        {
            string nonExistingMovieId = "9999999999";
            var request = new RestRequest("/api/Movie/Delete/", Method.Delete);
            request.AddQueryParameter("movieid", nonExistingMovieId);

            var response = this._client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }
        [OneTimeTearDown]

        public void TearDown()
        {
            this._client.Dispose();
        }
    }
}
