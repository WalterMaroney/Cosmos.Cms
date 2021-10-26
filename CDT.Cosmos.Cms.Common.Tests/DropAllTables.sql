IF OBJECT_ID(N'dbo.ArticleLogs', N'U') IS NOT NULL
BEGIN
	ALTER TABLE dbo.[ArticleLogs] DROP CONSTRAINT [FK_ArticleLogs_Articles_ArticleId];
	ALTER TABLE dbo.[ArticleLogs] DROP CONSTRAINT [FK_ArticleLogs_AspNetUsers_IdentityUserId];
END
IF OBJECT_ID(N'dbo.Articles', N'U') IS NOT NULL
BEGIN
	ALTER TABLE dbo.[Articles] DROP CONSTRAINT [FK_Articles_FontIcons_FontIconId];
	ALTER TABLE dbo.[Articles] DROP CONSTRAINT [FK_Articles_Layouts_LayoutId];
	ALTER TABLE dbo.[Articles] DROP CONSTRAINT [FK_Articles_Teams_TeamId];
END

IF OBJECT_ID(N'dbo.AspNetRoleClaims', N'U') IS NOT NULL
ALTER TABLE dbo.[AspNetRoleClaims] DROP CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId];

IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NOT NULL
ALTER TABLE dbo.[AspNetUserClaims] DROP CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId];

IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NOT NULL
ALTER TABLE dbo.[AspNetUserLogins] DROP CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId];

IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NOT NULL
ALTER TABLE dbo.[AspNetUserRoles] DROP CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId];

IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NOT NULL
ALTER TABLE dbo.[AspNetUserRoles] DROP CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId];

IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NOT NULL
ALTER TABLE dbo.[AspNetUserTokens] DROP CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId];

IF OBJECT_ID(N'dbo.MenuItems', N'U') IS NOT NULL
BEGIN
	ALTER TABLE dbo.[MenuItems] DROP CONSTRAINT [FK_MenuItems_Articles_ArticleId];
	ALTER TABLE dbo.[MenuItems] DROP CONSTRAINT [FK_MenuItems_MenuItems_ParentId];
END

IF OBJECT_ID(N'dbo.TeamMembers', N'U') IS NOT NULL
BEGIN
	ALTER TABLE dbo.[TeamMembers] DROP CONSTRAINT [FK_TeamMembers_AspNetUsers_UserId];
	ALTER TABLE dbo.[TeamMembers] DROP CONSTRAINT [FK_TeamMembers_Teams_TeamId];
END

DROP TABLE IF EXISTS dbo.[__EFMigrationsHistory];
DROP TABLE IF EXISTS dbo.[ArticleLogs];
DROP TABLE IF EXISTS dbo.[Articles];
DROP TABLE IF EXISTS dbo.[AspNetRoleClaims];
DROP TABLE IF EXISTS dbo.[AspNetRoles];
DROP TABLE IF EXISTS dbo.[AspNetUserClaims];
DROP TABLE IF EXISTS dbo.[AspNetUserLogins];
DROP TABLE IF EXISTS dbo.[AspNetUserRoles];
DROP TABLE IF EXISTS dbo.[AspNetUsers];
DROP TABLE IF EXISTS dbo.[AspNetUserTokens];
DROP TABLE IF EXISTS dbo.[FontIcons];
DROP TABLE IF EXISTS dbo.[Layouts];
DROP TABLE IF EXISTS dbo.[MenuItems];
DROP TABLE IF EXISTS dbo.[TeamMembers];
DROP TABLE IF EXISTS dbo.[Teams];
DROP TABLE IF EXISTS dbo.[Templates];