﻿using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    public class SettingPageExternal : SettingPage
    {
        public SettingPageExternal() : base(Properties.Resources.SettingPageExternal)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageExternalGeneral(),
                new SettingPageExternalProgram(),
            };
        }
    }

    public class SettingPageExternalGeneral : SettingPage
    {
        public SettingPageExternalGeneral() : base(Properties.Resources.SettingPageExternalGeneral)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageExternalGeneralCopyToClipboard,
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ClipboardUtility, nameof(ClipboardUtility.MultiPageOption))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ClipboardUtility, nameof(ClipboardUtility.ArchiveOption)))),

                new SettingItemSection(Properties.Resources.SettingPageExternalGeneralFromBrowser,
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.DownloadPath))) { IsStretch = true }),

                new SettingItemSection(Properties.Resources.SettingPageExternalGeneralSave, Properties.Resources.SettingPageExternalGeneralSaveTips,
                    new SettingItemProperty(PropertyMemberElement.Create(ExporterProfile.Current, nameof(ExporterProfile.IsEnableExportFolder))),
                    new SettingItemProperty(PropertyMemberElement.Create(ExporterProfile.Current, nameof(ExporterProfile.QualityLevel)))),
            };
        }
    }
    public class SettingPageExternalProgram : SettingPage
    {
        public SettingPageExternalProgram() : base(Properties.Resources.SettingPageExternalProgram)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageExternalProgramSetting,
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ProgramType))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.Command))) { IsStretch = true },
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.Parameter))) { IsStretch = true })
                    {
                        VisibleTrigger = new DataTriggerSource(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ProgramType), ExternalProgramType.Normal, true),
                    },
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.Protocol))) { IsStretch = true })
                    {
                        VisibleTrigger = new DataTriggerSource(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ProgramType), ExternalProgramType.Protocol, true),
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.MultiPageOption))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ArchiveOption)))),
            };
        }
    }
}
