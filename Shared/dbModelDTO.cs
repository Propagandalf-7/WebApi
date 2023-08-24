// This class contains data transfer objects (DTO) classes. These classes are used
// for input, edit and viewing of the data. The data in these classes can be structured
// in a specific way to return relationships between multiple tables as a single output
// and also hide certain sensitive data.
using System.ComponentModel.DataAnnotations;

public class UserItemDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string Surname { get; set; }
    public string? Email { get; set; }
    public List<int> GroupIds { get; set; }

    public List<string> GroupNames { get; set; }

    public List<int> PermissionIds { get; set; }

    public List<string> PermissionNames { get; set; }

    public UserItemDTO() {
        GroupIds = new List<int>();
        GroupNames = new List<string>();
        PermissionIds = new List<int>();
        PermissionNames = new List<string>();
    }
    public UserItemDTO(User userItem)
    {
        Id = userItem.Id;
        Name = userItem.Name;
        Surname = userItem.Surname;
        Email = userItem.Email;

        if (userItem.UserGroups != null)
        {
            GroupIds = userItem.UserGroups.Select(ug => ug.GroupId).ToList();

            GroupNames = userItem.UserGroups
                .Where(ug => ug.Group != null)
                .Select(ug => ug.Group.GroupName)
                .Distinct()
                .ToList();

            var permissions = userItem.UserGroups
                .Where(ug => ug.Group != null && ug.Group.GroupPermissions != null)
                .SelectMany(ug => ug.Group.GroupPermissions)
                .Distinct()
                .ToList();

            PermissionIds = permissions.Select(gp => gp.PermissionId)
                                       .Distinct()
                                       .ToList();

            PermissionNames = permissions
                .Where(gp => gp.Permission != null)
                .Select(gp => gp.Permission.PermissionName)
                .Distinct()
                .ToList();
        }
    }

}

public class UserInputDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string? Password { get; set; }
    public List<int>? GroupIds { get; set; }
    public List<string>? GroupNames { get; set; }
}

public class UserEditDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string NewPassword { get; set; }
    public string OldPassword { get; set; }
    public List<int>? GroupIds { get; set; }
    public List<string>? GroupNames { get; set; }
}

public class EditGroupsDTO
{
    public int Id { get; set; }
    public List<int>? GroupIds { get; set; }
}

public class PasswordVerifyDTO
{
    public string Password { get; set; }
}

public class GroupItemDTO
{
    public int GroupId { get; set; }
    public string? GroupName { get; set; }
    public List<string> PermissionNames { get; set; }
    public List<int> PermissionIds { get; set; }
    public bool Selected { get; set; }

    public GroupItemDTO() { }

    public GroupItemDTO(Group group)
    {
        GroupId = group.GroupId;
        GroupName = group.GroupName;

        if (group.GroupPermissions != null)
        {
            PermissionIds = group.GroupPermissions
                                 .Where(gp => gp.Permission != null)
                                 .Select(gp => gp.PermissionId)
                                 .Distinct()
                                 .ToList();

            PermissionNames = group.GroupPermissions
                                   .Where(gp => gp.Permission != null)
                                   .Select(gp => gp.Permission.PermissionName)
                                   .Distinct()
                                   .ToList();
        }

    }
}

public class GroupInputDTO
{
    public int GroupId { get; set; }
    public string GroupName { get; set; }
    public List<int>? PermissionIds { get; set; }
    public List<int>? PermissionNames { get; set; }
}

public class EditPermissionsDTO
{
    public int GroupId { get; set; } // Group's ID for which permissions are being edited.

    // List of permission IDs the group should have.
    public List<int> PermissionIds { get; set; } = new List<int>();
}

public class PermissionItemDTO
{
    public int PermissionId { get; set; }
    public string PermissionName { get; set; }

    public bool Selected { get; set; }

    public PermissionItemDTO() { }
    public PermissionItemDTO(Permission permission)
    {
        PermissionId = permission.PermissionId;
        PermissionName = permission.PermissionName;
    }
}

public class PermissionInputDTO
{
    public string PermissionName { get; set; }
}

public class UserGroupsEditDTO
{
    public List<int> GroupIds { get; set; }
    public List<string> GroupNames { get; set; }
}

public class GroupPremissionDTO
{
    public List<int> PermissionIds { get; set; }
    public List<string> PermissionNames { get;}
}