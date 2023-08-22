using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<db>(opt => opt.UseInMemoryDatabase("ThePentagon"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

RouteGroupBuilder todoItems = app.MapGroup("/pentagon");

todoItems.MapGet("/user", GetAllUsers);
todoItems.MapGet("/user/{id}", GetUser);
todoItems.MapPost("/user", CreateUser);
todoItems.MapDelete("/user/{id}", DeleteUser);
todoItems.MapPut("/user/{id}/groups", EditUserGroups);

todoItems.MapGet("/group", GetAllGroups);
todoItems.MapGet("/group/{id}", GetGroup);
todoItems.MapPost("/group", CreateGroup);
todoItems.MapDelete("/group/{id}", DeleteGroup);
todoItems.MapPut("/group/{id}/permissions", EditGroupPermissions);

todoItems.MapGet("/permission", GetAllPermissions);
todoItems.MapGet("/permission/{id}", GetPermission);
todoItems.MapPost("/permission", CreatePermissions);
todoItems.MapDelete("/permission/{id}", DeletePermission);


using (var scope = app.Services.CreateScope()) // Ensure seeded data gets loading into DB.
{
    var context = scope.ServiceProvider.GetRequiredService<db>();
    context.Database.EnsureCreated();
}
app.Run();

static async Task<IResult> GetAllUsers(db db)
{
    var users = await db.Users
        .Include(u => u.UserGroups)
            .ThenInclude(ug => ug.Group)
        .ThenInclude(g => g.GroupPermissions)
            .ThenInclude(gp => gp.Permission)
        .Select(u => new UserItemDTO(u))
        .ToArrayAsync();

    return TypedResults.Ok(users);
}
static async Task<IResult> GetUser(int id, db db)
{
    var user = await db.Users
        .Include(u => u.UserGroups)
            .ThenInclude(ug => ug.Group)
        .ThenInclude(g => g.GroupPermissions)
            .ThenInclude(gp => gp.Permission)
        .FirstOrDefaultAsync(u => u.Id == id);

    return user != null
        ? TypedResults.Ok(new UserItemDTO(user))
        : TypedResults.NotFound();
}
static async Task<IResult> CreateUser(UserInputDTO UserInputDTO, db db)
{
    if (string.IsNullOrWhiteSpace(UserInputDTO.Name) ||
        string.IsNullOrWhiteSpace(UserInputDTO.Surname) ||
        string.IsNullOrWhiteSpace(UserInputDTO.Email) ||
        string.IsNullOrWhiteSpace(UserInputDTO.Password))
    {
        return TypedResults.BadRequest("All fields (Name, Surname, Email, and Password) are required.");
    }

    var userItem = new User
    {
        Name = UserInputDTO.Name,
        Surname = UserInputDTO.Surname,
        Email = UserInputDTO.Email,
        Password = UserInputDTO.Password,
        UserGroups = new List<UserGroup>()
    };

    var result = await AssignGroupsToUser(userItem, UserInputDTO.GroupIds, UserInputDTO.GroupNames, db);
    if (result is BadRequestResult)
    {
        return result;  // or use proper casting if needed.
    }

    db.Users.Add(userItem);
    await db.SaveChangesAsync();

    var response = new UserItemDTO(userItem);

    return TypedResults.Created($"/pentagon/{userItem.Id}", response);
}
static async Task<IResult> DeleteUser(int id, db db)
{
    var user = await db.Users.FindAsync(id);
    if (user == null)
    {
        return TypedResults.NotFound($"User with ID {id} not found.");
    }

    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}
static async Task<IResult> EditUserGroups(int id, UserGroupsEditDTO editDTO, db db)
{
    var user = await db.Users
        .Include(u => u.UserGroups)
        .FirstOrDefaultAsync(u => u.Id == id);

    if (user == null)
    {
        return TypedResults.NotFound($"User with ID {id} not found.");
    }

    db.UserGroups.RemoveRange(user.UserGroups);

    var result = await AssignGroupsToUser(user, editDTO.GroupIds, editDTO.GroupNames, db);
    if (result is BadRequestResult)
    {
        return result;  // or use proper casting if needed.
    }

    await db.SaveChangesAsync();

    return TypedResults.Ok(new UserItemDTO(user));
}
static async Task<IResult> AssignGroupsToUser(User user, List<int> groupIds, List<string> groupNames, db db)
{
    var userGroups = new List<UserGroup>();

    if ((groupIds == null || !groupIds.Any()) &&
        (groupNames == null || !groupNames.Any()))
    {
        var defaultGroup = await db.Groups.FirstOrDefaultAsync(g => g.GroupId == 0) ?? new Group { GroupName = "unspecified" };
        if (defaultGroup.GroupId == 0)
        {
            db.Groups.Add(defaultGroup);
            await db.SaveChangesAsync();
        }
        userGroups.Add(new UserGroup { GroupId = defaultGroup.GroupId, User = user });
    }
    else
    {
        if (groupIds != null && groupIds.Any())
        {
            var existingGroupIds = await db.Groups.Where(g => groupIds.Contains(g.GroupId)).Select(g => g.GroupId).ToListAsync();
            if (existingGroupIds.Count != groupIds.Count)
            {
                return TypedResults.BadRequest("Some group IDs do not exist.");
            }
            userGroups.AddRange(existingGroupIds.Select(gid => new UserGroup { GroupId = gid, User = user }));
        }

        if (groupNames != null && groupNames.Any())
        {
            var existingGroups = await db.Groups.Where(g => groupNames.Contains(g.GroupName)).ToListAsync();
            if (existingGroups.Count != groupNames.Count)
            {
                return TypedResults.BadRequest("Some group names do not exist.");
            }
            userGroups.AddRange(existingGroups.Select(group => new UserGroup { GroupId = group.GroupId, User = user }));
        }
    }

    user.UserGroups = userGroups;
    return TypedResults.Ok();
}


static async Task<IResult> GetAllGroups(db db)
{
    var groups = await db.Groups
        .Include(g => g.GroupPermissions)
            .ThenInclude(gp => gp.Permission)
        .Select(g => new GroupItemDTO(g))
        .ToArrayAsync();

    return TypedResults.Ok(groups);
}
static async Task<IResult> GetGroup(int id, db db)
{
    var group = await db.Groups
        .Include(g => g.GroupPermissions)
            .ThenInclude(gp => gp.Permission)
        .FirstOrDefaultAsync(g => g.GroupId == id);

    return group != null
        ? TypedResults.Ok(new GroupItemDTO(group))
        : TypedResults.NotFound();
}
static async Task<IResult> CreateGroup(GroupInputDTO GroupInputDTO, db db)
{
    if (string.IsNullOrWhiteSpace(GroupInputDTO.GroupName))
    {
        return TypedResults.BadRequest("Group name is required.");
    }

    // Check for existing group with the same name
    var existingGroup = await db.Groups.FirstOrDefaultAsync(g => g.GroupName == GroupInputDTO.GroupName);
    if (existingGroup != null)
    {
        return TypedResults.BadRequest($"Group with name {GroupInputDTO.GroupName} already exists.");
    }

    var group = new Group
    {
        GroupName = GroupInputDTO.GroupName,
        GroupPermissions = new List<GroupPermission>()
    };

    if (GroupInputDTO.PermissionIds != null && GroupInputDTO.PermissionIds.Any())
    {
        var existingPermissionIds = await db.Permissions
            .Where(p => GroupInputDTO.PermissionIds.Contains(p.PermissionId))
            .Select(p => p.PermissionId)
            .ToListAsync();

        if (existingPermissionIds.Count != GroupInputDTO.PermissionIds.Count)
        {
            return TypedResults.BadRequest("Some permission IDs do not exist.");
        }

        foreach (var permission in existingPermissionIds.Select(pid => new GroupPermission
        {
            PermissionId = pid,
            Group = group
        }))
        {
            group.GroupPermissions.Add(permission);
        }
    }

    db.Groups.Add(group);
    await db.SaveChangesAsync();

    var response = new GroupItemDTO(group);

    return TypedResults.Created($"/pentagon/groups/{group.GroupId}", response);
}
static async Task<IResult> DeleteGroup(int id, db db)
{
    var group = await db.Groups.FindAsync(id);
    if (group == null)
    {
        return TypedResults.NotFound($"Group with ID {id} not found.");
    }

    db.Groups.Remove(group);
    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}
static async Task<IResult> EditGroupPermissions(int id, GroupPremissionDTO GroupPremissionDTO, db db)
{
    var group = await db.Groups
        .Include(g => g.GroupPermissions)
        .FirstOrDefaultAsync(g => g.GroupId == id);

    if (group == null)
    {
        return TypedResults.NotFound($"Group with ID {id} not found.");
    }

    if (GroupPremissionDTO.PermissionIds != null && GroupPremissionDTO.PermissionIds.Any())
    {
        var existingPermissionIds = await db.Permissions
            .Where(p => GroupPremissionDTO.PermissionIds.Contains(p.PermissionId))
            .Select(p => p.PermissionId)
            .ToListAsync();

        if (existingPermissionIds.Count != GroupPremissionDTO.PermissionIds.Count)
        {
            return TypedResults.BadRequest("Some permission IDs do not exist.");
        }

        db.GroupPermissions.RemoveRange(group.GroupPermissions);

        foreach (var permission in existingPermissionIds.Select(pid => new GroupPermission
        {
            PermissionId = pid,
            Group = group
        }))
        {
            group.GroupPermissions.Add(permission);
        }
    }
    else
    {
        db.GroupPermissions.RemoveRange(group.GroupPermissions);
    }

    await db.SaveChangesAsync();

    return TypedResults.Ok(new GroupItemDTO(group));
}


static async Task<IResult> GetAllPermissions(db db)
{
    return TypedResults.Ok(await db.Permissions.Select(x => new PermissionItemDTO(x)).ToArrayAsync());
}
static async Task<IResult> GetPermission(int id, db db)
{
    return await db.Permissions.FindAsync(id)
        is Permission permission
            ? TypedResults.Ok(new PermissionItemDTO(permission))
            : TypedResults.NotFound();
}
static async Task<IResult> CreatePermissions(PermissionInputDTO PermissionInputDTO, db db)
{
    if (string.IsNullOrWhiteSpace(PermissionInputDTO.PermissionName))
    {
        return TypedResults.BadRequest("Permission name is required.");
    }

    var permission = new Permission
    {
        PermissionName = PermissionInputDTO.PermissionName
    };

    db.Permissions.Add(permission);
    await db.SaveChangesAsync();

    var response = new PermissionItemDTO(permission);

    return TypedResults.Created($"/pentagon/permissions/{permission.PermissionId}", response);
}
static async Task<IResult> DeletePermission(int id, db db)
{
    var permission = await db.Permissions.FindAsync(id);
    if (permission == null)
    {
        return TypedResults.NotFound($"Permission with ID {id} not found.");
    }

    // Before removing the permission, it might be a good idea to check if any group still references it.
    var isPermissionUsed = await db.GroupPermissions.AnyAsync(gp => gp.PermissionId == id);
    if (isPermissionUsed)
    {
        return TypedResults.BadRequest($"Permission with ID {id} is still being used by one or more groups and cannot be deleted.");
    }

    db.Permissions.Remove(permission);
    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}
