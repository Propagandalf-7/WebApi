public class User
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string Surname { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public ICollection<UserGroup> UserGroups { get; set; }
}

public class Group
{
    public int GroupId { get; set; }
    public string GroupName { get; set; }
    public ICollection<UserGroup> UserGroups { get; set; }
    public ICollection<GroupPermission> GroupPermissions { get; set; }
}

public class Permission
{
    public int PermissionId { get; set; }
    public string PermissionName { get; set; }
    public ICollection<GroupPermission> GroupPermissions { get; set; }
}

public class UserGroup
{
    public int UserId { get; set; }
    public User User { get; set; }
    public int GroupId { get; set; }
    public Group Group { get; set; }
}

public class GroupPermission
{
    public int GroupId { get; set; }
    public Group Group { get; set; }
    public int PermissionId { get; set; }
    public Permission Permission { get; set; }
}