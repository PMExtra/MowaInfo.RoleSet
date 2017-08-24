using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MowaInfo.RoleSet
{
    public class RoleSet : RoleSet<IdentityRole, string>
    {
        public RoleSet(RoleManager<IdentityRole> roleManager) : base(roleManager)
        {
        }
    }

    public class RoleSet<TKey> : RoleSet<IdentityRole<TKey>, TKey>
        where TKey : IEquatable<TKey>
    {
        public RoleSet(RoleManager<IdentityRole<TKey>> roleManager) : base(roleManager)
        {
        }
    }

    public class RoleSet<TRole, TKey> : RoleSet<RoleManager<TRole>, TRole, TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        public RoleSet(RoleManager<TRole> roleManager) : base(roleManager)
        {
        }
    }

    public class RoleSet<TRoleManager, TRole, TKey>
        where TRoleManager : RoleManager<TRole>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly Dictionary<TKey, TRole> _idDictionary;

        private readonly Dictionary<string, TRole> _nameDictionary;

        private readonly TRoleManager _roleManager;

        public RoleSet(TRoleManager roleManager)
        {
            _roleManager = roleManager;
            _idDictionary = new Dictionary<TKey, TRole>();
            _nameDictionary = new Dictionary<string, TRole>();

            var roles = roleManager.Roles.AsNoTracking().ToList();
            foreach (var role in roles)
            {
                _idDictionary.Add(role.Id, role);
                _nameDictionary.Add(role.NormalizedName, role);
            }

            var roleProperties = GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => typeof(TRole).IsAssignableFrom(p.PropertyType));
            foreach (var property in roleProperties)
            {
                property.SetValue(this, FindByName(property.Name));
            }
        }

        public TRole Find(TKey id)
        {
            return _idDictionary[id];
        }

        public TRole FindByName(string roleName)
        {
            var normalizedName = _roleManager.NormalizeKey(roleName);
            return _nameDictionary[normalizedName];
        }
    }
}
