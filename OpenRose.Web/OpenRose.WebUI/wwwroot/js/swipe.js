/*
 * OpenRose - Requirements Management
    * Licensed under the Apache License, Version 2.0.
 * See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.
*/


/*
 * Touch-only swipe detection for OpenRose read-only views.
 *
 * Purpose:
 *  - Detect single-finger left/right swipe gestures on touch devices
 *    and call the Blazor callback dotNetRef.OnSwipe('left'|'right').
 *  - Preserve multi-finger gestures (pinch/zoom) and normal mouse clicks.
 *  - Avoid global style changes; only temporarily install a non-passive
 *    touchmove handler during an active single-finger gesture and call
 *    preventDefault only once horizontal motion is clearly detected.
 *
 * Usage:
 *  - Blazor components call `openRoseRegisterSwipeElement(element, dotNetRef)`
 *    passing the ElementReference and a DotNetObjectReference for callbacks.
 *  - Unregister with `openRoseUnregisterSwipeElement(element)` on dispose.
 *
 * Notes / tradeoffs:
 *  - Edge swipes initiated at the very device edge may still be handled by the OS/browser.
 *  - If a single-finger gesture begins and we already prevented default, then adding a second finger
 *    mid-gesture may not immediately recover browser pinch support until fingers are lifted and a new gesture starts.
 *  - Keep debug=false in production.
 */

