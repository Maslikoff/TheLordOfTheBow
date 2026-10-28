#if YandexGamesPlatform_yg
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace YG.EditorScr.BuildModify
{
    public partial class ModifyBuild
    {
        private const string LoadingBackgroundStart = "// YG_LOADING_BACKGROUND_START";
        private const string LoadingBackgroundEnd = "// YG_LOADING_BACKGROUND_END";
        private const string LegacyLoadingBackground =
            @"loadingCover.style.background = ""url('Images/background.png') center / cover"";";

        public static void SetBackgroundFormat()
        {
            bool hasMarkedBlock = indexFile.Contains(LoadingBackgroundStart) && indexFile.Contains(LoadingBackgroundEnd);
            bool hasLegacyLine = indexFile.Contains(LegacyLoadingBackground);

            if (!hasMarkedBlock && !hasLegacyLine)
            {
                Debug.LogWarning("Search string not found in index.html");
                return;
            }

            if (infoYG.Templates.backgroundImgFormat == InfoYG.TemplatesSettings.BackgroundImageFormat.PNG)
            {
                EnsureDualBackgroundBlock();
                DeleteImage("jpg");
                DeleteImage("gif");
            }
            else if (infoYG.Templates.backgroundImgFormat == InfoYG.TemplatesSettings.BackgroundImageFormat.JPG)
            {
                EnsureDualBackgroundBlock("jpg");
                DeleteImage("png");
                DeleteImage("gif");
            }
            else if (infoYG.Templates.backgroundImgFormat == InfoYG.TemplatesSettings.BackgroundImageFormat.GIF)
            {
                EnsureDualBackgroundBlock("gif");
                DeleteImage("png");
                DeleteImage("jpg");
            }
            else if (infoYG.Templates.backgroundImgFormat == InfoYG.TemplatesSettings.BackgroundImageFormat.Unity)
            {
                ReplaceLoadingBackgroundBlock("canvas.style.background = backgroundUnity;");
                DeleteImage("png");
                DeleteImage("jpg");
                DeleteImage("gif");
                DeleteDesktopImage("png");
                DeleteDesktopImage("jpg");
                DeleteDesktopImage("gif");
            }
            else
            {
                ReplaceLoadingBackgroundBlock(string.Empty);
                DeleteImage("png");
                DeleteImage("jpg");
                DeleteImage("gif");
                DeleteDesktopImage("png");
                DeleteDesktopImage("jpg");
                DeleteDesktopImage("gif");
            }

            void EnsureDualBackgroundBlock(string format = "png")
            {
                string block =
                    $"{LoadingBackgroundStart}\n" +
                    $"        loadingCover.style.backgroundImage = \"url('\" + (isMobileDevice ? \"Images/background.{format}\" : \"Images/background-desktop.{format}\") + \"')\";\n" +
                    "        loadingCover.style.backgroundPosition = \"center\";\n" +
                    "        loadingCover.style.backgroundSize = \"cover\";\n" +
                    "        loadingCover.style.backgroundRepeat = \"no-repeat\";\n" +
                    $"        {LoadingBackgroundEnd}";

                ReplaceLoadingBackgroundBlock(block);
            }

            void ReplaceLoadingBackgroundBlock(string replacement)
            {
                if (indexFile.Contains(LoadingBackgroundStart) && indexFile.Contains(LoadingBackgroundEnd))
                {
                    string pattern = Regex.Escape(LoadingBackgroundStart) + ".*?" + Regex.Escape(LoadingBackgroundEnd);
                    indexFile = Regex.Replace(indexFile, pattern, replacement.TrimEnd(), RegexOptions.Singleline);
                    return;
                }

                if (indexFile.Contains(LegacyLoadingBackground))
                    indexFile = indexFile.Replace(LegacyLoadingBackground, replacement);
            }

            void DeleteImage(string format)
            {
                string pathImage = $"{ProcessBuild.BuildPath}/Images/background.{format}";

                if (File.Exists(pathImage))
                    FileYG.Delete(pathImage);
            }

            void DeleteDesktopImage(string format)
            {
                string pathImage = $"{ProcessBuild.BuildPath}/Images/background-desktop.{format}";

                if (File.Exists(pathImage))
                    FileYG.Delete(pathImage);
            }
        }

    }
}
#endif
