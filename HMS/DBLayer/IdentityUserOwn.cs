using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DBLayer
{

    public class IdentityUserOwn : IdentityUser
    {
        [PersonalData]
        [Column(TypeName = "nvarchar(64)")]
        public string Name { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(64)")]
        public string Surname { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(64)")]
        public string? IBAN { get; set; } = default!;

        [PersonalData]
        [Column(TypeName = "nvarchar(64)")]
        public string? Adresa { get; set; } = default!;


        /// <summary>
        /// povolene role pre pouzitie funkcionality správy profesií v modulu LudskeZdroje
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_CRUD_ROLI { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Personalista };

        /// <summary>
        /// povolene role pre spravu zamestnancov v modulu LudskeZdroje
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_CRUD_ZAMESTNANCI { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Personalista };

        /// <summary>
        /// povolene role pre zobrazenie zamestnancov v modulu LudskeZdroje
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_R_ZAMESTNANCI { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Skladnik, RolesOwn.Recepcny, RolesOwn.Bezpecnostnik, RolesOwn.FBVeduci, RolesOwn.HKVeduci, RolesOwn.Nakupca, RolesOwn.Personalista, RolesOwn.Riaditel, RolesOwn.Uctovnik, RolesOwn.UdalostnyPlanovac, RolesOwn.Udrzbar };
    }

    /// <summary>
    /// Pomoc od AI, tato trieda je okopírovaná z originalneho kodu pre UserValidator, s miernou upravou pre validaciu UserName
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class CustomUserValidator<TUser> : IUserValidator<TUser> where TUser : class
    {
        /// <summary>
        /// Creates a new instance of <see cref="UserValidator{TUser}"/>.
        /// </summary>
        /// <param name="errors">The <see cref="IdentityErrorDescriber"/> used to provider error messages.</param>
        public CustomUserValidator(IdentityErrorDescriber? errors = null)
        {
            Describer = errors ?? new IdentityErrorDescriber();
        }

        /// <summary>
        /// Gets the <see cref="IdentityErrorDescriber"/> used to provider error messages for the current <see cref="UserValidator{TUser}"/>.
        /// </summary>
        /// <value>The <see cref="IdentityErrorDescriber"/> used to provider error messages for the current <see cref="UserValidator{TUser}"/>.</value>
        public IdentityErrorDescriber Describer { get; private set; }

        public virtual async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {
            if (manager is null || user is null) {
                throw new ArgumentNullException(manager is null ? nameof(manager) : nameof(user));
            }
            var errors = await ValidateUserName(manager, user).ConfigureAwait(false);
            if (manager.Options.User.RequireUniqueEmail)
            {
                errors = await ValidateEmail(manager, user, errors).ConfigureAwait(false);
            }
            return errors?.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        private async Task<List<IdentityError>?> ValidateUserName(UserManager<TUser> manager, TUser user)
        {
            List<IdentityError>? errors = null;
            var userName = await manager.GetUserNameAsync(user).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(userName))    //bez kontroly na null
            {
                //errors ??= new List<IdentityError>();
                //errors.Add(Describer.InvalidUserName(userName));
            }
            else if (!string.IsNullOrEmpty(manager.Options.User.AllowedUserNameCharacters) &&
                userName.Any(c => !manager.Options.User.AllowedUserNameCharacters.Contains(c)))
            {
                errors ??= new List<IdentityError>();
                errors.Add(Describer.InvalidUserName(userName));
            }
            else
            {
                var owner = await manager.FindByNameAsync(userName).ConfigureAwait(false);
                if (owner != null &&
                    !string.Equals(await manager.GetUserIdAsync(owner).ConfigureAwait(false), await manager.GetUserIdAsync(user).ConfigureAwait(false)))
                {
                    errors ??= new List<IdentityError>();
                    errors.Add(Describer.DuplicateUserName(userName));
                }
            }

            return errors;
        }
        
        private async Task<List<IdentityError>?> ValidateEmail(UserManager<TUser> manager, TUser user, List<IdentityError>? errors)
        {
            var email = await manager.GetEmailAsync(user).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(email))
            {
                errors ??= new List<IdentityError>();
                errors.Add(Describer.InvalidEmail(email));
                return errors;
            }
            if (!new EmailAddressAttribute().IsValid(email))
            {
                errors ??= new List<IdentityError>();
                errors.Add(Describer.InvalidEmail(email));
                return errors;
            }
            var owner = await manager.FindByEmailAsync(email).ConfigureAwait(false);
            if (owner != null &&
                !string.Equals(await manager.GetUserIdAsync(owner).ConfigureAwait(false), await manager.GetUserIdAsync(user).ConfigureAwait(false)))
            {
                errors ??= new List<IdentityError>();
                errors.Add(Describer.DuplicateEmail(email));
            }
            return errors;
        }
    }

}
