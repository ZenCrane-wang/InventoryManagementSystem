using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryERP.Infrastructure.Entities;

namespace InventoryERP.Infrastructure.Services;

public interface IRoleService
{
    // 获取角色
    Task<Role?> GetByIdAsync(int id);
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllAsync();
    
    // 创建角色
    Task<Role> CreateAsync(Role role);
    
    // 更新角色
    Task<Role> UpdateAsync(Role role);
    
    // 删除角色
    Task<bool> DeleteAsync(int id);
    
    // 分配权限给角色
    Task<bool> AssignPermissionsToRoleAsync(int roleId, IEnumerable<int> permissionIds);
    
    // 获取角色的权限
    Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId);
    
    // 获取拥有特定角色的用户
    Task<IEnumerable<User>> GetUsersInRoleAsync(int roleId);
}