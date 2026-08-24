/*
 * OpenRose - Requirements Management
    * Licensed under the Apache License, Version 2.0.
 * See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.
*/

/*
 * EXPLANATION:
 * This script enables OpenRose to reliably detect browser‑level navigation
 * events that Blazor cannot observe on its own. Blazor internal navigation
 * (NavigationManager.NavigateTo) does NOT unload the page, so it never fires
 * browser lifecycle events such as `beforeunload`. However, a full page
 * refresh, tab close, window close, or external navigation DOES unload the
 * document and triggers `beforeunload`.
 *
 * When `beforeunload` fires, we use the modern Navigation API
 * (performance.getEntriesByType("navigation")[0].type) to determine the
 * specific reason the browser is leaving the page:
 *
 *   - "reload"       → user refreshed the page
 *   - "navigate"     → user navigated to a different URL outside Blazor
 *   - "back_forward" → browser history navigation
 *
 * We then pass this navigation type into Blazor via a .NET callback
 * (`OnBrowserNavigation`). Blazor uses this information to decide whether
 * refresh‑recovery state should be preserved (on reload) or cleared (on
 * external navigation, tab close, or history navigation).
 *
 * IMPORTANT:
 * This script ONLY detects browser‑level unload events. It does NOT fire
 * for Blazor internal navigation, which must be handled separately using
 * NavigationManager.LocationChanged inside the Blazor application.
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

