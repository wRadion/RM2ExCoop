using Microsoft.Win32;
using RM2ExCoop.RM2C;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace RM2ExCoop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string _romPath;

        public MainWindow()
        {
            InitializeComponent();
            _romPath = string.Empty;
        }

        private void RM2CBtn_Click(object sender, RoutedEventArgs e)
        {
            bool?[] levelsBool = new bool?[]
            {
                null, null, null, null,
                BbhCheck.IsChecked, CcmCheck.IsChecked, CastInsideCheck.IsChecked,
                HmcCheck.IsChecked, SslCheck.IsChecked, BobCheck.IsChecked,
                SlCheck.IsChecked, WdwCheck.IsChecked, JrbCheck.IsChecked,
                ThiCheck.IsChecked, TtcCheck.IsChecked, RrCheck.IsChecked,
                CastGroundsCheck.IsChecked, BitdwCheck.IsChecked, VcutmCheck.IsChecked,
                BitfsCheck.IsChecked, TsaCheck.IsChecked, BitsCheck.IsChecked,
                LllCheck.IsChecked, DddCheck.IsChecked, WfCheck.IsChecked,
                true, CastCourtyardCheck.IsChecked, PssCheck.IsChecked,
                CotmcCheck.IsChecked, TotwcCheck.IsChecked, Bow1Check.IsChecked,
                WmotrCheck.IsChecked, null, Bow2Check.IsChecked, Bow3Check.IsChecked,
                null, TtmCheck.IsChecked
            };

            List<int> levels = new();

            if (!(AllLevelsCheck.IsChecked ?? false))
            {
                for (int i = 0; i < levelsBool.Length; ++i)
                {
                    if (levelsBool[i] ?? false)
                        levels.Add(i);
                }
            }

            if (!uint.TryParse(MusicExtendInput.Text, out uint musicExtend))
                musicExtend = 0;

            Options options = new()
            {
                Levels = new LevelsOption(levels.ToArray()),
                Actors = new ActorsOption((ExportActorsCheck.IsChecked ?? false) ? ActorsOptionType.ALL : ActorsOptionType.NONE),
                Objects = new ObjectsOption((ExportObjectsCheck.IsChecked ?? false) ? ObjectsOptionType.ALL : ObjectsOptionType.NONE),
                Text = ExportTextCheck.IsChecked ?? false,
                Misc = ExportMiscCheck.IsChecked ?? false,
                Segment2 = ExportSegment2Check.IsChecked ?? false,
                Skyboxes = ExportSkyboxesCheck.IsChecked ?? false,
                Editor = EditorCheck.IsChecked ?? false,
                Music = ExportMusicsCheck.IsChecked ?? false,
                MusicExtend = musicExtend
            };

            ClearLogs();
            DisableButtons();
            _ = Task.Run(() =>
            {
                try
                {
                    RM2C.Main.Run(_romPath, options);
                    MessageBox.Show("RM2C done!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception e)
                {
                    Dispatcher.BeginInvoke(() => Log(e.GetType().Name + ":\n\t" + e.Message + "\n\n" + e.StackTrace, LogType.ERROR));
                    MessageBox.Show("There was an error that ended RM2C sooner that expected. Generated C files may be incomplete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Dispatcher.BeginInvoke(EnableButtons);
            });
        }

        public void ClearLogs()
        {
            LogsBox.Document.Blocks.Clear();
        }

        public enum LogType { INFO, WARN, ERROR, DEBUG }
        public void Log(string text, LogType type = LogType.INFO)
        {
            Color c = type switch
            {
                LogType.WARN => Colors.DarkGoldenrod,
                LogType.ERROR => Colors.Red,
                LogType.DEBUG => Colors.CornflowerBlue,
                _ => Colors.Black
            };

            Run r = new(text)
            {
                Foreground = new SolidColorBrush(c)
            };
            Paragraph p = new(r);

            LogsDocument.Blocks.Add(p);
            LogsBox.ScrollToEnd();
        }

        private void C2ExCoopBtn_Click(object sender, RoutedEventArgs e)
        {
            string modName = ModNameInput.Text;
            string modDesc = ModDescInput.Text;
            bool commentSOM = CommentSOMCheck.IsChecked ?? false;
            bool removeFlags = RemoveFlagsCheck.IsChecked ?? false;
            bool removePaintings = RemovePaintingCheck.IsChecked ?? false;
            bool removeTrajectories = RemoveTrajectoriesCheck.IsChecked ?? false;
            bool tryFixFog = TryFixFogCheck.IsChecked ?? false;
            bool dontUseCameraSpecific = DontUseCameraSpecificCheck.IsChecked ?? false;
            string entryLevel = (string)((ComboBoxItem)EntryLevelSelect.SelectedValue).Content;

            ClearLogs();
            DisableButtons();
            Task.Run(() =>
            {
                try
                {
                    C2ExCoop.Main.Run(modName, modDesc, commentSOM, removeFlags, removePaintings, removeTrajectories, tryFixFog, dontUseCameraSpecific, entryLevel);
                    MessageBox.Show("C2ExCoop done!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception e)
                {
                    Dispatcher.BeginInvoke(() => Log(e.GetType().Name + ":\n\t" + e.Message + "\n\n" + e.StackTrace, LogType.ERROR));
                    MessageBox.Show("There was an error that ended C2ExCoop sooner that expected. Generated Lua files may be incomplete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Dispatcher.BeginInvoke(EnableButtons);
            });
        }

        void DisableButtons()
        {
            RM2CBtn.IsEnabled = false;
            C2ExCoopBtn.IsEnabled = false;
        }

        void EnableButtons()
        {
            RM2CBtn.IsEnabled = true;
            C2ExCoopBtn.IsEnabled = true;
        }

        private void OpenRomBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                CheckFileExists = true,
                Filter = "N64 ROM Files (*.z64)|*.z64",
                Multiselect = false,
                DefaultExt = "*.z64",
                Title = "Open a N64 ROM file"
            };
            
            if (dialog.ShowDialog() ?? false)
            {
                _romPath = dialog.FileName;
                RomPathText.Text = dialog.SafeFileName;
                ModNameInput.Text = dialog.SafeFileName[..dialog.SafeFileName.LastIndexOf('.')];
                RM2CBtn.IsEnabled = true;
            }
        }

        void EnableAllLevels(bool enable)
        {
            BobCheck.IsEnabled = enable;
            WfCheck.IsEnabled = enable;
            JrbCheck.IsEnabled = enable;
            CcmCheck.IsEnabled = enable;
            BbhCheck.IsEnabled = enable;
            HmcCheck.IsEnabled = enable;
            LllCheck.IsEnabled = enable;
            SslCheck.IsEnabled = enable;
            DddCheck.IsEnabled = enable;
            SlCheck.IsEnabled = enable;
            WdwCheck.IsEnabled = enable;
            TtmCheck.IsEnabled = enable;
            ThiCheck.IsEnabled = enable;
            TtcCheck.IsEnabled = enable;
            RrCheck.IsEnabled = enable;
            CastGroundsCheck.IsEnabled = enable;
            CastInsideCheck.IsEnabled = enable;
            CastCourtyardCheck.IsEnabled = enable;
            TotwcCheck.IsEnabled = enable;
            VcutmCheck.IsEnabled = enable;
            CotmcCheck.IsEnabled = enable;
            PssCheck.IsEnabled = enable;
            TsaCheck.IsEnabled = enable;
            WmotrCheck.IsEnabled = enable;
            BitdwCheck.IsEnabled = enable;
            BitfsCheck.IsEnabled = enable;
            BitsCheck.IsEnabled = enable;
            Bow1Check.IsEnabled = enable;
            Bow2Check.IsEnabled = enable;
            Bow3Check.IsEnabled = enable;
        }

        void CheckAllLevels(bool check)
        {
            BobCheck.IsChecked = check;
            WfCheck.IsChecked = check;
            JrbCheck.IsChecked = check;
            CcmCheck.IsChecked = check;
            BbhCheck.IsChecked = check;
            HmcCheck.IsChecked = check;
            LllCheck.IsChecked = check;
            SslCheck.IsChecked = check;
            DddCheck.IsChecked = check;
            SlCheck.IsChecked = check;
            WdwCheck.IsChecked = check;
            TtmCheck.IsChecked = check;
            ThiCheck.IsChecked = check;
            TtcCheck.IsChecked = check;
            RrCheck.IsChecked = check;
            CastGroundsCheck.IsChecked = check;
            CastInsideCheck.IsChecked = check;
            CastCourtyardCheck.IsChecked = check;
            TotwcCheck.IsChecked = check;
            VcutmCheck.IsChecked = check;
            CotmcCheck.IsChecked = check;
            PssCheck.IsChecked = check;
            TsaCheck.IsChecked = check;
            WmotrCheck.IsChecked = check;
            BitdwCheck.IsChecked = check;
            BitfsCheck.IsChecked = check;
            BitsCheck.IsChecked = check;
            Bow1Check.IsChecked = check;
            Bow2Check.IsChecked = check;
            Bow3Check.IsChecked = check;
        }

        private void CheckAllLevelsBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckAllLevels(true);
        }

        private void UncheckAllLevelsBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckAllLevels(false);
        }

        private void AllLevelsCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            CheckAllLevels(true);
            EnableAllLevels(false);
            CheckAllLevelsBtn.IsEnabled = false;
            UncheckAllLevelsBtn.IsEnabled = false;
        }

        private void AllLevelsCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            EnableAllLevels(true);
            CheckAllLevelsBtn.IsEnabled = true;
            UncheckAllLevelsBtn.IsEnabled = true;
        }
    }
}
