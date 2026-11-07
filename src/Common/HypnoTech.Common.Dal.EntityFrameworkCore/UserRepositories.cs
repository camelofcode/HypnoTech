using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Common.Dal.EntityFrameworkCore
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly IHypnoTechDbContextFactory _contextFactory;

        public UserRepository(IHypnoTechDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<AdminUser> GetUserForLoginAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var normalized = username.ToLower();
            return await context.Set<AdminUser>().AsNoTracking()
                .SingleOrDefaultAsync(user => user.Username.ToLower() == normalized, cancellationToken);
        }

        public async Task<List<string>> GetUserRolesAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            return (await GetUserForLoginAsync(username, cancellationToken)).Roles;
        }

        public async Task<List<AdminUser>> GetAllUsersAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Set<AdminUser>().AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<AdminUser> GetUserForEditAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Set<AdminUser>().AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
        }

        public async Task<string> AddUserAsync(
            AdminUser user,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var error = await DuplicateErrorAsync(context, user, cancellationToken);
            if (error != null)
            {
                return error;
            }

            user.Id = Guid.NewGuid().ToString("N");
            context.Add(user);
            await context.SaveChangesAsync(cancellationToken);
            return null;
        }

        public async Task<string> UpdateUserAsync(
            AdminUser user,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var error = await DuplicateErrorAsync(context, user, cancellationToken);
            if (error != null)
            {
                return error;
            }

            var existing = await context.Set<AdminUser>()
                .SingleAsync(item => item.Id == user.Id, cancellationToken);
            var password = existing.Password;
            context.Entry(existing).CurrentValues.SetValues(user);
            existing.Password = password;
            existing.Roles = user.Roles.ToList();
            await context.SaveChangesAsync(cancellationToken);
            return null;
        }

        public Task<AdminUser> ChangePasswordAsync(
            string id,
            string password,
            CancellationToken cancellationToken = default)
        {
            return ChangePasswordAsync(user => user.Id == id, password, cancellationToken);
        }

        public Task<AdminUser> ChangePasswordByEmailAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            var normalized = email.ToLower();
            return ChangePasswordAsync(
                user => user.Email.ToLower() == normalized,
                password,
                cancellationToken);
        }

        public async Task RemoveUserAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            await context.Set<AdminUser>()
                .Where(user => user.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<AdminUser> GetUserByUsernameOrEmailAsync(
            string nameOrEmail,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var normalized = nameOrEmail.ToLower();
            return await context.Set<AdminUser>().AsNoTracking().SingleOrDefaultAsync(
                user => user.Username.ToLower() == normalized || user.Email.ToLower() == normalized,
                cancellationToken);
        }

        internal static async Task<string> DuplicateErrorAsync(
            DbContext context,
            AdminUser user,
            CancellationToken cancellationToken)
        {
            var username = user.Username.ToLower();
            if (await context.Set<AdminUser>().AnyAsync(
                    existing => existing.Id != user.Id && existing.Username.ToLower() == username,
                    cancellationToken))
            {
                return "Username is already in use";
            }

            var email = user.Email.ToLower();
            return await context.Set<AdminUser>().AnyAsync(
                existing => existing.Id != user.Id && existing.Email.ToLower() == email,
                cancellationToken)
                ? "Email is already in use"
                : null;
        }

        private async Task<AdminUser> ChangePasswordAsync(
            System.Linq.Expressions.Expression<Func<AdminUser, bool>> predicate,
            string password,
            CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var user = await context.Set<AdminUser>().SingleAsync(predicate, cancellationToken);
            user.Password = password;
            await context.SaveChangesAsync(cancellationToken);
            return user;
        }
    }

    public sealed class AccountRepository : IAccountRepository
    {
        private readonly IHypnoTechDbContextFactory _contextFactory;

        public AccountRepository(IHypnoTechDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<AdminUser> GetUserByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var normalized = name.ToLower();
            return await context.Set<AdminUser>().AsNoTracking()
                .SingleOrDefaultAsync(user => user.Username.ToLower() == normalized, cancellationToken);
        }

        public async Task<string> UpdateAccountAsync(
            AdminUser user,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var error = await UserRepository.DuplicateErrorAsync(context, user, cancellationToken);
            if (error != null)
            {
                return error;
            }

            var existing = await context.Set<AdminUser>()
                .SingleAsync(item => item.Id == user.Id, cancellationToken);
            existing.Username = user.Username;
            existing.Email = user.Email;
            await context.SaveChangesAsync(cancellationToken);
            return null;
        }

        public async Task ChangePasswordAsync(
            string id,
            string password,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var user = await context.Set<AdminUser>().SingleAsync(item => item.Id == id, cancellationToken);
            user.Password = password;
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
