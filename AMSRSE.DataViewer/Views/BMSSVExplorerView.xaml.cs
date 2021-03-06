﻿using AMSRSE.BMSSV;
using AMSRSE.DataViewer.Commands;
using AMSRSE.DataViewer.Controls;
using AMSRSE.DataViewer.DataModels;
using Magatama.Core.DataModels;
using Magatama.Core.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AMSRSE.DataViewer.Views
{
    public class BMSSVExplorerView : ViewBase
    {
        #region Enums

        public enum FileComparisonSides
        {
            SideA,
            SideB
        }

        #endregion Enums

        #region Dependency Properties

        public static readonly DependencyProperty LoadedFileProperty =
            DependencyProperty.Register("LoadedFile", typeof(FileModel), typeof(BMSSVExplorerView));

        public static readonly DependencyProperty SelectedBlockProperty =
            DependencyProperty.Register("SelectedBlock", typeof(BlockModel), typeof(BMSSVExplorerView));

        public static readonly DependencyPropertyKey LoadFileCommandPropertyKey =
            DependencyProperty.RegisterReadOnly("LoadFileCommand", typeof(DelegateCommand), typeof(BMSSVExplorerView), null);

        public static readonly DependencyProperty LoadFileCommandProperty =
            LoadFileCommandPropertyKey.DependencyProperty;

        public static readonly DependencyProperty CanEditProperty =
            DependencyProperty.Register("CanEdit", typeof(bool), typeof(BMSSVExplorerView));

        public static readonly DependencyProperty FileComparisonResultsProperty =
            DependencyProperty.Register("FileComparisonResults", typeof(EditableModelChangeHistoryEntry), typeof(BMSSVExplorerView));

        public static readonly DependencyProperty FileComparisonSideProperty =
            DependencyProperty.Register("FileComparisonSide", typeof(FileComparisonSides), typeof(BMSSVExplorerView));

        #endregion Dependency Properties

        #region Properties

        public FileModel LoadedFile
        {
            get { return (FileModel)GetValue(LoadedFileProperty); }
            set { SetValue(LoadedFileProperty, value); }
        }

        public BlockModel SelectedBlock
        {
            get { return (BlockModel)GetValue(SelectedBlockProperty); }
            set { SetValue(SelectedBlockProperty, value); }
        }

        public DelegateCommand LoadFileCommand
        {
            get { return (DelegateCommand)GetValue(LoadFileCommandProperty); }
            private set { SetValue(LoadFileCommandPropertyKey, value); }
        }

        public bool CanEdit
        {
            get { return (bool)GetValue(CanEditProperty); }
            set { SetValue(CanEditProperty, value); }
        }

        public EditableModelChangeHistoryEntry FileComparisonResults
        {
            get { return (EditableModelChangeHistoryEntry)GetValue(FileComparisonResultsProperty); }
            set { SetValue(FileComparisonResultsProperty, value); }
        }

        public FileComparisonSides FileComparisonSide
        {
            get { return (FileComparisonSides)GetValue(FileComparisonSideProperty); }
            set { SetValue(FileComparisonSideProperty, value); }
        }

        #endregion Properties

        #region Ctor

        public BMSSVExplorerView()
        {
            LoadFileCommand = new DelegateCommand((o) => { LoadFile(); });

            InitializeComponent(
                xamlPath: new Uri("/AMSRSE.DataViewer;component/Views/BMSSVExplorerView.xaml", UriKind.Relative));
        }

        #endregion Ctor

        #region Methods

        private void LoadFile()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "BMSSV Files|*.bmssv",
                Title = "Choose BMSSV . . .",
                RestoreDirectory = true
            };

            if (ofd.ShowDialog() == true)
            {
                BMSSVFile bmssv = BMSSVFile.Open(ofd.FileName);
                LoadedFile = FileModel.FromBMSSV(bmssv);

                //this.DataContext = LoadedFile;
            }
        }

        #endregion Methods
    }
}
