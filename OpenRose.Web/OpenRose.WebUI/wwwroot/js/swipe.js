/*
 * OpenRose - Requirements Management
    * Licensed under the Apache License, Version 2.0.
 * See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.
*/

// Touch-only swipe detection (single-finger).
// - DOES NOT change touch-action or overscroll styles.
// - Dynamically installs non-passive touchmove listener and only calls preventDefault
//   when single-finger horizontal gesture is clearly detected.
// - Cancels custom handling immediately when a second touch appears.
// - Moderate sensitivity: threshold=50px swipe, earlyPrevent=30px to start preventing.
// - Set debug = true for diagnostic logs.

(function () {
    if (window.openRoseSwipeLibLoaded) return;
    window.openRoseSwipeLibLoaded = true;
    window.openRoseHandlers = window.openRoseHandlers || {};

    var debug = true; // set to true to enable console logs on device during testing

    function dlog() {
        if (!debug) return;
        try { console.log.apply(console, arguments); } catch (_) { }
    }

    var DEFAULT_THRESHOLD = 50;    // px horizontal movement required to count as a swipe on end
    var DEFAULT_RESTRAINT = 120;   // px max vertical movement allowed for a swipe
    var DEFAULT_ALLOWED_TIME = 600; // ms allowed duration for the swipe
    var EARLY_PREVENT_THRESHOLD = 30; // px horizontal movement before we call preventDefault on touchmove

    function registerOnElement(el, dotNetRef, elementKey, opts) {
        if (!el) return false;

        var threshold = (opts && opts.threshold) || DEFAULT_THRESHOLD;
        var restraint = (opts && opts.restraint) || DEFAULT_RESTRAINT;
        var allowedTime = (opts && opts.allowedTime) || DEFAULT_ALLOWED_TIME;
        var earlyPrevent = (opts && opts.earlyPrevent) || EARLY_PREVENT_THRESHOLD;

        // Track a single active touch for this element
        var active = null; // { id, startX, startY, startTime, prevented }
        var docMoveHandler = null;

        function cleanup() {
            if (docMoveHandler) {
                try { document.removeEventListener('touchmove', docMoveHandler, { capture: true }); } catch (e) { /* ignore */ }
                docMoveHandler = null;
            }
            active = null;
        }

        function onTouchStart(ev) {
            try {
                if (!ev || !ev.touches) return;
                // Start tracking only if exactly one touch is present
                if (ev.touches.length !== 1) {
                    active = null;
                    return;
                }
                var t = ev.touches[0];
                active = {
                    id: t.identifier,
                    startX: t.clientX,
                    startY: t.clientY,
                    startTime: Date.now(),
                    prevented: false
                };

                dlog("swipe: touchstart id", active.id, "element", elementKey);

                // Install non-passive document touchmove so we can call preventDefault when needed
                docMoveHandler = function (moveEv) {
                    try {
                        if (!active) return;

                        // If multi-touch starts, cancel our swipe tracking and allow browser to handle it
                        if (moveEv.touches && moveEv.touches.length > 1) {
                            dlog("swipe: multi-touch detected -> cancel custom handling", elementKey);
                            cleanup();
                            return;
                        }

                        // find tracked touch
                        var found = null;
                        for (var i = 0; i < moveEv.touches.length; i++) {
                            if (moveEv.touches[i].identifier === active.id) { found = moveEv.touches[i]; break; }
                        }
                        if (!found) return;

                        var dx = found.clientX - active.startX;
                        var dy = found.clientY - active.startY;
                        var absDx = Math.abs(dx), absDy = Math.abs(dy);

                        // Only prevent default when movement is clearly horizontal and exceeds earlyPrevent
                        if (!active.prevented && absDx > earlyPrevent && absDx > absDy) {
                            try {
                                moveEv.preventDefault(); // prevents browser history navigation in many browsers
                                active.prevented = true;
                                dlog("swipe: prevented default on touchmove (element)", elementKey);
                            } catch (e) {
                                // ignore errors from preventDefault if any
                            }
                        } else if (active.prevented) {
                            // Continue to prevent if we've already started preventing
                            try { moveEv.preventDefault(); } catch (e) { /* ignore */ }
                        }
                    } catch (e) {
                        // ignore
                    }
                };

                document.addEventListener('touchmove', docMoveHandler, { passive: false, capture: true });
            } catch (ex) {
                dlog("swipe: onTouchStart error", ex);
                cleanup();
            }
        }

        function onTouchEnd(ev) {
            try {
                if (!active) {
                    cleanup();
                    return;
                }

                if (!ev.changedTouches) {
                    cleanup();
                    return;
                }

                var matched = null;
                for (var i = 0; i < ev.changedTouches.length; i++) {
                    if (ev.changedTouches[i].identifier === active.id) { matched = ev.changedTouches[i]; break; }
                }

                if (!matched) {
                    // Not the tracked touch; just cleanup
                    cleanup();
                    return;
                }

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

        function onTouchCancel(ev) {
            try {
                dlog("swipe: touchcancel", elementKey);
                cleanup();
            } catch (e) { /* ignore */ }
        }

        // Attach touch handlers to the element (no mouse/pointer handlers)
        try {
            el.addEventListener('touchstart', onTouchStart, { passive: false });
            el.addEventListener('touchend', onTouchEnd, { passive: true });
            el.addEventListener('touchcancel', onTouchCancel, { passive: true });
            dlog("swipe: attached touch handlers for element", elementKey);
        } catch (e) {
            // older browsers fallback
            el.addEventListener('touchstart', onTouchStart);
            el.addEventListener('touchend', onTouchEnd);
            el.addEventListener('touchcancel', onTouchCancel);
        }

        window.openRoseHandlers[elementKey] = { el: el, handlers: { onTouchStart: onTouchStart, onTouchEnd: onTouchEnd, onTouchCancel: onTouchCancel }, dotNetRef: dotNetRef };
        return true;
    }

    function unregisterElement(element) {
        try {
            if (!element) return false;
            var elementKey = element.dataset && element.dataset.openroseId ? element.dataset.openroseId : null;
            if (!elementKey) return false;
            var rec = window.openRoseHandlers[elementKey];
            if (!rec) return false;
            var el = rec.el;
            var h = rec.handlers || {};
            try {
                el.removeEventListener('touchstart', h.onTouchStart, { passive: false });
                el.removeEventListener('touchend', h.onTouchEnd, { passive: true });
                el.removeEventListener('touchcancel', h.onTouchCancel, { passive: true });
            } catch (e) {
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