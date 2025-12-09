using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using basic_kanban_api.Services;

namespace basic_kanban_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EndpointsController : ControllerBase
    {
        private readonly IEndpointService _endpointService;

        public EndpointsController(IEndpointService endpointService)
        {
            _endpointService = endpointService;
        }

        /// <summary>
        /// Retorna uma lista de todos os endpoints disponíveis na API
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<dynamic>> GetAllEndpoints()
        {
            var endpoints = _endpointService.GetAllEndpoints();

            return Ok(new
            {
                totalEndpoints = endpoints.Count,
                endpoints = endpoints.Select(e => new
                {
                    e.HttpMethod,
                    e.Route,
                    e.ControllerName,
                    e.ActionName,
                    e.IsAuthorized,
                    e.Description
                }).ToList()
            });
        }

        /// <summary>
        /// Retorna apenas os endpoints públicos (sem autenticação)
        /// </summary>
        [HttpGet("public")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<dynamic>> GetPublicEndpoints()
        {
            var endpoints = _endpointService.GetAllEndpoints()
                .Where(e => !e.IsAuthorized)
                .ToList();

            return Ok(new
            {
                totalPublicEndpoints = endpoints.Count,
                endpoints = endpoints.Select(e => new
                {
                    e.HttpMethod,
                    e.Route,
                    e.ControllerName,
                    e.ActionName
                }).ToList()
            });
        }

        /// <summary>
        /// Retorna apenas os endpoints protegidos (requerem autenticação)
        /// </summary>
        [HttpGet("protected")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<dynamic>> GetProtectedEndpoints()
        {
            var endpoints = _endpointService.GetAllEndpoints()
                .Where(e => e.IsAuthorized)
                .ToList();

            return Ok(new
            {
                totalProtectedEndpoints = endpoints.Count,
                endpoints = endpoints.Select(e => new
                {
                    e.HttpMethod,
                    e.Route,
                    e.ControllerName,
                    e.ActionName
                }).ToList()
            });
        }
    }
}
