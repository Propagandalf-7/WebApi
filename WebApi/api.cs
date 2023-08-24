// This is the main API server for interacting with the data. It follows the 
// design pattern as suggested in the .NET simple http documentation and tutorial.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<db>(opt => opt.UseInMemoryDatabase("ThePentagon"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("DefaultCorsPolicy");

RouteGroupBuilder todoItems = app.MapGroup("/pentagon");

todoItems.MapGet("/user", GetAllUsers); // Get all users
todoItems.MapGet("/user/{id}", GetUser); // Get user by id
todoItems.MapPost("/user", CreateUser); // Create a user
todoItems.MapDelete("/user/{id}", DeleteUser); // Delete a user by id
todoItems.MapPut("/user/{id}/groups", EditUserGroups); // Edit a user group
todoItems.MapPut("/user/{id}", EditUserDetails); // Edit a user

todoItems.MapPost("/user/{id}/verify-password", VerifyPassword); // Verify a user password

todoItems.MapGet("/group", GetAllGroups); // Get all groups
todoItems.MapGet("/group/{id}", GetGroup); // Get group by id
todoItems.MapPost("/group", CreateGroup); // Create group
todoItems.MapDelete("/group/{id}", DeleteGroup); // Delete a group by id
todoItems.MapPut("/group/{id}/permissions", EditGroupPermissions); // Edit a group permissions

todoItems.MapGet("/permission", GetAllPermissions); // Get all permissions
todoItems.MapGet("/permission/{id}", GetPermission); // Get permission by id
todoItems.MapPost("/permission", CreatePermissions); // Create a permission
todoItems.MapDelete("/permission/{id}", DeletePermission); // Delete a permission by id


using (var scope = app.Services.CreateScope()) // Ensure seeded data gets loading into DB.
{
    var context = scope.ServiceProvider.GetRequiredService<db>();
    context.Database.EnsureCreated();
}
app.Run();

static string HashPassword(string password)
{
    return password;
}

static bool VerifyHashedPassword(string checkPassword, string storedPassword)
{
    if (checkPassword == storedPassword)
    {
        return true;
    }
    else
    {
        return false;
    }
}

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
static async Task<IResult> EditUserDetails(int id, UserEditDTO userEditDTO, db db)
{
    var user = await db.Users
        .Include(u => u.UserGroups)
        .FirstOrDefaultAsync(u => u.Id == id);

    if (user == null)
    {
        return TypedResults.NotFound($"User with ID {id} not found.");
    }

    // Check old password if a new one is provided
    if (!string.IsNullOrWhiteSpace(userEditDTO.NewPassword))
    {
        if (string.IsNullOrWhiteSpace(userEditDTO.OldPassword))
        {
            return TypedResults.BadRequest("Old password is required to set a new password.");
        }

        var oldPasswordHash = HashPassword(userEditDTO.OldPassword); // Assuming you have a function to hash the password.

        if (oldPasswordHash != user.Password)
        {
            return TypedResults.BadRequest("Old password is incorrect.");
        }

        user.Password = HashPassword(userEditDTO.NewPassword); // Update with new hashed password
    }

    // Update other user details
    if (!string.IsNullOrWhiteSpace(userEditDTO.Name))
    {
        user.Name = userEditDTO.Name;
    }

    if (!string.IsNullOrWhiteSpace(userEditDTO.Surname))
    {
        user.Surname = userEditDTO.Surname;
    }

    if (!string.IsNullOrWhiteSpace(userEditDTO.Email))
    {
        var existingUserWithSameEmail = await db.Users.AnyAsync(u => u.Email == userEditDTO.Email && u.Id != id);
        if (existingUserWithSameEmail)
        {
            return TypedResults.BadRequest("The email is already associated with another user.");
        }
        user.Email = userEditDTO.Email;
    }

    // Update the user's group associations based on either group IDs or group names.
    if (userEditDTO.GroupIds != null || userEditDTO.GroupNames != null)
    {
        // Remove all existing group associations for this user.
        db.UserGroups.RemoveRange(user.UserGroups);

        List<int> newGroupIds = new List<int>();

        // If group IDs are provided, use them.
        if (userEditDTO.GroupIds != null)
        {
            newGroupIds.AddRange(userEditDTO.GroupIds);
        }

        // If group names are provided, convert them to IDs.
        if (userEditDTO.GroupNames != null)
        {
            var namedGroupIds = await db.Groups
                .Where(g => userEditDTO.GroupNames.Contains(g.GroupName))
                .Select(g => g.GroupId)
                .ToListAsync();

            newGroupIds.AddRange(namedGroupIds);
        }

        // Ensure no duplicates.
        newGroupIds = newGroupIds.Distinct().ToList();

        // Add new group associations.
        foreach (var groupId in newGroupIds)
        {
            user.UserGroups.Add(new UserGroup { UserId = id, GroupId = groupId });
        }
    }

    await db.SaveChangesAsync();

    return TypedResults.Ok(new UserItemDTO(user));
}
static async Task<IResult> VerifyPassword(int id, PasswordVerifyDTO passwordDTO, db db)
{
    var user = await db.Users.FindAsync(id);
    if (user == null)
    {
        return TypedResults.NotFound($"User with ID {id} not found.");
    }

    bool isPasswordCorrect = VerifyHashedPassword(user.Password, passwordDTO.Password);

    if (!isPasswordCorrect)
    {
        return TypedResults.BadRequest("Incorrect password.");
    }

    return TypedResults.Ok(new { status = "Password is correct" });
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
