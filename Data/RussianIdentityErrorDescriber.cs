using Microsoft.AspNetCore.Identity;

namespace FoodTech.Data
{
    public class RussianIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() 
            => new() { Code = nameof(DefaultError), Description = "Произошла неизвестная ошибка." };

        public override IdentityError PasswordRequiresDigit() 
            => new() { Code = nameof(PasswordRequiresDigit), Description = "Пароль должен содержать цифры (0-9)." };

        public override IdentityError PasswordRequiresLower() 
            => new() { Code = nameof(PasswordRequiresLower), Description = "Пароль должен содержать строчные буквы (a-z)." };

        public override IdentityError PasswordRequiresUpper() 
            => new() { Code = nameof(PasswordRequiresUpper), Description = "Пароль должен содержать прописные буквы (A-Z)." };

        public override IdentityError PasswordRequiresNonAlphanumeric() 
            => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Пароль должен содержать хотя бы один символ, не являющийся буквой или цифрой." };

        public override IdentityError PasswordTooShort(int length) 
            => new() { Code = nameof(PasswordTooShort), Description = $"Пароль должен быть не менее {length} символов." };

        public override IdentityError DuplicateUserName(string? userName) 
            => new() { Code = nameof(DuplicateUserName), Description = $"Имя пользователя '{userName}' уже занято." };

        public override IdentityError DuplicateEmail(string? email) 
            => new() { Code = nameof(DuplicateEmail), Description = $"Email '{email}' уже занят." };

        public override IdentityError InvalidEmail(string? email) 
            => new() { Code = nameof(InvalidEmail), Description = $"Email '{email}' имеет неверный формат." };

        // Добавляем остальные методы с nullable параметрами если нужно
        public override IdentityError InvalidUserName(string? userName) 
            => new() { Code = nameof(InvalidUserName), Description = "Недопустимое имя пользователя." };

        public override IdentityError InvalidToken() 
            => new() { Code = nameof(InvalidToken), Description = "Неверный токен." };

        public override IdentityError LoginAlreadyAssociated() 
            => new() { Code = nameof(LoginAlreadyAssociated), Description = "Пользователь с таким логином уже существует." };

        public override IdentityError PasswordMismatch() 
            => new() { Code = nameof(PasswordMismatch), Description = "Неверный пароль." };

        public override IdentityError UserAlreadyHasPassword() 
            => new() { Code = nameof(UserAlreadyHasPassword), Description = "Пользователь уже имеет пароль." };

        public override IdentityError UserAlreadyInRole(string? role) 
            => new() { Code = nameof(UserAlreadyInRole), Description = $"Пользователь уже имеет роль '{role}'." };

        public override IdentityError UserNotInRole(string? role) 
            => new() { Code = nameof(UserNotInRole), Description = $"Пользователь не имеет роли '{role}'." };

        public override IdentityError UserLockoutNotEnabled() 
            => new() { Code = nameof(UserLockoutNotEnabled), Description = "Блокировка аккаунта не включена для этого пользователя." };

        public override IdentityError RecoveryCodeRedemptionFailed() 
            => new() { Code = nameof(RecoveryCodeRedemptionFailed), Description = "Не удалось использовать код восстановления." };

        public override IdentityError ConcurrencyFailure() 
            => new() { Code = nameof(ConcurrencyFailure), Description = "Ошибка параллельного доступа." };
    }
}