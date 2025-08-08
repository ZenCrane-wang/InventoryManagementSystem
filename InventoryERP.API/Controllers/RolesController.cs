using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryERP.Infrastructure.Entities;
using InventoryERP.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    
    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
                return NotFound(new { message = "角色不存在" });
                
            return Ok(role);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Role role)
    {
        try
        {
            var newRole = await _roleService.CreateAsync(role);
            return CreatedAtAction(nameof(GetById), new { id = newRole.Id }, newRole);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Role role)
    {
        try
        {
            if (id != role.Id)
                return BadRequest(new { message = "角色ID不匹配" });
                
            var existingRole = await _roleService.GetByIdAsync(id);
            if (existingRole == null)
                return NotFound(new { message = "角色不存在" });
                
            var updatedRole = await _roleService.UpdateAsync(role);
            return Ok(updatedRole);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _roleService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = "角色不存在" });
                
            return Ok(new { message = "角色删除成功" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetRolePermissions(int id)
    {
        try
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
                return NotFound(new { message = "角色不存在" });
                
            var permissions = await _roleService.GetRolePermissionsAsync(id);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPost("{id}/permissions")]
    public async Task<IActionResult> AssignPermissions(int id, [FromBody] List<int> permissionIds)
    {
        try
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
                return NotFound(new { message = "角色不存在" });
                
            var result = await _roleService.AssignPermissionsToRoleAsync(id, permissionIds);
            if (!result)
                return BadRequest(new { message = "分配权限失败" });
                
            return Ok(new { message = "权限分配成功" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("{id}/users")]
    public async Task<IActionResult> GetRoleUsers(int id)
    {
        try
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
                return NotFound(new { message = "角色不存在" });
                
            var users = await _roleService.GetUsersInRoleAsync(id);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}