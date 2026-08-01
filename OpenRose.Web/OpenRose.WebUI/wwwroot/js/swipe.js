/*
 * OpenRose - Requirements Management
    * Licensed under the Apache License, Version 2.0.
 * See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.
*/

// Production-ready, robust touch/pointer swipe detection (document-level, capture-phase).


(function () {
    if (window.openRoseSwipeLibLoaded) {
        return;
    }
    window.openRoseSwipeLibLoaded = true;
    window.openRoseHandlers = window.openRoseHandlers || {};

    function registerOnElement(el, dotNetRef, elementKey) {
        if (!el) return false;

        // Hint browser how to treat touch gestures:
        // pan-y allows vertical scrolling while enabling horizontal gestures to be used by JS.
        try { el.style.touchAction = 'pan-y'; } catch (e) { }

        var startX = 0, startY = 0, startTime = 0;
        var threshold = 50; // pixels min for swipe
        var restraint = 120; // max y delta
        var allowedTime = 600; // ms

        function onStart(e) {
            var p = (e.touches && e.touches[0]) || e;
            startX = p.clientX;
            startY = p.clientY;
            startTime = Date.now();
        }

        function onEnd(e) {
            var p = (e.changedTouches && e.changedTouches[0]) || e;
            var distX = p.clientX - startX;
            var distY = p.clientY - startY;
            var elapsed = Date.now() - startTime;

            if (elapsed <= allowedTime && Math.abs(distX) >= threshold && Math.abs(distY) <= restraint) {
                if (distX < 0) {
                    dotNetRef.invokeMethodAsync('OnSwipe', 'left').catch(function () { /* ignore */ });
                } else {
                    dotNetRef.invokeMethodAsync('OnSwipe', 'right').catch(function () { /* ignore */ });
                }
            }
        }

        // Document-level capture handlers that filter by el.contains(target)
        function makeDocPointerDown() {
            return function (ev) {
                // only start if gesture begins inside element
                try {
                    if (el.contains(ev.target)) onStart(ev);
                } catch (e) { /* ignore */ }
            };
        }
        function makeDocPointerUp() {
            return function (ev) {
                try {
                    if (el.contains(ev.target)) onEnd(ev);
                } catch (e) { /* ignore */ }
            };
        }

        var handlers = {
            pointerDown: makeDocPointerDown(),
            pointerUp: makeDocPointerUp(),
            touchStart: makeDocPointerDown(),
            touchEnd: makeDocPointerUp(),
            mouseDown: makeDocPointerDown(),
            mouseUp: makeDocPointerUp()
        };

        var uses = [];

        // Prefer pointer events where available
        if (window.PointerEvent) {
            document.addEventListener('pointerdown', handlers.pointerDown, { passive: true, capture: true });
            document.addEventListener('pointerup', handlers.pointerUp, { passive: true, capture: true });
            uses.push('pointer');
        } else {
            // touch
            document.addEventListener('touchstart', handlers.touchStart, { passive: true, capture: true });
            document.addEventListener('touchend', handlers.touchEnd, { passive: true, capture: true });
            // mouse as fallback
            document.addEventListener('mousedown', handlers.mouseDown, { passive: true, capture: true });
            document.addEventListener('mouseup', handlers.mouseUp, { passive: true, capture: true });
            uses.push('touch', 'mouse');
        }

        window.openRoseHandlers[elementKey] = {
            el: el,
            handlers: handlers,
            uses: uses,
            dotNetRef: dotNetRef
        };

        return true;
    }

    window.openRoseRegisterSwipeElement = function (element, dotNetRef) {
        try {
            if (!element) return false;
            var elementKey = element.dataset && element.dataset.openroseId ? element.dataset.openroseId : ("el-" + Math.random().toString(36).substr(2, 9));
            try { element.dataset.openroseId = elementKey; } catch (e) { }
            return registerOnElement(element, dotNetRef, elementKey);
        } catch (err) {
            return false;
        }
    };

    window.openRoseUnregisterSwipeElement = function (element) {
        try {
            if (!element) return false;
            var elementKey = element.dataset && element.dataset.openroseId ? element.dataset.openroseId : null;
            if (!elementKey) return false;
            var rec = window.openRoseHandlers[elementKey];
            if (!rec) return false;

            var handlers = rec.handlers || {};
            var uses = rec.uses || [];

            try {
                if (uses.indexOf('pointer') >= 0) {
                    document.removeEventListener('pointerdown', handlers.pointerDown, { capture: true });
                    document.removeEventListener('pointerup', handlers.pointerUp, { capture: true });
                } else {
                    document.removeEventListener('touchstart', handlers.touchStart, { capture: true });
                    document.removeEventListener('touchend', handlers.touchEnd, { capture: true });
                    document.removeEventListener('mousedown', handlers.mouseDown, { capture: true });
                    document.removeEventListener('mouseup', handlers.mouseUp, { capture: true });
                }
            } catch (er) { /* ignore */ }

            delete window.openRoseHandlers[elementKey];
            try { delete element.dataset.openroseId; } catch (e) { }
            return true;
        } catch (err) {
            return false;
        }
    };
})();