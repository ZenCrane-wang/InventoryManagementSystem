# InventoryERP - 企业级库存管理系统

## 📖 项目简介

InventoryERP 是一个基于 .NET 8 和 Entity Framework Core 构建的现代化库存管理系统后端 API。系统采用分层架构设计，实现了完整的用户认证、权限管理和库存管理功能，适用于中小型企业的库存管理需求。

## ✨ 核心特性

- 🔐 安全认证 : JWT 令牌认证 + RBAC 权限控制
- 📦 库存管理 : 产品、供应商、采购订单管理
- 🏗️ 现代架构 : .NET 8 + EF Core + MySQL
- 📚 API 文档 : 集成 Swagger 自动文档生成
- 🔒 企业安全 : HMAC-SHA512 密码加密
- ⚡ 高性能 : 异步编程 + 数据库优化

## 🛠️ 技术栈

### 后端框架

- .NET 8 - 最新的.NET 平台
- ASP.NET Core Web API - RESTful API 框架
- Entity Framework Core 8.0 - ORM 数据访问框架

### 数据库

- MySQL - 主数据库
- Pomelo.EntityFrameworkCore.MySql - MySQL EF Core 提供商

### 认证与安全

- JWT Bearer Token - 无状态身份认证
- HMAC-SHA512 - 密码哈希算法
- RBAC 权限模型 - 基于角色的访问控制

### 开发工具

- Swagger/OpenAPI - API 文档自动生成
- 依赖注入 - 内置 IoC 容器
