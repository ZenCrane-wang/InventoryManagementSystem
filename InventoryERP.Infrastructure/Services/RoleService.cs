using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryERP.Infrastructure.Entities;
using InventoryERP.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventoryERP.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    
    public RoleService(AppDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Role?> GetByIdAsync(int id)
    {
        return await _context.Roles.FindAsync(id);
    }
    
    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
    }
    
    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles.ToListAsync();
    }
    
    public async Task<Role> CreateAsync(Role role)
    {
        // 检查角色名是否已存在
        if (await _context.Roles.AnyAsync(r => r.Name == role.Name))
            throw new System.Exception("角色名已存在");
            
        _context.Roles.Add(role);
        await _unitOfWork.SaveChangesAsync();
        return role;
    }
    
    public async Task<Role> UpdateAsync(Role role)
    {
        // 检查角色名是否已被其他角色使用
        if (await _context.Roles.AnyAsync(r => r.Name == role.Name && r.Id != role.Id))
            throw new System.Exception("角色名已被其他角色使用");
            
        _context.Roles.Update(role);
        await _unitOfWork.SaveChangesAsync();
        return role;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
            return false;
            
        // 检查是否有用户使用此角色
        var usersWithRole = await _context.UserRoles.AnyAsync(ur => ur.RoleId == id);
        if (usersWithRole)
            throw new System.Exception("无法删除角色，因为有用户正在使用此角色");
            
        _context.Roles.Remove(role);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> AssignPermissionsToRoleAsync(int roleId, IEnumerable<int> permissionIds)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
            return false;
            
        // 获取当前角色的权限
        var currentRolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
            
        // 移除不在新权限列表中的权限
        foreach (var rolePermission in currentRolePermissions)
        {
            if (!permissionIds.Contains(rolePermission.PermissionId))
            {
                _context.RolePermissions.Remove(rolePermission);
            }
        }
        
        // 添加新权限
        foreach (var permissionId in permissionIds)
        {
            if (!currentRolePermissions.Any(rp => rp.PermissionId == permissionId))
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });
            }
        }
        
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
    
    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<User>> GetUsersInRoleAsync(int roleId)
    {
        return await _context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.User)
            .ToListAsync();
    }
}