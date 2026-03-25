using System.Text.Json;
using Xunit.Abstractions;

namespace NeeView.UnitTest
{
    public class ConfigCompatible
    {
        private readonly ITestOutputHelper _output;
        
        public ConfigCompatible(ITestOutputHelper testOutputHelper)
        {
            _output = testOutputHelper;
        }


        [Fact]
        public void AutoHideConfigCompatible()
        {
            var options = UserSettingTools.GetDeserializeOptions();

            string json = """
            {
                  "AutoHideConfrictTopMargin": "AllowPixel",
                  "AutoHideConfrictBottomMargin": "Deny"
            }
            """;

#pragma warning disable CS0618 // 型またはメンバーが旧型式です
            // ver.41
            var config = JsonSerializer.Deserialize<AutoHideConfig>(json, options);
            Assert.NotNull(config);
            Assert.Equal(AutoHideConflictMode.AllowPixel, config.AutoHideConflictTopMargin);
            Assert.Equal(AutoHideConflictMode.AllowPixel, config.AutoHideConfrictTopMargin);
            Assert.Equal(AutoHideConflictMode.Allow, config.AutoHideConfrictTopMargin_Typo);
            Assert.Equal(AutoHideConflictMode.Deny, config.AutoHideConflictBottomMargin);
            Assert.Equal(AutoHideConflictMode.Deny, config.AutoHideConfrictBottomMargin);
            Assert.Equal(AutoHideConflictMode.Allow, config.AutoHideConfrictBottomMargin_Typo);
#pragma warning restore CS0618 // 型またはメンバーが旧型式です
        }

        [Fact]
        public void SystemConfigCompatible()
        {
            var options = UserSettingTools.GetDeserializeOptions();

            string json = """
            {
                  "IsHiddenFileVisibled": true,
                  "IsOpenbookAtCurrentPlace": true,
                  "DestinationFodlerCollection":[
                      {
                        "Name": "Pictures",
                        "Path": "C:\\Pictures"
                      }
                   ],
            }
            """;

#pragma warning disable CS0618 // 型またはメンバーが旧型式です
#pragma warning disable xUnit2004 // Do not use equality check to test for boolean conditions
#pragma warning disable xUnit2003 // Do not use equality check to test for null value
            // ver.41
            var config = JsonSerializer.Deserialize<SystemConfig>(json, options);
            Assert.NotNull(config);
            Assert.Equal(true, config.IsHiddenFileVisible);
            Assert.Equal(true, config.IsHiddenFileVisibled);
            Assert.Equal(false, config.IsHiddenFileVisibled_Typo);
            Assert.Equal(true, config.IsOpenBookAtCurrentPlace);
            Assert.Equal(true, config.IsOpenbookAtCurrentPlace);
            Assert.Equal(false, config.IsOpenbookAtCurrentPlace_Typo);
            Assert.Single(config.DestinationFolderCollection);
            Assert.Equal("Pictures", config.DestinationFolderCollection[0].Name);
            Assert.Equal("C:\\Pictures", config.DestinationFolderCollection[0].Path);
            Assert.Single(config.DestinationFodlerCollection);
            Assert.Equal("Pictures", config.DestinationFodlerCollection[0].Name);
            Assert.Equal("C:\\Pictures", config.DestinationFodlerCollection[0].Path);
            Assert.Equal(null, config.DestinationFodlerCollection_Typo);
#pragma warning restore xUnit2003 // Do not use equality check to test for null value
#pragma warning restore xUnit2004 // Do not use equality check to test for boolean conditions
#pragma warning restore CS0618 // 型またはメンバーが旧型式です
        }
    }
}
