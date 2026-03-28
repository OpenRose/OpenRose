/*
 * OpenRose - Requirements Management
    * Licensed under the Apache License, Version 2.0.
 * See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.
*/

window.openroseNavigationBlocker = {
    enable: function () {
        // Block all links
        document.querySelectorAll(".markdown-body a").forEach(a => {
            a.dataset.href = a.getAttribute("href"); // store original
            a.removeAttribute("href");
            a.style.pointerEvents = "none";
            a.style.cursor = "default";
        });

        // Block iframes (YouTube, Vimeo, etc.)
        document.querySelectorAll(".markdown-body iframe").forEach(f => {
            f.style.pointerEvents = "none";
        });
    },

    disable: function () {
        // Restore links
        document.querySelectorAll(".markdown-body a").forEach(a => {
            if (a.dataset.href) {
                a.setAttribute("href", a.dataset.href);
            }
            a.style.pointerEvents = "auto";
            a.style.cursor = "pointer";
        });

        // Restore iframes
        document.querySelectorAll(".markdown-body iframe").forEach(f => {
            f.style.pointerEvents = "auto";
        });
    }
};
