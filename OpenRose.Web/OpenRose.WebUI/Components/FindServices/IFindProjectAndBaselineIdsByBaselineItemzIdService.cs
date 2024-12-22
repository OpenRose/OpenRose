// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Components.FindServices
{
    public interface IFindProjectAndBaselineIdsByBaselineItemzIdService
    {
        Task<(Guid ProjectId, Guid BaselineId)> GetProjectAndBaselineId(Guid baselineItemzId);
    }
}
