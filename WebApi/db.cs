// This class sets up the database, byt initializing the tables, setting the relationships
// and seeding the tables with dummy data.

using Microsoft.EntityFrameworkCore;
class db : DbContext
{
    public db(DbContextOptions<db> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<GroupPermission> GroupPermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserGroup>()
            .HasKey(ug => new { ug.UserId, ug.GroupId });

        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.User)
            .WithMany(u => u.UserGroups)
            .HasForeignKey(ug => ug.UserId);

        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.Group)
            .WithMany(g => g.UserGroups)
            .HasForeignKey(ug => ug.GroupId);

        modelBuilder.Entity<GroupPermission>()
            .HasKey(gp => new { gp.GroupId, gp.PermissionId });

        modelBuilder.Entity<GroupPermission>()
            .HasOne(gp => gp.Group)
            .WithMany(g => g.GroupPermissions)
            .HasForeignKey(gp => gp.GroupId);

        modelBuilder.Entity<GroupPermission>()
            .HasOne(gp => gp.Permission)
            .WithMany(p => p.GroupPermissions)
            .HasForeignKey(gp => gp.PermissionId);

        //SEEDING
        modelBuilder.Entity<Permission>().HasData(
            new Permission { PermissionId = 1, PermissionName = "Level_1" },
            new Permission { PermissionId = 2, PermissionName = "Level_2" },
            new Permission { PermissionId = 3, PermissionName = "Level_3" },
            new Permission { PermissionId = 4, PermissionName = "Level_4" },
            new Permission { PermissionId = 5, PermissionName = "Level_5" }
        );

        modelBuilder.Entity<Group>().HasData(
            new Group { GroupId = 1, GroupName = "POTUS" },
            new Group { GroupId = 2, GroupName = "General" },
            new Group { GroupId = 3, GroupName = "CIA Director" },
            new Group { GroupId = 4, GroupName = "CIA Agent" },
            new Group { GroupId = 5, GroupName = "Military" },
            new Group { GroupId = 6, GroupName = "Administration" },
            new Group { GroupId = 7, GroupName = "Civilian" }
        );

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "John", Surname = "Doe", Email = "john1@example.com", Password = "admin" },
            new User { Id = 2, Name = "Jane", Surname = "Smith", Email = "jane2@example.com", Password = "admin" },
            new User { Id = 3, Name = "Robert", Surname = "Johnson", Email = "robert3@example.com", Password = "admin" },
            new User { Id = 4, Name = "Michael", Surname = "Williams", Email = "michael4@example.com", Password = "admin" },
            new User { Id = 5, Name = "William", Surname = "Brown", Email = "william5@example.com", Password = "admin" },
            new User { Id = 6, Name = "David", Surname = "Jones", Email = "david6@example.com", Password = "admin" },
            new User { Id = 7, Name = "Richard", Surname = "Garcia", Email = "richard7@example.com", Password = "admin" },
            new User { Id = 8, Name = "Joseph", Surname = "Miller", Email = "joseph8@example.com", Password = "admin" },
            new User { Id = 9, Name = "Charles", Surname = "Davis", Email = "charles9@example.com", Password = "admin" },
            new User { Id = 10, Name = "Thomas", Surname = "Rodriguez", Email = "thomas10@example.com", Password = "admin" }
        );

        modelBuilder.Entity<UserGroup>().HasData(
            new UserGroup { UserId = 1, GroupId = 1 },
            new UserGroup { UserId = 2, GroupId = 2 },
            new UserGroup { UserId = 3, GroupId = 2 },
            new UserGroup { UserId = 4, GroupId = 3 },
            new UserGroup { UserId = 5, GroupId = 4 },
            new UserGroup { UserId = 6, GroupId = 4 },
            new UserGroup { UserId = 7, GroupId = 5 },
            new UserGroup { UserId = 8, GroupId = 5 },
            new UserGroup { UserId = 9, GroupId = 6 },
            new UserGroup { UserId = 10, GroupId = 7 }
        );

        modelBuilder.Entity<GroupPermission>().HasData(
            new GroupPermission { GroupId = 1, PermissionId = 1 },
            new GroupPermission { GroupId = 1, PermissionId = 2 },
            new GroupPermission { GroupId = 1, PermissionId = 3 },
            new GroupPermission { GroupId = 1, PermissionId = 4 },
            new GroupPermission { GroupId = 1, PermissionId = 5 },
            new GroupPermission { GroupId = 2, PermissionId = 1 },
            new GroupPermission { GroupId = 2, PermissionId = 2 },
            new GroupPermission { GroupId = 2, PermissionId = 3 },
            new GroupPermission { GroupId = 2, PermissionId = 4 },
            new GroupPermission { GroupId = 3, PermissionId = 1 },
            new GroupPermission { GroupId = 3, PermissionId = 2 },
            new GroupPermission { GroupId = 3, PermissionId = 3 },
            new GroupPermission { GroupId = 3, PermissionId = 4 },
            new GroupPermission { GroupId = 4, PermissionId = 1 },
            new GroupPermission { GroupId = 4, PermissionId = 2 },
            new GroupPermission { GroupId = 4, PermissionId = 3 },
            new GroupPermission { GroupId = 5, PermissionId = 1 },
            new GroupPermission { GroupId = 5, PermissionId = 2 },
            new GroupPermission { GroupId = 6, PermissionId = 1 },
            new GroupPermission { GroupId = 7, PermissionId = 1 }
        );
    }
}