using Carter;

namespace carter01test
{
    public class HomeModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/", () => "Hello there ;)");
        }
    }

}
