using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.Validators.Auth;

namespace SmartCare.UnitTests.Validators;

public class AuthValidatorTests
{
    #region LoginValidator

    [Fact]
    public void LoginValidator_ShouldFail_WhenEmailIsEmpty()
    {
        var validator = new LoginRequestDtoValidator();
        var dto = new LoginRequestDto { Email = "", Password = "pass" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void LoginValidator_ShouldFail_WhenEmailIsInvalid()
    {
        var validator = new LoginRequestDtoValidator();
        var dto = new LoginRequestDto { Email = "bad-email", Password = "pass" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void LoginValidator_ShouldFail_WhenPasswordIsEmpty()
    {
        var validator = new LoginRequestDtoValidator();
        var dto = new LoginRequestDto { Email = "user@test.com", Password = "" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void LoginValidator_ShouldPass_WhenPayloadIsValid()
    {
        var validator = new LoginRequestDtoValidator();
        var dto = new LoginRequestDto { Email = "user@example.com", Password = "secret" };

        var result = validator.TestValidate(dto);
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region ForgetPasswordValidator

    [Fact]
    public void ForgetPasswordValidator_ShouldFail_WhenEmailIsEmpty()
    {
        var validator = new ForgetPasswordRequestDtoValidator();
        var dto = new ForgetPasswordRequestDto { Email = "" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ForgetPasswordValidator_ShouldPass_WhenEmailIsValid()
    {
        var validator = new ForgetPasswordRequestDtoValidator();
        var dto = new ForgetPasswordRequestDto { Email = "user@example.com" };

        var result = validator.TestValidate(dto);
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region ConfirmResetPasswordValidator

    [Fact]
    public void ConfirmResetPasswordValidator_ShouldFail_WhenCodeIsEmpty()
    {
        var validator = new ConfirmResetPasswordCodeRequestDtoValidator();
        var dto = new ConfirmResetPasswordCodeRequestDto { Email = "u@test.com", Code = "" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void ConfirmResetPasswordValidator_ShouldFail_WhenEmailIsEmpty()
    {
        var validator = new ConfirmResetPasswordCodeRequestDtoValidator();
        var dto = new ConfirmResetPasswordCodeRequestDto { Email = "", Code = "123456" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region SetNewPasswordValidator

    [Fact]
    public void SetNewPasswordValidator_ShouldFail_WhenEmailIsEmpty()
    {
        var validator = new SetNewPasswordRequestDtoValidator();
        var dto = new SetNewPasswordRequestDto { Email = "", NewPassword = "Pass@12345678" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void SetNewPasswordValidator_ShouldFail_WhenPasswordIsEmpty()
    {
        var validator = new SetNewPasswordRequestDtoValidator();
        var dto = new SetNewPasswordRequestDto { Email = "u@test.com", NewPassword = "" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    #endregion
}
