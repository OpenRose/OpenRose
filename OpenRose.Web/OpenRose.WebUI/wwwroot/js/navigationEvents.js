/*
 * OpenRose - Requirements Management
    * Licensed under the Apache License, Version 2.0.
 * See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.
*/

window.OpenRoseNavigation = {
    registerNavigationHandler: function (dotNetObj) {
        window.addEventListener("beforeunload", function (event) {
            const navEntry = performance.getEntriesByType("navigation")[0];
            const navType = navEntry ? navEntry.type : "navigate";

            // Send navigation type to .NET
            dotNetObj.invokeMethodAsync("OnBrowserNavigation", navType);
        });
    }
};

