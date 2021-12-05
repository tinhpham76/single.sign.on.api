using SingleSignOn.Api.Authorization;
using SingleSignOn.Api.Data;
using SingleSignOn.Api.Data.Entities;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using SingleSignOn.Utilites.Constants;
using SingleSignOn.Utilites.ViewModel;
using SingleSignOn.Utilites.RequestModel;
using SingleSignOn.Utilites;

namespace SingleSignOn.Api.Controllers
{
    public class ApiScopesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ConfigurationDbContext _configurationDbContext;
        public ApiScopesController(
            ApplicationDbContext context,
            ConfigurationDbContext configurationDbContext
            )
        {
            _context = context;
            _configurationDbContext = configurationDbContext;
        }

        // Find api resource with name or display name
        [HttpGet("filter")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_VIEW)]
        public async Task<IActionResult> GetApiScopesPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _context.ApiScopes.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Name.Contains(filter) || x.DisplayName.Contains(filter));

            }
            var totalReconds = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ApiScopeQuickViewModels()
                {
                    Name = x.Name,
                    DisplayName = x.DisplayName,
                    Description = x.Description
                }).ToListAsync();

            var pagination = new Pagination<ApiScopeQuickViewModels>
            {
                Items = items,
                TotalRecords = totalReconds
            };
            return Ok(pagination);
        }

        //Post basic infor api scope
        [HttpPost]
        [ClaimRequirement(PermissionCode.SSO_SERVER_CREATE)]
        public async Task<IActionResult> PostApiScope([FromBody] ApiScopeRequestModel request)
        {
            var apiScope = await _configurationDbContext.ApiScopes.FirstOrDefaultAsync(x => x.Name == request.Name);
            if (apiScope != null)
                return BadRequest();
            var apiApiScopeRequest = new IdentityServer4.Models.ApiScope()
            {
                Name = request.Name,
                DisplayName = request.DisplayName,
                Description = request.Description,
                Enabled = request.Enabled,
                ShowInDiscoveryDocument = request.ShowInDiscoveryDocument,
                Emphasize = request.Emphasize,
                Required = request.Required,
                UserClaims = request.UserClaims
            };
            _configurationDbContext.ApiScopes.Add(apiApiScopeRequest.ToEntity());
            var result = await _configurationDbContext.SaveChangesAsync();
            if (result > 0)
                return Ok();
            return BadRequest();
        }

        // Get scope name
        [HttpGet("{apiScopeName}")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_VIEW)]
        public async Task<IActionResult> GetDetailApiScope(string apiScopeName)
        {
            var apiScope = await _context.ApiScopes.FirstOrDefaultAsync(x => x.Name == apiScopeName);
            if (apiScope == null)
            {
                return NotFound();
            }
            var claims = await _context.ApiScopeClaims
                 .Where(x => x.ScopeId == apiScope.Id)
                 .Select(x => x.Type.ToString()).ToListAsync();

            var apiScopeViewModel = new ApiScopeViewModel()
            {
                Name = apiScope.Name,
                DisplayName = apiScope.DisplayName,
                Description = apiScope.Description,
                Emphasize = apiScope.Emphasize,
                Enabled = apiScope.Enabled,
                ShowInDiscoveryDocument = apiScope.ShowInDiscoveryDocument,
                UserClaims = claims,
                Required = apiScope.Required
            };
            return Ok(apiScopeViewModel);
        }

        // Put sope
        [HttpPut("{apiScopeName}")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_UPDATE)]
        public async Task<IActionResult> PutApiScope(string apiScopeName, [FromBody] ApiScopeRequestModel request)
        {
            var apiScope = await _context.ApiScopes.FirstOrDefaultAsync(x => x.Name == apiScopeName);
            if (apiScope == null)
            {
                return NotFound();
            }
            apiScope.DisplayName = request.DisplayName;
            apiScope.Description = request.Description;
            apiScope.Enabled = request.Enabled;
            apiScope.ShowInDiscoveryDocument = request.ShowInDiscoveryDocument;
            apiScope.Required = request.Required;
            apiScope.Emphasize = request.Emphasize;
            // Claim
            var claims = await _context.ApiScopeClaims
                  .Where(x => x.ScopeId == apiScope.Id)
                  .Select(x => x.Type.ToString()).ToListAsync();
            foreach (var claim in claims)
            {
                if (!(request.UserClaims.Contains(claim)))
                {
                    var removeClaim = await _context.ApiScopeClaims.
                        FirstOrDefaultAsync(x => x.Type == claim && x.ScopeId == apiScope.Id);
                    _context.ApiScopeClaims.Remove(removeClaim);
                }
            }

            foreach (var requestClaim in request.UserClaims)
            {
                if (!claims.Contains(requestClaim))
                {
                    var addClaim = new ApiScopeClaim()
                    {
                        Type = requestClaim,
                        ScopeId = apiScope.Id
                    };
                    _context.ApiScopeClaims.Add(addClaim);
                }
            }
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

        //Delete api scope
        [HttpDelete("{apiScopeName}")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_DELETE)]
        public async Task<IActionResult> DeleteApiScope(string apiScopeName)
        {
            var apiScope = await _configurationDbContext.ApiScopes.FirstOrDefaultAsync(x => x.Name == apiScopeName);
            if (apiScope == null)
                return NotFound();
            _configurationDbContext.ApiScopes.Remove(apiScope);
            var result = await _configurationDbContext.SaveChangesAsync();
            if (result > 0)
                return Ok();
            return BadRequest();
        }

        //Get api scope properties
        [HttpGet("{apiScopeName}/properties")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_VIEW)]
        public async Task<IActionResult> GetApiScopeProperties(string apiScopeName)
        {
            var apiScope = await _context.ApiScopes.FirstOrDefaultAsync(x => x.Name == apiScopeName);
            if (apiScope == null)
            {
                return NotFound();
            }
            var query = _context.ApiScopeProperties.Where(x => x.ScopeId.Equals(apiScope.Id));
            var apiScopeProperties = await query.Select(x => new ApiScopePropertyViewModels()
            {
                Id = x.Id,
                Key = x.Key,
                Value = x.Value
            }).ToListAsync();
            return Ok(apiScopeProperties);
        }

        //Post api scope property
        [HttpPost("{apiScopeName}/properties")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_CREATE)]
        public async Task<IActionResult> PostApiScopeProperty(string apiScopeName, [FromBody] ApiScopePropertyRequestModel request)
        {
            var apiScope = await _context.ApiScopes.FirstOrDefaultAsync(x => x.Name == apiScopeName);
            if (apiScope == null)
            {
                return BadRequest();
            }
            var apiProperty = await _context.ApiScopeProperties.FirstOrDefaultAsync(x => x.Key == request.Key && x.ScopeId == apiScope.Id);
            if (apiProperty != null)
            {
                return BadRequest();
            }
            var apiPropertyRequest = new ApiScopeProperty()
            {
                Key = request.Key,
                Value = request.Value,
                ScopeId = apiScope.Id
            };
            _context.ApiScopeProperties.Add(apiPropertyRequest);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

        //Delete api scope property
        [HttpDelete("{apiScopeName}/properties/{propertyKey}")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_DELETE)]
        public async Task<IActionResult> DeleteApiScopeProperty(string apiScopeName, string propertyKey)
        {
            var apiScope = await _context.ApiScopes.FirstOrDefaultAsync(x => x.Name == apiScopeName);
            if (apiScope == null)
                return NotFound();
            var apiScopeProperty = await _context.ApiScopeProperties.FirstOrDefaultAsync(x => x.ScopeId == apiScope.Id && x.Key == propertyKey);
            if (apiScopeProperty == null)
                return NotFound();
            _context.ApiScopeProperties.Remove(apiScopeProperty);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

    }
}
