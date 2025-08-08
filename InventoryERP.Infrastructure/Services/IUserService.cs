using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryERP.Infrastructure.Entities;
using InventoryERP.Infrastructure.Models;

namespace InventoryERP.Infrastructure.Services;

public interface IUserService
{
    // 用户认证
    Task<AuthResponse?> AuthenticateAsync(string username, string password);
    
    // 用户注册
    Task<User> RegisterAsync(RegisterRequest model);
    
    // 修改密码
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    
    // 重置密码（管理员功能）
    Task<string> ResetPasswordAsync(int userId);
    
    // 获取用户信息
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    
    // 获取用户列表
    Task<IEnumerable<User>> GetAllAsync();
    
    // 更新用户信息
    Task<User> UpdateAsync(User user);
    
    // 删除用户
    Task<bool> DeleteAsync(int id);
    
    // 分配角色给用户
    Task<bool> AssignRolesToUserAsync(int userId, IEnumerable<int> roleIds);
    
    // 获取用户的角色
    Task<IEnumerable<Role>> GetUserRolesAsync(int userId);
    
    // 获取用户的权限
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId);
}