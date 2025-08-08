using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryERP.Infrastructure.Entities;
using InventoryERP.Infrastructure.Models;
using InventoryERP.Infrastructure.Repositories;
using InventoryERP.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InventoryERP.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    
    public UserService(AppDbContext context, IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = new JwtTokenGenerator(configuration);
    }
    
    public async Task<AuthResponse?> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);
            
        if (user == null || !user.IsActive)
            return null;
            
        // 验证密码
        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            return null;
            
        // 更新最后登录时间
        user.LastLogin = DateTime.Now;
        await _unitOfWork.SaveChangesAsync();
        
        // 获取用户角色和权限
        var roles = await GetUserRolesAsync(user.Id);
        var permissions = await GetUserPermissionsAsync(user.Id);
        
        // 生成JWT令牌
        var token = _jwtTokenGenerator.GenerateToken(
            user,
            roles.Select(r => r.Name).ToList(),
            permissions.Select(p => $"{p.Module}.{p.Action}").ToList()
        );
        
        return new AuthResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Token = token,
            Roles = roles.Select(r => r.Name).ToList(),
            Permissions = permissions.Select(p => $"{p.Module}.{p.Action}").ToList()
        };
    }
    
    public async Task<User> RegisterAsync(RegisterRequest model)
    {
        // 检查用户名和邮箱是否已存在
        if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            throw new Exception("用户名已被使用");
            
        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            throw new Exception("邮箱已被使用");
            
        // 生成密码盐和哈希
        var salt = PasswordHasher.GenerateSalt();
        var passwordHash = PasswordHasher.HashPassword(model.Password, salt);
        
        // 创建新用户
        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = passwordHash,
            PasswordSalt = salt,
            PhoneNumber = model.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        
        _context.Users.Add(user);
        await _unitOfWork.SaveChangesAsync();
        
        return user;
    }
    
    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;
            
        // 验证当前密码
        if (!PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
            return false;
            
        // 更新密码
        var salt = PasswordHasher.GenerateSalt();
        var passwordHash = PasswordHasher.HashPassword(newPassword, salt);
        
        user.PasswordHash = passwordHash;
        user.PasswordSalt = salt;
        
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
    
    public async Task<string> ResetPasswordAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("用户不存在");
            
        // 生成随机密码
        var newPassword = GenerateRandomPassword();
        
        // 更新密码
        var salt = PasswordHasher.GenerateSalt();
        var passwordHash = PasswordHasher.HashPassword(newPassword, salt);
        
        user.PasswordHash = passwordHash;
        user.PasswordSalt = salt;
        
        await _unitOfWork.SaveChangesAsync();
        return newPassword;
    }
    
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }
    
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }
    
    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;
            
        // 软删除 - 将用户标记为非活动状态
        user.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> AssignRolesToUserAsync(int userId, IEnumerable<int> roleIds)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;
            
        // 获取当前用户的角色
        var currentUserRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
            
        // 移除不在新角色列表中的角色
        foreach (var userRole in currentUserRoles)
        {
            if (!roleIds.Contains(userRole.RoleId))
            {
                _context.UserRoles.Remove(userRole);
            }
        }
        
        // 添加新角色
        foreach (var roleId in roleIds)
        {
            if (!currentUserRoles.Any(ur => ur.RoleId == roleId))
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                });
            }
        }
        
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
    
    public async Task<IEnumerable<Role>> GetUserRolesAsync(int userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId)
    {
        // 获取用户的所有角色ID
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();
            
        // 获取这些角色的所有权限
        return await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync();
    }
    
    // 生成随机密码
    private string GenerateRandomPassword(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}