(function () {
    if (window.openRoseSwipeLibLoaded) return;
    window.openRoseSwipeLibLoaded = true;
    window.openRoseHandlers = window.openRoseHandlers || {};

    // Toggle for development diagnostics; set to `true` to capture remote device console logs.
    var debug = false;

    function dlog() {
        if (!debug) return;
        try { console.log.apply(console, arguments); } catch (_) { }
    }

    // Tuning parameters (moderate defaults)
    var DEFAULT_THRESHOLD = 50;    // px horizontal movement required on touchend to count as swipe
    var DEFAULT_RESTRAINT = 120;   // px max vertical movement allowed
    var DEFAULT_ALLOWED_TIME = 600; // ms allowed for the swipe duration
    var EARLY_PREVENT_THRESHOLD = 30; // px horizontal movement on touchmove before we call preventDefault

    // Register a DOM element for touch swipe detection
    function registerOnElement(el, dotNetRef, elementKey, opts) {
        if (!el) return false;

        // Merge per-element options or fall back to defaults
        var threshold = (opts && opts.threshold) || DEFAULT_THRESHOLD;
        var restraint = (opts && opts.restraint) || DEFAULT_RESTRAINT;
        var allowedTime = (opts && opts.allowedTime) || DEFAULT_ALLOWED_TIME;
        var earlyPrevent = (opts && opts.earlyPrevent) || EARLY_PREVENT_THRESHOLD;

        // State for the active single-touch being tracked (if any)
        var active = null; // { id, startX, startY, startTime, prevented }
        var docMoveHandler = null; // reference to the installed document touchmove handler

        // Cleanup helper removes the document move handler and resets state
        function cleanup() {
            if (docMoveHandler) {
                try { document.removeEventListener('touchmove', docMoveHandler, { capture: true }); } catch (e) { /* ignore */ }
                docMoveHandler = null;
            }
            active = null;
        }

        // Element-level touchstart: start tracking only for single-finger touches
        function onTouchStart(ev) {
            try {
                if (!ev || !ev.touches) return;
                if (ev.touches.length !== 1) { active = null; return; } // multi-touch → ignore

                var t = ev.touches[0];
                active = {
                    id: t.identifier,
                    startX: t.clientX,
                    startY: t.clientY,
                    startTime: Date.now(),
                    prevented: false
                };

                dlog("swipe: touchstart id", active.id, "element", elementKey);

                // Install a non-passive document touchmove so we can call preventDefault when the gesture is clearly horizontal.
                // This prevents browser history navigation for single-finger swipes in many browsers;
                // we call preventDefault only after earlyPrevent threshold and only when horizontal motion dominates.
                docMoveHandler = function (moveEv) {
                    try {
                        if (!active) return;

                        // If multi-touch appears while moving, cancel our handling immediately and let browser take over (pinch preserved).
                        if (moveEv.touches && moveEv.touches.length > 1) {
                            dlog("swipe: multi-touch detected -> cancel custom handling", elementKey);
                            cleanup();
                            return;
                        }

                        // Find the tracked touch in the current touches
                        var found = null;
                        for (var i = 0; i < moveEv.touches.length; i++) {
                            if (moveEv.touches[i].identifier === active.id) { found = moveEv.touches[i]; break; }
                        }
                        if (!found) return;

                        var dx = found.clientX - active.startX;
                        var dy = found.clientY - active.startY;
                        var absDx = Math.abs(dx), absDy = Math.abs(dy);

                        // Call preventDefault only when horizontal motion is clearly dominant and exceeds earlyPrevent.
                        if (!active.prevented && absDx > earlyPrevent && absDx > absDy) {
                            try {
                                moveEv.preventDefault();
                                active.prevented = true;
                                dlog("swipe: prevented default on touchmove (element)", elementKey);
                            } catch (e) { /* ignore */ }
                        } else if (active.prevented) {
                            // Keep preventing once we've started preventing
                            try { moveEv.preventDefault(); } catch (e) { /* ignore */ }
                        }
                    } catch (e) {
                        // swallow to avoid breaking the page
                    }
                };

                document.addEventListener('touchmove', docMoveHandler, { passive: false, capture: true });
            } catch (ex) {
                dlog("swipe: onTouchStart error", ex);
                cleanup();
            }
        }

        // Element-level touchend: compute deltas and fire swipe if thresholds pass
        function onTouchEnd(ev) {
            try {
                if (!active) { cleanup(); return; }
                if (!ev.changedTouches) { cleanup(); return; }

                var matched = null;
                for (var i = 0; i < ev.changedTouches.length; i++) {
                    if (ev.changedTouches[i].identifier === active.id) { matched = ev.changedTouches[i]; break; }
                }
                if (!matched) { cleanup(); return; }

                var dx = matched.clientX - active.startX;
                var dy = matched.clientY - active.startY;
                var dt = Date.now() - active.startTime;

                dlog("swipe: touchend id", active.id, "dx", dx, "dy", dy, "dt", dt, "prevented", active.prevented);

                cleanup();

                if (dt <= allowedTime && Math.abs(dx) >= threshold && Math.abs(dy) <= restraint) {
                    if (dx < 0) {
                        dotNetRef.invokeMethodAsync('OnSwipe', 'left').catch(function () { /* ignore */ });
                        dlog("swipe: invoked left for", elementKey);
                    } else {
                        dotNetRef.invokeMethodAsync('OnSwipe', 'right').catch(function () { /* ignore */ });
                        dlog("swipe: invoked right for", elementKey);
                    }
                }
            } catch (ex) {
                dlog("swipe: onTouchEnd error", ex);
                cleanup();
            }
        }

        // Element-level touchcancel: abort tracking
        function onTouchCancel(ev) {
            try {
                dlog("swipe: touchcancel", elementKey);
                cleanup();
            } catch (e) { /* ignore */ }
        }

        // Attach element-level touch listeners.
        // We do not attach pointer/mouse handlers here — mouse behavior remains unchanged.
        try {
            el.addEventListener('touchstart', onTouchStart, { passive: false });
            el.addEventListener('touchend', onTouchEnd, { passive: true });
            el.addEventListener('touchcancel', onTouchCancel, { passive: true });
            dlog("swipe: attached touch handlers for element", elementKey);
        } catch (e) {
            // Fallback for older browsers
            el.addEventListener('touchstart', onTouchStart);
            el.addEventListener('touchend', onTouchEnd);
            el.addEventListener('touchcancel', onTouchCancel);
        }

        // Store handler references to support unregister
        window.openRoseHandlers[elementKey] = {
            el: el,
            handlers: {
                onTouchStart: onTouchStart,
                onTouchEnd: onTouchEnd,
                onTouchCancel: onTouchCancel
            },
            dotNetRef: dotNetRef
        };
        return true;
    }

    // Remove attached listeners and cleanup references
    function unregisterElement(element) {
        try {
            if (!element) return false;
            var elementKey = element.dataset && element.dataset.openroseId ? element.dataset.openroseId : null;
            if (!elementKey) return false;
            var rec = window.openRoseHandlers[elementKey];
            if (!rec) return false;
            var el = rec.el, h = rec.handlers || {};
            try {
                el.removeEventListener('touchstart', h.onTouchStart, { passive: false });
                el.removeEventListener('touchend', h.onTouchEnd, { passive: true });
                el.removeEventListener('touchcancel', h.onTouchCancel, { passive: true });
            } catch (e) {
                // Greaceful fallback removal for browsers that don't honor options on removeEventListener
                try { el.removeEventListener('touchstart', h.onTouchStart); el.removeEventListener('touchend', h.onTouchEnd); el.removeEventListener('touchcancel', h.onTouchCancel); } catch (_) { }
            }
            delete window.openRoseHandlers[elementKey];
            try { delete element.dataset.openroseId; } catch (_) { }
            dlog("swipe: unregistered element", elementKey);
            return true;
        } catch (err) {
            dlog("swipe: unregister error", err);
            return false;
        }
    }

    // Blazor-callable registration/unregistration
    window.openRoseRegisterSwipeElement = function (element, dotNetRef, options) {
        try {
            if (!element) return false;
            var elementKey = element.dataset && element.dataset.openroseId ? element.dataset.openroseId : ("el-" + Math.random().toString(36).substr(2, 9));
            try { element.dataset.openroseId = elementKey; } catch (e) { /* ignore */ }
            var ok = registerOnElement(element, dotNetRef, elementKey, options || {});
            dlog("swipe: register result for", elementKey, ok);
            return ok;
        } catch (err) {
            dlog("swipe: register error", err);
            return false;
        }
    };

    window.openRoseUnregisterSwipeElement = function (element) {
        try {
            return unregisterElement(element);
        } catch (err) {
            dlog("swipe: unregister error", err);
            return false;
        }
    };
})();