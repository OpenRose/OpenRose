// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ItemzApp.API.Helper
{
    public static class ControllerAndActionNames
    {
        public static string GetFormattedControllerAndActionNames(ControllerContext controllerContext)
        {
            if(controllerContext.ActionDescriptor.ControllerName is not null &&
                controllerContext.ActionDescriptor.ActionName is not null)
                return $"::{controllerContext.ActionDescriptor.ControllerName}-{controllerContext.ActionDescriptor.ActionName}:: ";
            
            return "";
        }
    }
}
