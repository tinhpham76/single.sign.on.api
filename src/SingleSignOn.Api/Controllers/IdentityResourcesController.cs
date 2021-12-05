using SingleSignOn.Api.Authorization;
using SingleSignOn.Api.Data;
using SingleSignOn.Api.Data.Entities;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using SingleSignOn.Utilites.Constants;
using SingleSignOn.Utilites.ViewModel;
using SingleSignOn.Utilites;
using SingleSignOn.Utilites.RequestModel;

namespace SingleSignOn.Api.Controllers
{
    public class IdentityResourcesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ConfigurationDbContext _configurationDbContext;
        public IdentityResourcesController(
            ConfigurationDbContext configurationDbContext,
            ApplicationDbContext context)
        {
            _context = context;
            _configurationDbContext = configurationDbContext;
        }

        [HttpGet("filter")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_VIEW)]
        public async Task<IActionResult> GetIdentityResourcesPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _context.IdentityResources.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Name.Contains(filter) || x.DisplayName.Contains(filter));

            }
            var totalReconds = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new IdentityResourceQuickViewModels()
                {
                    Name = x.Name,
                    DisplayName = x.DisplayName,
                    Description = x.Description
                }).ToListAsync();

            var pagination = new Pagination<IdentityResourceQuickViewModels>
            {
                Items = items,
                TotalRecords = totalReconds
            };
            return Ok(pagination);
        }

        [HttpPost]
        [ClaimRequirement(PermissionCode.SSO_SERVER_CREATE)]
        public async Task<IActionResult> PostIdentityResource([FromBody] IdentityResourceRequestModel request)
        {
            var identityResource = await _configurationDbContext.IdentityResources.FirstOrDefaultAsync(x => x.Name == request.Name);
            if (identityResource != null)
            {
                return BadRequest();
            }
            var identityResourceRequest = new IdentityServer4.Models.IdentityResource()
            {
                Name = request.Name,
                DisplayName = request.DisplayName,
                Description = request.Description,
                Enabled = request.Enabled,
                ShowInDiscoveryDocument = request.ShowInDiscoveryDocument,
                Required = request.Required,
                Emphasize = request.Emphasize,
                UserClaims = request.UserClaims
            };
            _configurationDbContext.IdentityResources.Add(identityResourceRequest.ToEntity());
            var result = await _configurationDbContext.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

        //Get detail info identity resource
        [HttpGet("{identityResourceName}")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_VIEW)]
        public async Task<IActionResult> GetIdentityResource(string identityResourceName)
        {
            var identityResource = await _context.IdentityResources.FirstOrDefaultAsync(x => x.Name == identityResourceName);
            if (identityResource == null)
                return NotFound();
            var claims = await _context.IdentityResourceClaims
                .Where(x => x.IdentityResourceId == identityResource.Id)
                .Select(x => x.Type.ToString()).ToListAsync();
            var identityResourceViewModel = new IdentityResourceViewModel()
            {
                Name = identityResource.Name,
                DisplayName = identityResource.DisplayName,
                Description = identityResource.Description,
                Enabled = identityResource.Enabled,
                ShowInDiscoveryDocument = identityResource.ShowInDiscoveryDocument,
                Required = identityResource.Required,
                Emphasize = identityResource.Emphasize,
                UserClaims = claims
            };
            return Ok(identityResourceViewModel);
        }

        // Put identity
        [HttpPut("{identityResourceName}")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_VIEW)]
        public async Task<IActionResult> PutApiResource(string identityResourceName, [FromBody] IdentityResourceRequestModel request)
        {
            var identityResource = await _context.IdentityResources.FirstOrDefaultAsync(x => x.Name == identityResourceName);
            if (identityResource == null)
            {
                return NotFound();
            }
            identityResource.DisplayName = request.DisplayName;
            identityResource.Description = request.Description;
            identityResource.Enabled = request.Enabled;
            identityResource.ShowInDiscoveryDocument = request.ShowInDiscoveryDocument;
            identityResource.Required = request.Required;
            identityResource.Emphasize = request.Emphasize;
            identityResource.Updated = DateTime.UtcNow;

            // Claim
            var claims = await _context.IdentityResourceClaims
                  .Where(x => x.IdentityResourceId == identityResource.Id)
                  .Select(x => x.Type.ToString()).ToListAsync();
            foreach (var claim in claims)
            {
                if (!(request.UserClaims.Contains(claim)))
                {
                    var removeClaim = await _context.IdentityResourceClaims.FirstOrDefaultAsync(x => x.Type == claim);
                    _context.IdentityResourceClaims.Remove(removeClaim);
                }
            }

            foreach (var requestClaim in request.UserClaims)
            {
                if (!claims.Contains(requestClaim))
                {
                    var addClaim = new IdentityResourceClaim()
                    {
                        Type = requestClaim,
                        IdentityResourceId = identityResource.Id
                    };
                    _context.IdentityResourceClaims.Add(addClaim);
                }
            }
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

        //Get identity resource properties
        [HttpGet("{identityResourceName}/properties")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_VIEW)]
        public async Task<IActionResult> GetIdentityResourceProperties(string identityResourceName)
        {
            var identityResource = await _context.IdentityResources.FirstOrDefaultAsync(x => x.Name == identityResourceName);
            if (identityResource == null)
            {
                return NotFound();
            }
            var query = _context.IdentityResourceProperties.Where(x => x.IdentityResourceId.Equals(identityResource.Id));
            var identityResourceProperties = await query.Select(x => new IdentityResourcePropertyViewModels()
            {
                Id = x.Id,
                Key = x.Key,
                Value = x.Value
            }).ToListAsync();
            return Ok(identityResourceProperties);
        }

        //Post identity resource property
        [HttpPost("{identityResourceName}/properties")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_CREATE)]
        public async Task<IActionResult> PostApiResourceProperty(string identityResourceName, [FromBody] IdentityResourcePropertyRequestModel request)
        {
            var identityResource = await _context.IdentityResources.FirstOrDefaultAsync(x => x.Name == identityResourceName);
            if (identityResource == null)
            {
                return BadRequest();
            }
            var identityProperty = await _context.IdentityResourceProperties.
                FirstOrDefaultAsync(x => x.Key == request.Key && x.IdentityResourceId == identityResource.Id);
            if (identityProperty != null)
            {
                return BadRequest();
            }
            var identityPropertyRequest = new IdentityResourceProperty()
            {
                Key = request.Key,
                Value = request.Value,
                IdentityResourceId = identityResource.Id
            };
            _context.IdentityResourceProperties.Add(identityPropertyRequest);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

        //Delete api resource property
        [HttpDelete("{identityResourceName}/properties/{propertyKey}")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_DELETE)]
        public async Task<IActionResult> DeleteIdentityResourceProperty(string identityResourceName, string propertyKey)
        {
            var identityResource = await _context.IdentityResources.FirstOrDefaultAsync(x => x.Name == identityResourceName);
            if (identityResource == null)
                return NotFound();
            var identityResourceProperty = await _context.IdentityResourceProperties.
                FirstOrDefaultAsync(x => x.IdentityResourceId == identityResource.Id && x.Key == propertyKey);
            if (identityResourceProperty == null)
                return NotFound();
            _context.IdentityResourceProperties.Remove(identityResourceProperty);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

        //Delete Identity Resource
        [HttpDelete("{identityResourceName}")]
        [ClaimRequirement(PermissionCode.SSO_SERVER_DELETE)]
        public async Task<IActionResult> DeleteIdentityResource(string identityResourceName)
        {
            var identityResource = await _configurationDbContext.IdentityResources.FirstOrDefaultAsync(x => x.Name == identityResourceName);
            if (identityResource == null)
                return NotFound();
            _configurationDbContext.IdentityResources.Remove(identityResource);
            var result = await _configurationDbContext.SaveChangesAsync();
            if (result > 0)
                return Ok();
            return BadRequest();
        }
    }
}
