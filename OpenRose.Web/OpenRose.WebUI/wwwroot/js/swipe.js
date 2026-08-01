/*
 * OpenRose - Requirements Management
    * Licensed under the Apache License, Version 2.0.
 * See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.
*/

// Hybrid approach: element-level start + global end handlers.
// - Preserves button clicks (no pointer capture).
// - Detects single-finger swipes reliably on mobile.
// - Preserves multi-touch pinch/zoom (we ignore multi-touch).
// - Default sensitivity: threshold=50px, restraint=120px, allowedTime=600ms.
// Set debug = true to enable console logging for troubleshooting.

(function () {
    if (window.openRoseSwipeLibLoaded) {
        return;
    }
    window.openRoseSwipeLibLoaded = true;
    window.openRoseHandlers = window.openRoseHandlers || {};
    window.openRoseActivePointers = window.openRoseActivePointers || {}; // pointerId -> { elementKey, meta }
    window.openRoseActiveTouches = window.openRoseActiveTouches || {};   // touchId -> { elementKey, meta }
    window.openRoseGlobalHandlersInstalled = window.openRoseGlobalHandlersInstalled || false;

    var debug = false; // set to true when testing to get console logs

    function log() {
        if (!debug) return;
        console.log.apply(console, arguments);
    }

    // Sensitivity default (moderate)
    var defaultThreshold = 50;    // px horizontal
    var defaultRestraint = 120;   // px vertical
    var defaultAllowedTime = 600;  // ms

    function installGlobalHandlersIfNeeded() {
        if (window.openRoseGlobalHandlersInstalled) return;
        // Pointer global handlers (handle pointerup/pointercancel regardless of where pointer moved)
        if (window.PointerEvent) {
            document.addEventListener('pointerup', globalPointerUp, { passive: true });
            document.addEventListener('pointercancel', globalPointerCancel, { passive: true });
            document.addEventListener('pointerdown', globalPointerDownCapture, { passive: true, capture: true });
            window.openRoseGlobalHandlersInstalled = true;
            log("swipe: installed global pointer handlers");
        } else {
            // touch fallback
            document.addEventListener('touchend', globalTouchEnd, { passive: true });
            document.addEventListener('touchcancel', globalTouchCancel, { passive: true });
            // Also capture touchstart at document capture phase to ensure we see starts that might not reach elements (rare)
            document.addEventListener('touchstart', globalTouchStartCapture, { passive: true, capture: true });
            window.openRoseGlobalHandlersInstalled = true;
            log("swipe: installed global touch handlers");
        }
    }

    // Helper: remove global handlers (rarely needed)
    function removeGlobalHandlers() {
        if (!window.openRoseGlobalHandlersInstalled) return;
        if (window.PointerEvent) {
            document.removeEventListener('pointerup', globalPointerUp, { passive: true });
            document.removeEventListener('pointercancel', globalPointerCancel, { passive: true });
            document.removeEventListener('pointerdown', globalPointerDownCapture, { passive: true, capture: true });
        } else {
            document.removeEventListener('touchend', globalTouchEnd, { passive: true });
            document.removeEventListener('touchcancel', globalTouchCancel, { passive: true });
            document.removeEventListener('touchstart', globalTouchStartCapture, { passive: true, capture: true });
        }
        window.openRoseGlobalHandlersInstalled = false;
    }

    // Global capture to detect pointerdown that may not bubble (fallback); we don't use it to start gesture processing here.
    function globalPointerDownCapture(ev) {
        // noop for now, but installed to ensure pointer events are possible in some environments
    }
    function globalTouchStartCapture(ev) {
        // noop
    }

    // Global pointer end handlers
    function globalPointerUp(ev) {
        try {
            var pid = ev.pointerId;
            var rec = window.openRoseActivePointers[pid];
            if (!rec) return;
            // rec: { elementKey, startX, startY, startTime, threshold, restraint, allowedTime, dotNetRef }
            // compute deltas
            var dx = ev.clientX - rec.startX;
            var dy = ev.clientY - rec.startY;
            var dt = Date.now() - rec.startTime;

            log("swipe: globalPointerUp", pid, "dx=", dx, "dy=", dy, "dt=", dt);

            // cleanup
            delete window.openRoseActivePointers[pid];

            if (dt <= rec.allowedTime && Math.abs(dx) >= rec.threshold && Math.abs(dy) <= rec.restraint) {
                if (dx < 0) {
                    rec.dotNetRef.invokeMethodAsync('OnSwipe', 'left').catch(function () { /* ignore */ });
                    log("swipe: invoked left for element", rec.elementKey);
                } else {
                    rec.dotNetRef.invokeMethodAsync('OnSwipe', 'right').catch(function () { /* ignore */ });
                    log("swipe: invoked right for element", rec.elementKey);
                }
            }
        } catch (e) {
            // ignore
            log("swipe: globalPointerUp error", e);
        }
    }

    function globalPointerCancel(ev) {
        try {
            var pid = ev.pointerId;
            if (window.openRoseActivePointers[pid]) {
                delete window.openRoseActivePointers[pid];
                log("swipe: pointer cancel cleaned", pid);
            }
        } catch (e) { /* ignore */ }
    }

    // Global touch end handlers
    function globalTouchEnd(ev) {
        try {
            if (!ev.changedTouches) return;
            for (var i = 0; i < ev.changedTouches.length; i++) {
                var t = ev.changedTouches[i];
                var touchId = t.identifier;
                var rec = window.openRoseActiveTouches[touchId];
                if (!rec) continue;
                var dx = t.clientX - rec.startX;
                var dy = t.clientY - rec.startY;
                var dt = Date.now() - rec.startTime;

                log("swipe: globalTouchEnd id=", touchId, "dx=", dx, "dy=", dy, "dt=", dt);

                delete window.openRoseActiveTouches[touchId];

                if (dt <= rec.allowedTime && Math.abs(dx) >= rec.threshold && Math.abs(dy) <= rec.restraint) {
                    if (dx < 0) {
                        rec.dotNetRef.invokeMethodAsync('OnSwipe', 'left').catch(function () { /* ignore */ });
                        log("swipe: invoked left for element", rec.elementKey);
                    } else {
                        rec.dotNetRef.invokeMethodAsync('OnSwipe', 'right').catch(function () { /* ignore */ });
                        log("swipe: invoked right for element", rec.elementKey);
                    }
                }
            }
        } catch (e) {
            log("swipe: globalTouchEnd error", e);
        }
    }

    function globalTouchCancel(ev) {
        try {
            if (!ev.changedTouches) return;
            for (var i = 0; i < ev.changedTouches.length; i++) {
                var t = ev.changedTouches[i];
                var touchId = t.identifier;
                if (window.openRoseActiveTouches[touchId]) {
                    delete window.openRoseActiveTouches[touchId];
                }
            }
            log("swipe: globalTouchCancel cleaned");
        } catch (e) { /* ignore */ }
    }

    // Register an element to start tracking gestures when start occurs inside element.
    function registerOnElement(el, dotNetRef, elementKey, opts) {
        if (!el) return false;

        // per-element parameters (use defaults unless overridden)
        var threshold = (opts && opts.threshold) || defaultThreshold;
        var restraint = (opts && opts.restraint) || defaultRestraint;
        var allowedTime = (opts && opts.allowedTime) || defaultAllowedTime;

        // Pointerstart handler attached to element (bubble phase)
        function elementPointerDown(ev) {
            try {
                // Only consider primary buttons for mouse
                if (ev.pointerType === 'mouse' && ev.button !== 0) return;

                // Only start if the gesture begins inside this element (we are on element so OK)
                // Ignore multi-touch: if there is already a pointer tracked with a different pointerId for this browser,
                // we still treat each pointer independently but we will only process the first for a given pointerId.
                var pid = ev.pointerId;

                // store meta for this pointerId keyed globally
                window.openRoseActivePointers[pid] = {
                    elementKey: elementKey,
                    startX: ev.clientX,
                    startY: ev.clientY,
                    startTime: Date.now(),
                    threshold: threshold,
                    restraint: restraint,
                    allowedTime: allowedTime,
                    dotNetRef: dotNetRef
                };

                log("swipe: elementPointerDown registered for pid", pid, "element", elementKey);
            } catch (e) {
                log("swipe: elementPointerDown error", e);
            }
        }

        function elementPointerUp(ev) {
            // We purposely avoid processing up here, global handler will do it.
        }

        function elementPointerCancel(ev) {
            try {
                var pid = ev.pointerId;
                if (window.openRoseActivePointers[pid]) {
                    delete window.openRoseActivePointers[pid];
                    log("swipe: elementPointerCancel cleaned", pid);
                }
            } catch (e) { /* ignore */ }
        }

        // Touchstart on element -> store touch meta keyed by touch.identifier
        function elementTouchStart(ev) {
            try {
                if (!ev || !ev.touches) return;
                if (ev.touches.length !== 1) {
                    // multi-touch start: mark nothing for swipe
                    return;
                }
                var t = ev.touches[0];
                window.openRoseActiveTouches[t.identifier] = {
                    elementKey: elementKey,
                    startX: t.clientX,
                    startY: t.clientY,
                    startTime: Date.now(),
                    threshold: threshold,
                    restraint: restraint,
                    allowedTime: allowedTime,
                    dotNetRef: dotNetRef
                };
                log("swipe: elementTouchStart registered id", t.identifier, "element", elementKey);
            } catch (e) { log("swipe: elementTouchStart error", e); }
        }

        function elementTouchEnd(ev) {
            // global handler will process
        }

        function elementTouchCancel(ev) {
            try {
                if (!ev.changedTouches) return;
                for (var i = 0; i < ev.changedTouches.length; i++) {
                    var t = ev.changedTouches[i];
                    if (window.openRoseActiveTouches[t.identifier]) {
                        delete window.openRoseActiveTouches[t.identifier];
                    }
                }
            } catch (e) { /* ignore */ }
        }

        // Attach element-level listeners (bubble phase)
        var handlers = {};

        if (window.PointerEvent) {
            handlers.pointerdown = elementPointerDown;
            handlers.pointerup = elementPointerUp;
            handlers.pointercancel = elementPointerCancel;

            el.addEventListener('pointerdown', handlers.pointerdown, { passive: true });
            // pointerup on element not required; global will handle actual up event
            el.addEventListener('pointercancel', handlers.pointercancel, { passive: true });

            // store handler refs for removal later
            window.openRoseHandlers[elementKey] = {
                el: el,
                mode: 'pointer',
                handlers: handlers,
                dotNetRef: dotNetRef
            };

            log("swipe: registered element-level pointer handlers for", elementKey);
        } else {
            // touch/mouse fallback
            handlers.touchstart = elementTouchStart;
            handlers.touchend = elementTouchEnd;
            handlers.touchcancel = elementTouchCancel;
            handlers.mousedown = elementPointerDown; // treat mouse down similarly
            handlers.mouseup = elementPointerUp;

            el.addEventListener('touchstart', handlers.touchstart, { passive: true });
            el.addEventListener('touchcancel', handlers.touchcancel, { passive: true });
            el.addEventListener('mousedown', handlers.mousedown, { passive: true });

            window.openRoseHandlers[elementKey] = {
                el: el,
                mode: 'touchmouse',
                handlers: handlers,
                dotNetRef: dotNetRef
            };
            log("swipe: registered element-level touch/mouse handlers for", elementKey);
        }

        // ensure globals are installed (do once)
        installGlobalHandlersIfNeeded();

        return true;
    }

    // Public API: register/unregister functions expected by Blazor
    window.openRoseRegisterSwipeElement = function (element, dotNetRef, options) {
        try {
            if (!element) return false;
            var elementKey = element.dataset && element.dataset.openroseId ? element.dataset.openroseId : ("el-" + Math.random().toString(36).substr(2, 9));
            try { element.dataset.openroseId = elementKey; } catch (e) { /* ignore */ }
            return registerOnElement(element, dotNetRef, elementKey, options || {});
        } catch (err) {
            log("swipe: openRoseRegisterSwipeElement error", err);
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

            var mode = rec.mode;
            var h = rec.handlers || {};
            try {
                if (mode === 'pointer') {
                    element.removeEventListener('pointerdown', h.pointerdown);
                    element.removeEventListener('pointercancel', h.pointercancel);
                } else {
                    element.removeEventListener('touchstart', h.touchstart);
                    element.removeEventListener('touchcancel', h.touchcancel);
                    element.removeEventListener('mousedown', h.mousedown);
                }
            } catch (er) { /* ignore */ }

            // cleanup any active pointers/touches associated with this element
            for (var pid in window.openRoseActivePointers) {
                if (window.openRoseActivePointers[pid] && window.openRoseActivePointers[pid].elementKey === elementKey) {
                    delete window.openRoseActivePointers[pid];
                }
            }
            for (var tid in window.openRoseActiveTouches) {
                if (window.openRoseActiveTouches[tid] && window.openRoseActiveTouches[tid].elementKey === elementKey) {
                    delete window.openRoseActiveTouches[tid];
                }
            }

            delete window.openRoseHandlers[elementKey];
            try { delete element.dataset.openroseId; } catch (e) { }
            try { delete element.__swipeTouchMeta; } catch (e) { }
            log("swipe: unregistered element", elementKey);
            return true;
        } catch (err) {
            log("swipe: openRoseUnregisterSwipeElement error", err);
            return false;
        }
    };
})();