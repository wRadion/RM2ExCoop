using System.IO;
using System.Text.RegularExpressions;

namespace RM2ExCoop.C2ExCoop
{
    internal class TextLuasGenerator
    {
        readonly string _dialogsPath;
        readonly string _coursesPath;

        public TextLuasGenerator(string dialogsPath, string coursesPath)
        {
            _dialogsPath = dialogsPath;
            _coursesPath = coursesPath;
        }

        public void Generate(string outputDir)
        {
            // Dialog.h
            new FileObject(_dialogsPath).
                Replace(new Regex("DEFINE_DIALOG"), "smlua_text_utils_dialog_replace").
                Replace(new Regex("_\\("), "(").
                Replace(new Regex("\\\\n\\\\"), "\\")
                .ApplyAndSave(Path.Join(outputDir, "dialogs.lua"));

            // Courses.h
            new FileObject(_coursesPath).
                Replace(new Regex("COURSE_ACTS"), "smlua_text_utils_course_acts_replace").
                Replace(new Regex("CASTLE_SECRET_STARS"), "smlua_text_utils_castle_secret_stars_replace").
                Replace(new Regex("SECRET_STAR"), "smlua_text_utils_secret_star_replace").
                Replace(new Regex("EXTRA_TEXT"), "smlua_text_utils_extra_text_replace").
                Replace(new Regex("_\\("), "(")
                .ApplyAndSave(Path.Join(outputDir, "courses.lua"));
        }
    }
}
