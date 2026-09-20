using Microsoft.Extensions.Localization;
using Xhj.Project.Localization;
using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace Xhj.Project;

[Dependency(ReplaceServices = true)]
public class ProjectBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<ProjectResource> _localizer;

    public ProjectBrandingProvider(IStringLocalizer<ProjectResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
