using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace basic_kanban_api.Services
{
    public interface IEndpointService
    {
        List<EndpointInfo> GetAllEndpoints();
    }

    public class EndpointService : IEndpointService
    {
        private readonly IEnumerable<EndpointDataSource> _endpointDataSources;

        public EndpointService(IEnumerable<EndpointDataSource> endpointDataSources)
        {
            _endpointDataSources = endpointDataSources;
        }

        public List<EndpointInfo> GetAllEndpoints()
        {
            var endpoints = new List<EndpointInfo>();

            foreach (var endpointDataSource in _endpointDataSources)
            {
                var endpointDatas = endpointDataSource.Endpoints;

                foreach (var endpoint in endpointDatas)
                {
                    if (endpoint is RouteEndpoint routeEndpoint)
                    {
                        var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

                        endpoints.Add(new EndpointInfo
                        {
                            HttpMethod = routeEndpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods.First() ?? "N/A",
                            Route = routeEndpoint.RoutePattern.RawText ?? routeEndpoint.RoutePattern.ToString(),
                            ControllerName = controllerActionDescriptor?.ControllerName ?? "N/A",
                            ActionName = controllerActionDescriptor?.ActionName ?? "N/A",
                            IsAuthorized = endpoint.Metadata.OfType<AuthorizeAttribute>().Any(),
                            Description = GetDescription(controllerActionDescriptor)
                        });
                    }
                }
            }

            return endpoints.OrderBy(x => x.Route).ToList();
        }

        private string GetDescription(ControllerActionDescriptor descriptor)
        {
            if (descriptor == null) return "N/A";

            var method = descriptor.MethodInfo;
            var attribute = method?.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();

            return attribute?.Description ?? "N/A";
        }
    }

    public class EndpointInfo
    {
        public string HttpMethod { get; set; }
        public string Route { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public bool IsAuthorized { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            var auth = IsAuthorized ? "[PROTECTED]" : "[PUBLIC]";
            return $"{auth} {HttpMethod,-6} {Route,-50} ({ControllerName}.{ActionName})";
        }
    }
}
