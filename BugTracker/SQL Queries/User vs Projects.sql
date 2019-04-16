SELECT 
    [users].[Id] AS [User_Id],
	[users].[Email] AS [DisplayName],
	[project].[Name] AS [Name],
    [project].[Id] AS [Id]
FROM 
	[AspNetUsers] AS [users]

INNER JOIN 
	 [ApplicationUserProjects] AS [usersXprojects]
		ON [users].[Id] = [usersXprojects].[ApplicationUser_Id]

INNER JOIN
	[Projects] AS [project] 
		ON [usersXprojects].[Project_Id] = [project].[Id]