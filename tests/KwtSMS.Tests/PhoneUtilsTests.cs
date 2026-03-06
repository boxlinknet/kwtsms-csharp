using System;
using Xunit;
using KwtSMS;

namespace KwtSMS.Tests
{
    public class PhoneUtilsTests
    {
        // ── NormalizePhone ──

        [Fact]
        public void NormalizePhone_PlainInternational()
        {
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("96598765432"));
        }

        [Fact]
        public void NormalizePhone_StripsPlusPrefix()
        {
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("+96598765432"));
        }

        [Fact]
        public void NormalizePhone_StripsDoubleZero()
        {
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("0096598765432"));
        }

        [Fact]
        public void NormalizePhone_StripsSpaces()
        {
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("965 9876 5432"));
        }

        [Fact]
        public void NormalizePhone_StripsDashes()
        {
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("965-9876-5432"));
        }

        [Fact]
        public void NormalizePhone_StripsDots()
        {
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("965.9876.5432"));
        }

        [Fact]
        public void NormalizePhone_StripsParentheses()
        {
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("(965) 98765432"));
        }

        [Fact]
        public void NormalizePhone_ArabicIndicDigits()
        {
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("\u0669\u0666\u0665\u0669\u0668\u0667\u0666\u0665\u0664\u0663\u0662"));
        }

        [Fact]
        public void NormalizePhone_ExtendedArabicIndicDigits()
        {
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("\u06F9\u06F6\u06F5\u06F9\u06F8\u06F7\u06F6\u06F5\u06F4\u06F3\u06F2"));
        }

        [Fact]
        public void NormalizePhone_MixedFormats()
        {
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("+00 965-9876 5432"));
        }

        [Fact]
        public void NormalizePhone_EmptyString()
        {
            Assert.Equal("", PhoneUtils.NormalizePhone(""));
        }

        [Fact]
        public void NormalizePhone_Null()
        {
            Assert.Equal("", PhoneUtils.NormalizePhone(null));
        }

        [Fact]
        public void NormalizePhone_NoDigits()
        {
            Assert.Equal("", PhoneUtils.NormalizePhone("abc"));
        }

        [Fact]
        public void NormalizePhone_LeadingZerosStripped()
        {
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("00096598765432"));
        }

        // ── ValidatePhoneInput ──

        [Fact]
        public void Validate_ValidKuwaitNumber()
        {
            var result = PhoneUtils.ValidatePhoneInput("96598765432");
            Assert.True(result.IsValid);
            Assert.Null(result.Error);
            Assert.Equal("96598765432", result.Normalized);
        }

        [Fact]
        public void Validate_ValidWithPlusPrefix()
        {
            var result = PhoneUtils.ValidatePhoneInput("+96598765432");
            Assert.True(result.IsValid);
            Assert.Equal("96598765432", result.Normalized);
        }

        [Fact]
        public void Validate_Empty()
        {
            var result = PhoneUtils.ValidatePhoneInput("");
            Assert.False(result.IsValid);
            Assert.Equal("Phone number is required", result.Error);
        }

        [Fact]
        public void Validate_Null()
        {
            var result = PhoneUtils.ValidatePhoneInput(null);
            Assert.False(result.IsValid);
            Assert.Equal("Phone number is required", result.Error);
        }

        [Fact]
        public void Validate_Blank()
        {
            var result = PhoneUtils.ValidatePhoneInput("   ");
            Assert.False(result.IsValid);
            Assert.Equal("Phone number is required", result.Error);
        }

        [Fact]
        public void Validate_EmailAddress()
        {
            var result = PhoneUtils.ValidatePhoneInput("user@gmail.com");
            Assert.False(result.IsValid);
            Assert.Contains("email address", result.Error);
        }

        [Fact]
        public void Validate_NoDigits()
        {
            var result = PhoneUtils.ValidatePhoneInput("abc");
            Assert.False(result.IsValid);
            Assert.Contains("no digits found", result.Error);
        }

        [Fact]
        public void Validate_TooShort()
        {
            var result = PhoneUtils.ValidatePhoneInput("123");
            Assert.False(result.IsValid);
            Assert.Contains("too short", result.Error);
            Assert.Contains("3 digits", result.Error);
        }

        [Fact]
        public void Validate_MinimumValid_7Digits()
        {
            var result = PhoneUtils.ValidatePhoneInput("1234567");
            Assert.True(result.IsValid);
            Assert.Equal("1234567", result.Normalized);
        }

        [Fact]
        public void Validate_MaximumValid_15Digits()
        {
            var result = PhoneUtils.ValidatePhoneInput("123456789012345");
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_TooLong()
        {
            var result = PhoneUtils.ValidatePhoneInput("1234567890123456");
            Assert.False(result.IsValid);
            Assert.Contains("too long", result.Error);
        }

        [Fact]
        public void Validate_ArabicDigits()
        {
            // ٩٦٥٩٨٧٦٥٤٣٢ (Arabic-Indic)
            var result = PhoneUtils.ValidatePhoneInput("\u0669\u0666\u0665\u0669\u0668\u0667\u0666\u0665\u0664\u0663\u0662");
            Assert.True(result.IsValid);
            Assert.Equal("96598765432", result.Normalized);
        }

        [Fact]
        public void Validate_NonStringInput()
        {
            var result = PhoneUtils.ValidatePhoneInput(96598765432L);
            Assert.True(result.IsValid);
            Assert.Equal("96598765432", result.Normalized);
        }

        [Fact]
        public void Validate_WhitespaceAroundNumber()
        {
            var result = PhoneUtils.ValidatePhoneInput("  +96598765432  ");
            Assert.True(result.IsValid);
            Assert.Equal("96598765432", result.Normalized);
        }
    }
}
