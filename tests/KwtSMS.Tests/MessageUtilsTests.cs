using System;
using Xunit;
using KwtSMS;

namespace KwtSMS.Tests
{
    public class MessageUtilsTests
    {
        [Fact]
        public void CleanMessage_PlainEnglish()
        {
            Assert.Equal("Hello World", MessageUtils.CleanMessage("Hello World"));
        }

        [Fact]
        public void CleanMessage_ArabicText_Preserved()
        {
            Assert.Equal("\u0645\u0631\u062D\u0628\u0627", MessageUtils.CleanMessage("\u0645\u0631\u062D\u0628\u0627"));
        }

        [Fact]
        public void CleanMessage_ArabicIndicDigits_Converted()
        {
            Assert.Equal("123456", MessageUtils.CleanMessage("\u0661\u0662\u0663\u0664\u0665\u0666"));
        }

        [Fact]
        public void CleanMessage_ExtendedArabicIndicDigits_Converted()
        {
            Assert.Equal("123456", MessageUtils.CleanMessage("\u06F1\u06F2\u06F3\u06F4\u06F5\u06F6"));
        }

        [Fact]
        public void CleanMessage_OtpWithArabicDigits()
        {
            Assert.Equal("Your OTP is: 123456 ", MessageUtils.CleanMessage("Your OTP is: \u0661\u0662\u0663\u0664\u0665\u0666 "));
        }

        [Fact]
        public void CleanMessage_EmojisStripped()
        {
            Assert.Equal("Hello ", MessageUtils.CleanMessage("Hello \U0001F600"));
        }

        [Fact]
        public void CleanMessage_MultipleEmojis()
        {
            Assert.Equal("", MessageUtils.CleanMessage("\U0001F389\U0001F38A\U0001F680"));
        }

        [Fact]
        public void CleanMessage_EmojiInMiddle()
        {
            Assert.Equal("Hello World", MessageUtils.CleanMessage("Hello \U0001F600World"));
        }

        [Fact]
        public void CleanMessage_FlagEmoji()
        {
            // Regional indicators for flag
            Assert.Equal("Flag: ", MessageUtils.CleanMessage("Flag: \U0001F1F0\U0001F1FC"));
        }

        [Fact]
        public void CleanMessage_HtmlTags_Stripped()
        {
            Assert.Equal("HelloWorld", MessageUtils.CleanMessage("Hello<b>World</b>"));
        }

        [Fact]
        public void CleanMessage_ComplexHtml()
        {
            Assert.Equal("Click here", MessageUtils.CleanMessage("<a href=\"http://example.com\">Click here</a>"));
        }

        [Fact]
        public void CleanMessage_ZeroWidthSpace_Stripped()
        {
            Assert.Equal("HelloWorld", MessageUtils.CleanMessage("Hello\u200BWorld"));
        }

        [Fact]
        public void CleanMessage_BOM_Stripped()
        {
            Assert.Equal("Hello", MessageUtils.CleanMessage("\uFEFFHello"));
        }

        [Fact]
        public void CleanMessage_SoftHyphen_Stripped()
        {
            Assert.Equal("Hello", MessageUtils.CleanMessage("Hel\u00ADlo"));
        }

        [Fact]
        public void CleanMessage_ZeroWidthJoiner_Stripped()
        {
            Assert.Equal("ab", MessageUtils.CleanMessage("a\u200Db"));
        }

        [Fact]
        public void CleanMessage_DirectionalMarks_Stripped()
        {
            Assert.Equal("Hello", MessageUtils.CleanMessage("\u200EHello\u200F"));
        }

        [Fact]
        public void CleanMessage_NewlinePreserved()
        {
            Assert.Equal("Line1\nLine2", MessageUtils.CleanMessage("Line1\nLine2"));
        }

        [Fact]
        public void CleanMessage_TabPreserved()
        {
            Assert.Equal("Col1\tCol2", MessageUtils.CleanMessage("Col1\tCol2"));
        }

        [Fact]
        public void CleanMessage_C0ControlChars_Stripped()
        {
            Assert.Equal("Hello", MessageUtils.CleanMessage("He\x01l\x02lo"));
        }

        [Fact]
        public void CleanMessage_NullChar_Stripped()
        {
            Assert.Equal("Hello", MessageUtils.CleanMessage("He\x00llo"));
        }

        [Fact]
        public void CleanMessage_EmptyString()
        {
            Assert.Equal("", MessageUtils.CleanMessage(""));
        }

        [Fact]
        public void CleanMessage_NullInput()
        {
            Assert.Equal("", MessageUtils.CleanMessage(null));
        }

        [Fact]
        public void CleanMessage_OnlyEmojis_ReturnsEmpty()
        {
            Assert.Equal("", MessageUtils.CleanMessage("\U0001F600\U0001F601\U0001F602"));
        }

        [Fact]
        public void CleanMessage_VariationSelectors_Stripped()
        {
            Assert.Equal("", MessageUtils.CleanMessage("\uFE0F"));
        }

        [Fact]
        public void CleanMessage_MahjongTiles_Stripped()
        {
            Assert.Equal("tile: ", MessageUtils.CleanMessage("tile: \U0001F004"));
        }

        [Fact]
        public void CleanMessage_Dingbats_Stripped()
        {
            Assert.Equal("check ", MessageUtils.CleanMessage("check \u2714"));
        }

        [Fact]
        public void CleanMessage_MixedContent()
        {
            // Arabic text + Arabic digits + emoji + HTML
            var input = "\u0645\u0631\u062D\u0628\u0627 \u0661\u0662\u0663 \U0001F600 <b>bold</b>";
            var expected = "\u0645\u0631\u062D\u0628\u0627 123  bold";
            Assert.Equal(expected, MessageUtils.CleanMessage(input));
        }
    }
}
