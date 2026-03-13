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

        [Fact]
        public void NormalizePhone_ArabicIndicWithPlus()
        {
            // +٩٦٥٩٨٧٦٥٤٣٢
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("+\u0669\u0666\u0665\u0669\u0668\u0667\u0666\u0665\u0664\u0663\u0662"));
        }

        [Fact]
        public void NormalizePhone_ArabicIndicWithDoubleZero()
        {
            // ٠٠٩٦٥٩٨٧٦٥٤٣٢
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("\u0660\u0660\u0669\u0666\u0665\u0669\u0668\u0667\u0666\u0665\u0664\u0663\u0662"));
        }

        [Fact]
        public void NormalizePhone_ArabicIndicWithSpaces()
        {
            // ٩٦٥ ٩٨٧٦ ٥٤٣٢
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("\u0669\u0666\u0665 \u0669\u0668\u0667\u0666 \u0665\u0664\u0663\u0662"));
        }

        [Fact]
        public void NormalizePhone_ArabicIndicWithDashes()
        {
            // ٩٦٥-٩٨٧٦-٥٤٣٢
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("\u0669\u0666\u0665-\u0669\u0668\u0667\u0666-\u0665\u0664\u0663\u0662"));
        }

        [Fact]
        public void NormalizePhone_MixedArabicAndLatinDigits()
        {
            // ٩٦٥98765432
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("\u0669\u0666\u066598765432"));
        }

        [Fact]
        public void NormalizePhone_ExtendedArabicIndicWithPlus()
        {
            // +۹۶۵۹۸۷۶۵۴۳۲ (Farsi/Urdu digits)
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("+\u06F9\u06F6\u06F5\u06F9\u06F8\u06F7\u06F6\u06F5\u06F4\u06F3\u06F2"));
        }

        [Fact]
        public void NormalizePhone_ExtendedArabicIndicWithSpaces()
        {
            // ۹۶۵ ۹۸۷۶ ۵۴۳۲
            Assert.Equal("96598765432", PhoneUtils.NormalizePhone("\u06F9\u06F6\u06F5 \u06F9\u06F8\u06F7\u06F6 \u06F5\u06F4\u06F3\u06F2"));
        }

        [Fact]
        public void NormalizePhone_ArabicIndicSaudiNumber()
        {
            // ٩٦٦٥٥١٢٣٤٥٦٧
            Assert.Equal("966551234567", PhoneUtils.NormalizePhone("\u0669\u0666\u0666\u0665\u0665\u0661\u0662\u0663\u0664\u0665\u0666\u0667"));
        }

        [Fact]
        public void NormalizePhone_ArabicIndicUAENumber()
        {
            // ٩٧١٥٠٤٤٩٦٦٧٧
            Assert.Equal("971504496677", PhoneUtils.NormalizePhone("\u0669\u0667\u0661\u0665\u0660\u0664\u0664\u0669\u0666\u0666\u0667\u0667"));
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
            // Use a number with no country code match to test generic E.164 minimum only
            var result = PhoneUtils.ValidatePhoneInput("2234567");
            Assert.True(result.IsValid);
            Assert.Equal("2234567", result.Normalized);
        }

        [Fact]
        public void Validate_MaximumValid_15Digits()
        {
            // Use a number with no country code match (999 prefix) to test generic E.164 maximum only
            var result = PhoneUtils.ValidatePhoneInput("999123456789012");
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

        [Fact]
        public void Validate_ArabicIndicWithPlus()
        {
            var result = PhoneUtils.ValidatePhoneInput("+\u0669\u0666\u0665\u0669\u0668\u0667\u0666\u0665\u0664\u0663\u0662");
            Assert.True(result.IsValid);
            Assert.Equal("96598765432", result.Normalized);
        }

        [Fact]
        public void Validate_ExtendedArabicIndicDigits()
        {
            // ۹۶۵۹۸۷۶۵۴۳۲ (Farsi/Urdu digits)
            var result = PhoneUtils.ValidatePhoneInput("\u06F9\u06F6\u06F5\u06F9\u06F8\u06F7\u06F6\u06F5\u06F4\u06F3\u06F2");
            Assert.True(result.IsValid);
            Assert.Equal("96598765432", result.Normalized);
        }

        [Fact]
        public void Validate_ArabicIndicWithSpacesAndDashes()
        {
            // ٩٦٥-٩٨٧٦ ٥٤٣٢
            var result = PhoneUtils.ValidatePhoneInput("\u0669\u0666\u0665-\u0669\u0668\u0667\u0666 \u0665\u0664\u0663\u0662");
            Assert.True(result.IsValid);
            Assert.Equal("96598765432", result.Normalized);
        }

        [Fact]
        public void Validate_MixedArabicAndLatinDigits()
        {
            // ٩٦٥98765432
            var result = PhoneUtils.ValidatePhoneInput("\u0669\u0666\u066598765432");
            Assert.True(result.IsValid);
            Assert.Equal("96598765432", result.Normalized);
        }

        [Fact]
        public void Validate_ArabicIndicTooShort()
        {
            // ١٢٣ (too short after normalization)
            var result = PhoneUtils.ValidatePhoneInput("\u0661\u0662\u0663");
            Assert.False(result.IsValid);
            Assert.Contains("too short", result.Error);
        }

        [Fact]
        public void Validate_ArabicIndicSaudiNumber()
        {
            // ٩٦٦٥٥١٢٣٤٥٦٧
            var result = PhoneUtils.ValidatePhoneInput("\u0669\u0666\u0666\u0665\u0665\u0661\u0662\u0663\u0664\u0665\u0666\u0667");
            Assert.True(result.IsValid);
            Assert.Equal("966551234567", result.Normalized);
        }

        // ── Trunk prefix stripping ──

        [Fact]
        public void NormalizePhone_SaudiTrunkPrefix()
        {
            // 966 + 0551234567 → 966551234567
            Assert.Equal("966551234567", PhoneUtils.NormalizePhone("9660551234567"));
        }

        [Fact]
        public void NormalizePhone_UAETrunkPrefix()
        {
            // 971 + 0501234567 → 971501234567
            Assert.Equal("971501234567", PhoneUtils.NormalizePhone("9710501234567"));
        }

        [Fact]
        public void NormalizePhone_EgyptTrunkPrefix()
        {
            // 20 + 01012345678 → 201012345678
            Assert.Equal("201012345678", PhoneUtils.NormalizePhone("20001012345678"));
        }

        [Fact]
        public void Validate_SaudiTrunkPrefixNormalized()
        {
            // Input with domestic trunk 0 — should normalize and validate as valid Saudi
            var result = PhoneUtils.ValidatePhoneInput("9660551234567");
            Assert.True(result.IsValid);
            Assert.Equal("966551234567", result.Normalized);
        }

        [Fact]
        public void Validate_UAETrunkPrefixNormalized()
        {
            var result = PhoneUtils.ValidatePhoneInput("9710501234567");
            Assert.True(result.IsValid);
            Assert.Equal("971501234567", result.Normalized);
        }

        // ── Country-format validation ──

        [Fact]
        public void Validate_KuwaitInvalidMobilePrefix()
        {
            // 965 + 31234567: starts with 3, not a valid Kuwait mobile prefix
            var result = PhoneUtils.ValidatePhoneInput("96531234567");
            Assert.False(result.IsValid);
            Assert.Contains("Kuwait", result.Error);
            Assert.Contains("must start with", result.Error);
        }

        [Fact]
        public void Validate_SaudiInvalidLength()
        {
            // 966 + 51234567 = 8 local digits, Saudi expects 9
            var result = PhoneUtils.ValidatePhoneInput("96651234567");
            Assert.False(result.IsValid);
            Assert.Contains("Saudi Arabia", result.Error);
            Assert.Contains("expected 9", result.Error);
        }

        [Fact]
        public void Validate_UAEValidNumber()
        {
            // 971 + 501234567 = 9 local digits starting with 5
            var result = PhoneUtils.ValidatePhoneInput("971501234567");
            Assert.True(result.IsValid);
            Assert.Equal("971501234567", result.Normalized);
        }

        [Fact]
        public void FindCountryCode_Kuwait()
        {
            Assert.Equal("965", PhoneUtils.FindCountryCode("96598765432"));
        }

        [Fact]
        public void FindCountryCode_Saudi()
        {
            Assert.Equal("966", PhoneUtils.FindCountryCode("966551234567"));
        }

        [Fact]
        public void FindCountryCode_USA()
        {
            Assert.Equal("1", PhoneUtils.FindCountryCode("12025551234"));
        }

        [Fact]
        public void FindCountryCode_Unknown()
        {
            Assert.Null(PhoneUtils.FindCountryCode("9991234567"));
        }

        [Fact]
        public void ValidatePhoneFormat_ValidKuwait()
        {
            var (valid, error) = PhoneUtils.ValidatePhoneFormat("96598765432");
            Assert.True(valid);
            Assert.Null(error);
        }

        [Fact]
        public void ValidatePhoneFormat_InvalidKuwaitPrefix()
        {
            var (valid, error) = PhoneUtils.ValidatePhoneFormat("96531234567");
            Assert.False(valid);
            Assert.Contains("Kuwait", error);
        }

        [Fact]
        public void ValidatePhoneFormat_UnknownCountry()
        {
            // No rule for 999 prefix — passes through
            var (valid, error) = PhoneUtils.ValidatePhoneFormat("9991234567");
            Assert.True(valid);
            Assert.Null(error);
        }
    }
}
