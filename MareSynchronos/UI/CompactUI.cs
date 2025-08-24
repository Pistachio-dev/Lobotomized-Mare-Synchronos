using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using MareSynchronos.API.Dto.CharaData;
using MareSynchronos.MareConfiguration;
using MareSynchronos.MareConfiguration.Models;
using MareSynchronos.PlayerData.Pairs;
using MareSynchronos.Services;
using MareSynchronos.Services.CharaData;
using MareSynchronos.Services.CharaData.Models;
using MareSynchronos.Services.Mediator;
using MareSynchronos.Services.ServerConfiguration;
using MareSynchronos.Utils;
using Microsoft.Extensions.Logging;

namespace MareSynchronos.UI;

internal sealed partial class CompactUi : WindowMediatorSubscriberBase
{
    private const int maxPoses = 10;
    private readonly CharaDataManager _charaDataManager;
    private readonly CharaDataConfigService _configService;
    private readonly ILogger<CompactUi> _logger;
    private readonly UiSharedService _uiSharedService;

    private string _exportDescription = string.Empty;
 
    private bool _readExport;

    public CompactUi(ILogger<CompactUi> logger, MareMediator mediator,
                         CharaDataManager charaDataManager, CharaDataConfigService configService,
                         UiSharedService uiSharedService, PerformanceCollectorService performanceCollectorService)
        : base(logger, mediator, "Mare Synchronos Character Data Hub###MareSynchronosCharaDataUI", performanceCollectorService)
    {
        _logger = logger;
        _charaDataManager = charaDataManager;
        _configService = configService;
        _uiSharedService = uiSharedService;
    }

    protected override void DrawInternal()
    {
        DrawMcdfExport();
    }

    private void DrawMcdfExport()
    {
        _uiSharedService.BigText("Mare Character Data File Export");

        DrawHelpFoldout("This feature allows you to pack your character into a MCDF file and manually send it to other people. MCDF files can officially only be imported during GPose through Mare. " +
            "Be aware that the possibility exists that people write unofficial custom exporters to extract the containing data.");

        ImGuiHelpers.ScaledDummy(5);

        ImGui.Checkbox("##readExport", ref _readExport);
        ImGui.SameLine();
        UiSharedService.TextWrapped("I understand that by exporting my character data into a file and sending it to other people I am giving away my current character appearance irrevocably. People I am sharing my data with have the ability to share it with other people without limitations.");

        if (_readExport)
        {
            ImGui.Indent();

            ImGui.InputTextWithHint("Export Descriptor", "This description will be shown on loading the data", ref _exportDescription, 255);
            if (_uiSharedService.IconTextButton(FontAwesomeIcon.Save, "Export Character as MCDF"))
            {
                string defaultFileName = string.IsNullOrEmpty(_exportDescription)
                    ? "export.mcdf"
                    : string.Join('_', $"{_exportDescription}.mcdf".Split(Path.GetInvalidFileNameChars()));
                _uiSharedService.FileDialogManager.SaveFileDialog("Export Character to file", ".mcdf", defaultFileName, ".mcdf", (success, path) =>
                {
                    if (!success) return;

                    _configService.Current.LastSavedCharaDataLocation = Path.GetDirectoryName(path) ?? string.Empty;
                    _configService.Save();

                    _charaDataManager.SaveMareCharaFile(_exportDescription, path);
                    _exportDescription = string.Empty;
                }, Directory.Exists(_configService.Current.LastSavedCharaDataLocation) ? _configService.Current.LastSavedCharaDataLocation : null);
            }
            UiSharedService.ColorTextWrapped("Note: For best results make sure you have everything you want to be shared as well as the correct character appearance" +
                " equipped and redraw your character before exporting.", ImGuiColors.DalamudYellow);

            ImGui.Unindent();
        }
    }

    private void DrawHelpFoldout(string text)
    {
        if (_configService.Current.ShowHelpTexts)
        {
            ImGuiHelpers.ScaledDummy(5);
            UiSharedService.DrawTree("What is this? (Explanation / Help)", () =>
            {
                UiSharedService.TextWrapped(text);
            });
        }
    }


}