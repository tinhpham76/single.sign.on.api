using SingleSignOn.Api.Authorization;
using SingleSignOn.Api.Data;
using SingleSignOn.Api.Data.Entities;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using SingleSignOn.Utilites.ViewModel;
using SingleSignOn.Utilites.Constants;
using SingleSignOn.Utilites;
using SingleSignOn.Utilites.RequestModel;

namespace SingleSignOn.Api.Controllers
{
    public class ApiResourcesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ConfigurationDbContext _configurationDbContext;
        public ApiResourcesController(
            ConfigurationDbContext configurationDbContext,
            ApplicationDbContext context
            )
        {
            _context = context;
            _configurationDbContext = configurationDbContext;
        }

        // Get all api scope
        [HttpGet]
        [ClaimRequirement(PermissionCode.SSO_SERVER_VIEW)]
        public async Task<IActionResult> GetAllApiScopes()
        {
            var apiScopes = await _context.ApiScopes.Select(x => x.Name.ToString()).ToListAsync();
            return Ok(apiScopes);
        }

        // Find api resource with name or display name
        [HttpGet("filter")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_VIEW)]
        public async Task<IActionResult> GetApiResourcesPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _context.ApiResources.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Name.Contains(filter) || x.DisplayName.Contains(filter));

            }
            var totalReconds = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ApiResourceQuickViewModels()
                {
                    Name = x.Name,
                    DisplayName = x.DisplayName,
                    Description = x.Description
                }).ToListAsync();

            var pagination = new Pagination<ApiResourceQuickViewModels>
            {
                Items = items,
                TotalRecords = totalReconds
            };
            return Ok(pagination);
        }

        //Post api resource
        [HttpPost]
        [ClaimRequirement(PermissionCode.SSO_SERVER_CREATE)]
        public async Task<IActionResult> PostApiResource([FromBody] ApiResourceRequestModel request)
        {
            var apiResource = await _configurationDbContext.ApiResources.FirstOrDefaultAsync(x => x.Name == request.Name);
            if (apiResource != null)
                return BadRequest();
            var apiResourceRequest = new IdentityServer4.Models.ApiResource()
            {
                Name = request.Name,
                DisplayName = request.DisplayName,
                Description = request.Description,
                Enabled = request.Enabled,
                AllowedAccessTokenSigningAlgorithms = { request.AllowedAccessTokenSigningAlgorithms },
                ShowInDiscoveryDocument = request.ShowInDiscoveryDocument,
                UserClaims = request.UserClaims,
                Scopes = request.Scopes
            };
            _configurationDbContext.ApiResources.Add(apiResourceRequest.ToEntity());
            var result = await _configurationDbContext.SaveChangesAsync();
            if (result > 0)
                return Ok();
            return BadRequest();
        }

        // Get api resource detail
        [HttpGet("{apiResourceName}")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_VIEW)]
        public async Task<IActionResult> GetDetailApiResource(string apiResourceName)
        {
            var apiResource = await _context.ApiResources.FirstOrDefaultAsync(x => x.Name == apiResourceName);
            if (apiResource == null)
            {
                return NotFound();
            }
            var claims = await _context.ApiResourceClaims
                 .Where(x => x.ApiResourceId == apiResource.Id)
                 .Select(x => x.Type.ToString()).ToListAsync();
            var scopes = await _context.ApiResourceScopes
                 .Where(x => x.ApiResourceId == apiResource.Id)
                 .Select(x => x.Scope.ToString()).ToListAsync();
            var apiResourceViewModel = new ApiResourceViewModel()
            {
                Name = apiResource.Name,
                DisplayName = apiResource.DisplayName,
                Description = apiResource.Description,
                AllowedAccessTokenSigningAlgorithms = apiResource.AllowedAccessTokenSigningAlgorithms,
                Enabled = apiResource.Enabled,
                ShowInDiscoveryDocument = apiResource.ShowInDiscoveryDocument,
                UserClaims = claims,
                Scopes = scopes
            };
            return Ok(apiResourceViewModel);
        }

        // Put api resource
        [HttpPut("{apiResourceName}")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_UPDATE)]
        public async Task<IActionResult> PutApiResource(string apiResourceName, [FromBody] ApiResourceRequestModel request)
        {
            var apiResource = await _context.ApiResources.FirstOrDefaultAsync(x => x.Name == apiResourceName);
            if (apiResource == null)
            {
                return NotFound();
            }
            apiResource.DisplayName = request.DisplayName;
            apiResource.Description = request.Description;
            apiResource.Enabled = request.Enabled;
            apiResource.ShowInDiscoveryDocument = request.ShowInDiscoveryDocument;
            apiResource.AllowedAccessTokenSigningAlgorithms = request.AllowedAccessTokenSigningAlgorithms;
            apiResource.Updated = DateTime.UtcNow;

            // Claim
            var claims = await _context.ApiResourceClaims
                  .Where(x => x.ApiResourceId == apiResource.Id)
                  .Select(x => x.Type.ToString()).ToListAsync();
            foreach (var claim in claims)
            {
                if (!(request.UserClaims.Contains(claim)))
                {
                    var removeClaim = await _context.ApiResourceClaims.
                        FirstOrDefaultAsync(x => x.Type == claim && x.ApiResourceId == apiResource.Id);
                    _context.ApiResourceClaims.Remove(removeClaim);
                }
            }

            foreach (var requestClaim in request.UserClaims)
            {
                if (!claims.Contains(requestClaim))
                {
                    var addClaim = new ApiResourceClaim()
                    {
                        Type = requestClaim,
                        ApiResourceId = apiResource.Id
                    };
                    _context.ApiResourceClaims.Add(addClaim);
                }
            }

            // Scope
            var scopes = await _context.ApiResourceScopes
                  .Where(x => x.ApiResourceId == apiResource.Id)
                  .Select(x => x.Scope.ToString()).ToListAsync();
            foreach (var scope in scopes)
            {
                if (!(request.Scopes.Contains(scope)))
                {
                    var removeScope = await _context.ApiResourceScopes.
                        FirstOrDefaultAsync(x => x.Scope == scope && x.ApiResourceId == apiResource.Id);
                    _context.ApiResourceScopes.Remove(removeScope);
                }
            }

            foreach (var requestScope in request.Scopes)
            {
                if (!scopes.Contains(requestScope))
                {
                    var addScope = new ApiResourceScope()
                    {
                        Scope = requestScope,
                        ApiResourceId = apiResource.Id
                    };
                    _context.ApiResourceScopes.Add(addScope);
                }
            }

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

        //Delete api resource
        [HttpDelete("{apiResourceName}")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_DELETE)]
        public async Task<IActionResult> DeleteApiResource(string apiResourceName)
        {
            var apiResource = await _configurationDbContext.ApiResources.FirstOrDefaultAsync(x => x.Name == apiResourceName);
            if (apiResource == null)
                return NotFound();
            _configurationDbContext.ApiResources.Remove(apiResource);
            var result = await _configurationDbContext.SaveChangesAsync();
            if (result > 0)
                return Ok();
            return BadRequest();
        }

        //Get api resource secrets
        [HttpGet("{apiResourceName}/secrets")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_VIEW)]
        public async Task<IActionResult> GetApiResourceSecrets(string apiResourceName)
        {
            var apiResource = await _context.ApiResources.FirstOrDefaultAsync(x => x.Name == apiResourceName);
            if (apiResource == null)
            {
                return NotFound();
            }
            var query = _context.ApiResourceSecrets.Where(x => x.ApiResourceId.Equals(apiResource.Id));
            var apiSecrets = await query.Select(x => new ApiResourceSecretViewModels()
            {
                Id = x.Id,
                Value = x.Value,
                Type = x.Type,
                Expiration = x.Expiration.ToString().Substring(0, 10),
                Description = x.Description,
            }).ToListAsync();
            return Ok(apiSecrets);
        }

        //Post api resource secret
        [HttpPost("{apiResourceName}/secrets")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_CREATE)]
        public async Task<IActionResult> PostApiResourceSecret(string apiResourceName, [FromBody] ApiResourceSecretRequestModel request)
        {
            var apiResource = await _context.ApiResources.FirstOrDefaultAsync(x => x.Name == apiResourceName);
            if (apiResource == null)
            {
                return NotFound();
            }
            var apiSecret = new ApiResourceSecret()
            {
                Type = "SharedSecret",
                Value = request.Value.ToSha256(),
                Description = request.Description,
                ApiResourceId = apiResource.Id,
                Expiration = DateTime.Parse(request.Expiration),
                Created = DateTime.UtcNow
            };
            _context.ApiResourceSecrets.Add(apiSecret);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

        //Delete api resource secret
        [HttpDelete("{apiResourceName}/secrets/{secretId}")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_DELETE)]
        public async Task<IActionResult> DeleteApiResourceSecret(string apiResourceName, int secretId)
        {
            var apiResource = await _context.ApiResources.FirstOrDefaultAsync(x => x.Name == apiResourceName);
            if (apiResource == null)
                return NotFound();
            var apiSecret = await _context.ApiResourceSecrets.FirstOrDefaultAsync(x => x.ApiResourceId == apiResource.Id && x.Id == secretId);
            if (apiSecret == null)
                return NotFound();
            _context.ApiResourceSecrets.Remove(apiSecret);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

        //Get api resource properties
        [HttpGet("{apiResourceName}/properties")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_VIEW)]
        public async Task<IActionResult> GetApiResourceProperties(string apiResourceName)
        {
            var apiResource = await _context.ApiResources.FirstOrDefaultAsync(x => x.Name == apiResourceName);
            if (apiResource == null)
            {
                return NotFound();
            }
            var query = _context.ApiResourceProperties.Where(x => x.ApiResourceId.Equals(apiResource.Id));
            var apiResourceProperties = await query.Select(x => new ApiResourcePropertyViewModels()
            {
                Id = x.Id,
                Key = x.Key,
                Value = x.Value
            }).ToListAsync();
            return Ok(apiResourceProperties);
        }

        //Post api resource property
        [HttpPost("{apiResourceName}/properties")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_CREATE)]
        public async Task<IActionResult> PostApiResourceProperty(string apiResourceName, [FromBody] ApiResourcePropertyRequestModel request)
        {
            var apiResource = await _context.ApiResources.FirstOrDefaultAsync(x => x.Name == apiResourceName);
            if (apiResource == null)
            {
                return BadRequest();
            }
            var apiProperty = await _context.ApiResourceProperties.
                FirstOrDefaultAsync(x => x.Key == request.Key && x.ApiResourceId == apiResource.Id);
            if (apiProperty != null)
            {
                return BadRequest();
            }
            var apiPropertyRequest = new ApiResourceProperty()
            {
                Key = request.Key,
                Value = request.Value,
                ApiResourceId = apiResource.Id
            };
            _context.ApiResourceProperties.Add(apiPropertyRequest);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

        //Delete api resource property
        [HttpDelete("{apiResourceName}/properties/{propertyKey}")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_DELETE)]
        public async Task<IActionResult> DeleteApiResourceProperty(string apiResourceName, string propertyKey)
        {
            var apiResource = await _context.ApiResources.FirstOrDefaultAsync(x => x.Name == apiResourceName);
            if (apiResource == null)
                return NotFound();
            var apiResourceProperty = await _context.ApiResourceProperties.FirstOrDefaultAsync(x => x.ApiResourceId == apiResource.Id && x.Key == propertyKey);
            if (apiResourceProperty == null)
                return NotFound();
            _context.ApiResourceProperties.Remove(apiResourceProperty);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

    }
}
